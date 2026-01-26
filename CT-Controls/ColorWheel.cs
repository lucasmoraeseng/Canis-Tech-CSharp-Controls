using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CT_Controls
{
    public partial class ColorWheel : UserControl
    {
        Bitmap _bitmap;
        bool _mouseDown;
        Point _lastPos;
        // Hue 0..360, Saturation 0..1 (radial)
        public float Hue { get; set; } = 0f;
        public float Saturation { get; set; } = 0f;

        // Fired when user changes hue/saturation via mouse.
        public event Action<float, float> ColorChanged;

        public ColorWheel()
        {
            DoubleBuffered = true;
            MinimumSize = new Size(64, 64);
            Resize += (s, e) => RecreateBitmap();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_bitmap != null)
            {
                var cx = (Width - _bitmap.Width) / 2;
                var cy = (Height - _bitmap.Height) / 2;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(_bitmap, cx, cy, _bitmap.Width, _bitmap.Height);
            }

            // draw selector
            var center = new PointF(Width / 2f, Height / 2f);
            float radius = Math.Min(Width, Height) * 0.5f * 0.95f;
            float satRadius = Saturation * radius;
            float rad = (float)(Hue * Math.PI / 180.0);
            var selX = center.X + (float)(Math.Cos(rad) * satRadius);
            var selY = center.Y + (float)(Math.Sin(rad) * satRadius);

            using (var pen = new Pen(Color.FromArgb(220, Color.Black), 2f))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawEllipse(pen, selX - 6, selY - 6, 12, 12);
                using (var inner = new Pen(Color.FromArgb(200, Color.White), 1f))
                    e.Graphics.DrawEllipse(inner, selX - 5, selY - 5, 10, 10);
            }
        }

        void RecreateBitmap()
        {
            var size = Math.Min(Width, Height);
            if (size <= 0) return;

            _bitmap?.Dispose();
            _bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var cx = size / 2f;
            var cy = size / 2f;
            var maxR = size * 0.5f * 0.95f; // leave small margin

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (dist > maxR)
                    {
                        _bitmap.SetPixel(x, y, Color.Transparent);
                        continue;
                    }
                    // angle to hue
                    float angle = (float)(Math.Atan2(dy, dx)); // -pi..pi
                    float hue = angle * 180f / (float)Math.PI;
                    if (hue < 0) hue += 360f;
                    float saturation = Math.Min(1f, dist / maxR);
                    // value fixed at 1
                    var c = HSVToRGB(hue, saturation, 1f);
                    _bitmap.SetPixel(x, y, c);
                }
            }

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _mouseDown = true;
            Capture = true;
            UpdateFromPoint(e.Location);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseDown)
            {
                UpdateFromPoint(e.Location);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _mouseDown = false;
            Capture = false;
        }

        void UpdateFromPoint(Point p)
        {
            var center = new PointF(Width / 2f, Height / 2f);
            float dx = p.X - center.X;
            float dy = p.Y - center.Y;
            float angle = (float)Math.Atan2(dy, dx); // -pi..pi
            float hue = angle * 180f / (float)Math.PI;
            if (hue < 0) hue += 360f;
            float radius = (float)Math.Sqrt(dx * dx + dy * dy);
            float maxR = Math.Min(Width, Height) * 0.5f * 0.95f;
            float sat = Math.Max(0f, Math.Min(1f, radius / maxR));

            Hue = hue;
            Saturation = sat;
            ColorChanged?.Invoke(Hue, Saturation);
            Invalidate();
        }

        static Color HSVToRGB(float h, float s, float v)
        {
            if (s <= 0f) return Color.FromArgb(
                Clamp((int)(v * 255)),
                Clamp((int)(v * 255)),
                Clamp((int)(v * 255)));

            h = h % 360f;
            if (h < 0) h += 360f;
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
            return Color.FromArgb(Clamp((int)(r * 255f)), Clamp((int)(g * 255f)), Clamp((int)(b * 255f)));
        }

        static int Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }
    }
}