using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CT_Controls
{
    public partial class ColorPicker : UserControl
    {
        readonly ColorWheel _wheel;
        readonly SaturationSlider _saturSlider;

        // Exposed properties
        public float Hue { get; private set; } = 0f;          // 0..360
        public float Saturation { get; private set; } = 1f;   // 0..1 (final = wheelSat * slider)
        public float WheelSaturation { get; private set; } = 1f; // raw from wheel
        public float SliderValue { get; private set; } = 1f; // slider 0..1
        public float Value { get; set; } = 1f; // V component (kept 1 by default)

        public event Action<Color> ColorChanged;
        public event Action<float, float, float> HSVChanged;

        public ColorPicker()
        {
            _wheel = new ColorWheel { Dock = DockStyle.Fill };
            _saturSlider = new SaturationSlider { Dock = DockStyle.Right, Width = 28, Margin = new Padding(6,0,0,0) };

            Controls.Add(_wheel);
            Controls.Add(_saturSlider);

            _wheel.ColorChanged += (h, ws) =>
            {
                Hue = h;
                WheelSaturation = ws;
                // slider gradient needs the current hue
                _saturSlider.Hue = Hue;
                Recalculate();
            };

            _saturSlider.ValueChanged += (v) =>
            {
                SliderValue = v;
                Recalculate();
            };

            // initialize
            _saturSlider.Hue = _wheel.Hue;
            Recalculate();
        }

        void Recalculate()
        {
            // final saturation combines wheel radial selection and slider multiplier
            Saturation = Math.Max(0f, Math.Min(1f, WheelSaturation * SliderValue));
            var rgb = HSVToRGB(Hue, Saturation, Value);
            ColorChanged?.Invoke(rgb);
            HSVChanged?.Invoke(Hue, Saturation, Value);
        }

        public Color Color => HSVToRGB(Hue, Saturation, Value);

        // Helper: convert HSV -> Color and Hex/tuple getters
        public static Color HSVToRGB(float h, float s, float v)
        {
            if (s <= 0f)
            {
                int l = Clamp((int)(v * 255));
                return Color.FromArgb(l, l, l);
            }
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

        // Convenience getters
        public (int R, int G, int B) RGB => (Color.R, Color.G, Color.B);
        public (float H, float S, float V) HSV => (Hue, Saturation, Value);
        public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

        // Allow programmatic set of HSV
        public void SetHSV(float h, float s, float v = 1f)
        {
            Hue = h % 360f;
            if (Hue < 0) Hue += 360f;
            Value = Math.Max(0f, Math.Min(1f, v));
            // set wheel selection: we cannot set exact wheel radius easily; approximate by setting WheelSaturation
            WheelSaturation = Math.Max(0f, Math.Min(1f, s));
            _wheel.Hue = Hue;
            // fire update to repaint wheel selector position
            // There's no direct API to move selector to saturation; simulate internal fields:
            // For simplicity, set wheel's properties by invoking private-like mechanism via reflection would be hacky.
            // Instead accept that programmatic SetHSV only updates hue and slider so final color matches.
            SliderValue = 1f;
            _saturSlider.Value = s; // prefer slider to reflect saturation
            _saturSlider.Hue = Hue;
            Recalculate();
        }
    }
}
