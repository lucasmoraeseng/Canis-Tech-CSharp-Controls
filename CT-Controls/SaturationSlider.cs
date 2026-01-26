using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CT_Controls
{
    public enum SliderCursorSize
    {
        Small,
        Large
    }

    [ToolboxItem(true)]
    [DefaultEvent(nameof(ValueChanged))]
    [DefaultProperty(nameof(Value))]
    public partial class SaturationSlider : UserControl
    {
        private float _value = 1f; // 0..1
        private float _hue = 0f;   // 0..360
        private Orientation _orientation = Orientation.Vertical;

        private Color _cursorColor = Color.Black;
        private SliderCursorSize _cursorSize = SliderCursorSize.Small;

        public SaturationSlider()
        {
            InitializeComponent();

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            DoubleBuffered = true;

            MinimumSize = new Size(20, 40);
            SetStyle(ControlStyles.Selectable, true);
        }

        // =========================
        // Properties
        // =========================

        [Category("Behavior")]
        [Description("Saturation value (0.0 = black, 1.0 = full color).")]
        [DefaultValue(1f)]
        public float Value
        {
            get => _value;
            set
            {
                float v = Math.Max(0f, Math.Min(1f, value));
                if (Math.Abs(_value - v) < float.Epsilon)
                    return;

                _value = v;
                OnValueChanged();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Hue used to render the saturation gradient (0 to 360 degrees).")]
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
                Invalidate();
            }
        }

        [Category("Layout")]
        [Description("Orientation of the saturation slider.")]
        [DefaultValue(Orientation.Vertical)]
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value)
                    return;

                _orientation = value;
                MinimumSize = (_orientation == Orientation.Vertical)
                    ? new Size(20, 40)
                    : new Size(40, 20);

                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Color of the slider cursor.")]
        [DefaultValue(typeof(Color), "Black")]
        public Color CursorColor
        {
            get => _cursorColor;
            set
            {
                if (_cursorColor == value)
                    return;

                _cursorColor = value;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Size style of the slider cursor.")]
        [DefaultValue(SliderCursorSize.Small)]
        public SliderCursorSize CursorSize
        {
            get => _cursorSize;
            set
            {
                if (_cursorSize == value)
                    return;

                _cursorSize = value;
                Invalidate();
            }
        }

        // =========================
        // Event
        // =========================

        [Category("Property Changed")]
        [Description("Occurs when the saturation value changes.")]
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        // =========================
        // Mouse handling
        // =========================

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Capture = true;
            UpdateFromPoint(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Capture)
                UpdateFromPoint(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            Capture = false;
        }

        private void UpdateFromPoint(Point p)
        {
            if (_orientation == Orientation.Vertical)
            {
                float v = 1f - (p.Y / (float)Math.Max(1, Height));
                Value = v;
            }
            else
            {
                float v = p.X / (float)Math.Max(1, Width);
                Value = v;
            }
        }

        // =========================
        // Painting
        // =========================

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            // Gradient: black -> full saturation color
            Color fullColor = HSVToRGB(_hue, 1f, 1f);
            LinearGradientMode mode =
                (_orientation == Orientation.Vertical)
                    ? LinearGradientMode.Vertical
                    : LinearGradientMode.Horizontal;

            using (var brush = new LinearGradientBrush(rect, Color.Black, fullColor, mode))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            DrawCursor(e.Graphics);
        }

        private void DrawCursor(Graphics g)
        {
            using (var pen = new Pen(_cursorColor, 2f))
            {
                if (_orientation == Orientation.Vertical)
                {
                    float y = (1f - _value) * Height;

                    if (_cursorSize == SliderCursorSize.Small)
                    {
                        g.DrawRectangle(pen, 0, y - 3, Width - 1, 6);
                    }
                    else // Large
                    {
                        g.DrawRectangle(pen, -2, y - 5, Width + 3, 10);
                    }
                }
                else
                {
                    float x = _value * Width;

                    if (_cursorSize == SliderCursorSize.Small)
                    {
                        g.DrawRectangle(pen, x - 3, 0, 6, Height - 1);
                    }
                    else // Large
                    {
                        g.DrawRectangle(pen, x - 5, -2, 10, Height + 3);
                    }
                }
            }
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
