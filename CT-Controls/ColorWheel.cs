using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CT_Controls
{
    [ToolboxItem(true)]
    [DefaultEvent(nameof(ColorChanged))]
    [DefaultProperty(nameof(SelectedColor))]
    public partial class ColorWheel : UserControl
    {
        private Bitmap _bitmap;
        private bool _mouseDown;

        private float _hue = 0f;        // 0..360
        private float _saturation = 0f; // 0..1

        private const float CENTER_DEADZONE_RATIO = 0.03f;

        public ColorWheel()
        {
            InitializeComponent();

            // IMPORTANT: real transparency in WinForms
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;

            MinimumSize = new Size(64, 64);
            Resize += (s, e) => RecreateBitmap();
        }

        // =========================
        // Properties
        // =========================

        [Category("Color")]
        [Description("Hue component of the color (0 to 360 degrees).")]
        [DefaultValue(0f)]
        public float Hue
        {
            get => _hue;
            set
            {
                float v = value % 360f;
                if (v < 0) v += 360f;

                if (Math.Abs(_hue - v) < float.Epsilon)
                    return;

                _hue = v;
                OnColorChanged();
                Invalidate();
            }
        }

        [Category("Color")]
        [Description("Saturation level of the color (0.0 to 1.0).")]
        [DefaultValue(0f)]
        public float Saturation
        {
            get => _saturation;
            set
            {
                float v = Math.Max(0f, Math.Min(1f, value));

                if (Math.Abs(_saturation - v) < float.Epsilon)
                    return;

                _saturation = v;
                OnColorChanged();
                Invalidate();
            }
        }

        [Category("Color")]
        [Description("Currently selected color.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color SelectedColor
        {
            get => HSVToRGB(Hue, Saturation, 1f);
        }

        // =========================
        // Event
        // =========================

        [Category("Property Changed")]
        [Description("Occurs when the selected color changes.")]
        public event EventHandler ColorChanged;

        protected virtual void OnColorChanged()
        {
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        // =========================
        // Painting
        // =========================

        protected override void OnPaint(PaintEventArgs e)
        {
            // DO NOT call base.OnPaint or OnPaintBackground
            // This prevents WinForms from clearing the background

            if (_bitmap != null)
            {
                int cx = (Width - _bitmap.Width) / 2;
                int cy = (Height - _bitmap.Height) / 2;

                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawImageUnscaled(_bitmap, cx, cy);
            }

            DrawSelector(e.Graphics);
        }

        private void DrawSelector(Graphics g)
        {
            var center = new PointF(Width / 2f, Height / 2f);
            float radius = Math.Min(Width, Height) * 0.5f * 0.95f;
            float satRadius = Saturation * radius;
            float rad = (float)(Hue * Math.PI / 180.0);

            float x = center.X + (float)Math.Cos(rad) * satRadius;
            float y = center.Y + (float)Math.Sin(rad) * satRadius;

            using (var outer = new Pen(Color.FromArgb(220, Color.Black), 2f))
            using (var inner = new Pen(Color.FromArgb(200, Color.White), 1f))
            {
                g.DrawEllipse(outer, x - 6, y - 6, 12, 12);
                g.DrawEllipse(inner, x - 5, y - 5, 10, 10);
            }
        }

        // =========================
        // Mouse
        // =========================

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _mouseDown = true;
            Capture = true;
            UpdateFromPoint(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_mouseDown)
                UpdateFromPoint(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _mouseDown = false;
            Capture = false;
        }

        private void UpdateFromPoint(Point p)
        {
            var center = new PointF(Width / 2f, Height / 2f);

            float dx = p.X - center.X;
            float dy = p.Y - center.Y;

            float angle = (float)Math.Atan2(dy, dx);
            float hue = angle * 180f / (float)Math.PI;
            if (hue < 0) hue += 360f;

            float radius = (float)Math.Sqrt(dx * dx + dy * dy);
            float maxR = Math.Min(Width, Height) * 0.5f * 0.95f;
            float deadzone = maxR * CENTER_DEADZONE_RATIO;

            float sat = radius <= deadzone
                ? 0f
                : Math.Min(1f, radius / maxR);

            Hue = hue;
            Saturation = sat;
        }

        // =========================
        // Bitmap generation
        // =========================

        private void RecreateBitmap()
        {
            int size = Math.Min(Width, Height);
            if (size <= 0) return;

            _bitmap?.Dispose();
            _bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(_bitmap))
            {
                g.Clear(Color.Transparent);
            }

            float cx = size / 2f;
            float cy = size / 2f;
            float maxR = size * 0.5f * 0.95f;
            float deadzone = maxR * CENTER_DEADZONE_RATIO;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist > maxR)
                        continue;

                    float angle = (float)Math.Atan2(dy, dx);
                    float hue = angle * 180f / (float)Math.PI;
                    if (hue < 0) hue += 360f;

                    float sat = dist <= deadzone ? 0f : Math.Min(1f, dist / maxR);
                    Color c = HSVToRGB(hue, sat, 1f);

                    _bitmap.SetPixel(x, y, Color.FromArgb(255, c));
                }
            }

            Invalidate();
        }

        // =========================
        // Helpers
        // =========================

        private static Color HSVToRGB(float h, float s, float v)
        {
            if (s <= 0f)
            {
                int val = Clamp((int)(v * 255));
                return Color.FromArgb(val, val, val);
            }

            h = (h % 360 + 360) % 360;
            float hf = h / 60f;
            int i = (int)Math.Floor(hf);
            float f = hf - i;

            float p = v * (1f - s);
            float q = v * (1f - s * f);
            float t = v * (1f - s * (1f - f));

            float r = 0, g = 0, b = 0;

            switch (i)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return Color.FromArgb(
                Clamp((int)(r * 255)),
                Clamp((int)(g * 255)),
                Clamp((int)(b * 255)));
        }

        private static int Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }
    }
}
