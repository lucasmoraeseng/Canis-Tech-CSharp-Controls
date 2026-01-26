using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CT_Controls
{
    public partial class SaturationSlider : UserControl
    {
        // Value 0..1
        float _value = 1f;
        public float Value
        {
            get => _value;
            set
            {
                var nv = Math.Max(0f, Math.Min(1f, value));
                if (Math.Abs(nv - _value) > 1e-6)
                {
                    _value = nv;
                    ValueChanged?.Invoke(_value);
                    Invalidate();
                }
            }
        }

        // current hue for gradient rendering (0..360)
        float _hue = 0f;
        [Browsable(false)]
        public float Hue
        {
            get => _hue;
            set
            {
                _hue = value % 360f;
                if (_hue < 0) _hue += 360f;
                Invalidate();
            }
        }

        public event Action<float> ValueChanged;

        public SaturationSlider()
        {
            DoubleBuffered = true;
            MinimumSize = new Size(20, 40);
            SetStyle(ControlStyles.Selectable, true);
            MouseDown += (s, e) => { Capture = true; UpdateFromPoint(e.Location); };
            MouseMove += (s, e) => { if (Capture) UpdateFromPoint(e.Location); };
            MouseUp += (s, e) => { Capture = false; };
        }

        void UpdateFromPoint(Point p)
        {
            // vertical slider: top = 0 (white), bottom = 1 (full color)
            float y = p.Y;
            var v = 1f - (y / (float)Height);
            Value = v;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;
            using (var bmp = new Bitmap(Math.Max(1, rect.Width), Math.Max(1, rect.Height)))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    // gradient from white to hue color (top to bottom)
                    Color c = HSVToRGB(_hue, 1f, 1f);
                    using (var lg = new LinearGradientBrush(new Rectangle(0, 0, bmp.Width, bmp.Height),
                        Color.White, c, LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(lg, 0, 0, bmp.Width, bmp.Height);
                    }
                }
                e.Graphics.DrawImage(bmp, rect);
            }

            // Draw handle
            float handleY = (1f - _value) * Height;
            using (var pen = new Pen(Color.Black, 2f))
            {
                e.Graphics.DrawRectangle(pen, 0, handleY - 3, Width - 1, 6);
            }
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