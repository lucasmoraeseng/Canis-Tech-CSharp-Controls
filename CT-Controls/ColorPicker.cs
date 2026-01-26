using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CT_Controls
{
    /// <summary>
    /// Composite color picker control based on a hue/saturation wheel
    /// combined with a saturation slider.
    /// </summary>
    [ToolboxItem(true)]
    [DefaultEvent(nameof(ColorChanged))]
    [DefaultProperty(nameof(Color))]
    public partial class ColorPicker : UserControl
    {
        // Internal controls
        private readonly ColorWheel _wheel;
        private readonly SaturationSlider _saturSlider;

        // ============================================================
        // HSV STATE (read-only from outside)
        // ============================================================

        /// <summary>
        /// Current hue value selected on the color wheel (0 to 360 degrees).
        /// </summary>
        [Category("Color")]
        [Description("Current hue selected on the color wheel (0 to 360 degrees).")]
        [DefaultValue(0f)]
        public float Hue { get; private set; } = 0f;

        /// <summary>
        /// Final saturation value (0.0 to 1.0), calculated as:
        /// WheelSaturation * SliderValue.
        /// </summary>
        [Category("Color")]
        [Description("Final saturation value (WheelSaturation multiplied by SliderValue).")]
        [DefaultValue(1f)]
        public float Saturation { get; private set; } = 1f;

        /// <summary>
        /// Raw saturation coming directly from the color wheel (radial distance).
        /// </summary>
        [Category("Color")]
        [Description("Raw saturation selected on the color wheel before slider scaling.")]
        [DefaultValue(1f)]
        public float WheelSaturation { get; private set; } = 1f;

        /// <summary>
        /// Saturation multiplier coming from the saturation slider (0.0 to 1.0).
        /// </summary>
        [Category("Color")]
        [Description("Saturation multiplier controlled by the saturation slider.")]
        [DefaultValue(1f)]
        public float SliderValue { get; private set; } = 1f;

        /// <summary>
        /// Value (brightness) component of the HSV color.
        /// Kept at 1.0 by default.
        /// </summary>
        [Category("Color")]
        [Description("Value (brightness) component of HSV. Default is 1.0.")]
        [DefaultValue(1f)]
        public float Value { get; set; } = 1f;

        // ============================================================
        // SLIDER LAYOUT
        // ============================================================

        private Orientation _sliderOrientation = Orientation.Vertical;

        /// <summary>
        /// Orientation of the saturation slider.
        /// Vertical places it to the right of the wheel,
        /// Horizontal places it below the wheel.
        /// </summary>
        [Category("Layout")]
        [Description("Orientation of the saturation slider relative to the color wheel.")]
        [DefaultValue(Orientation.Vertical)]
        public Orientation SliderOrientation
        {
            get => _sliderOrientation;
            set
            {
                if (_sliderOrientation != value)
                {
                    _sliderOrientation = value;
                    ApplySliderLayout();
                }
            }
        }

        private int _sliderMargin = 6;

        /// <summary>
        /// Distance in pixels between the color wheel and the saturation slider.
        /// </summary>
        [Category("Layout")]
        [Description("Spacing in pixels between the color wheel and the saturation slider.")]
        [DefaultValue(6)]
        public int SliderMargin
        {
            get => _sliderMargin;
            set
            {
                if (value < 0) value = 0;
                if (_sliderMargin != value)
                {
                    _sliderMargin = value;
                    ApplySliderLayout();
                }
            }
        }

        // ============================================================
        // SLIDER CURSOR PROPERTIES (PASSTHROUGH)
        // ============================================================

        /// <summary>
        /// Color of the saturation slider cursor.
        /// </summary>
        [Category("Appearance")]
        [Description("Color of the saturation slider cursor.")]
        [DefaultValue(typeof(Color), "Black")]
        public Color SliderColor
        {
            get => _saturSlider.CursorColor;
            set => _saturSlider.CursorColor = value;
        }

        /// <summary>
        /// Size style of the saturation slider cursor.
        /// </summary>
        [Category("Appearance")]
        [Description("Defines whether the saturation slider cursor is small or large.")]
        [DefaultValue(SliderCursorSize.Small)]
        public SliderCursorSize SliderCursorSize
        {
            get => _saturSlider.CursorSize;
            set => _saturSlider.CursorSize = value;
        }

        // ============================================================
        // EVENTS
        // ============================================================

        /// <summary>
        /// Occurs when the selected color changes.
        /// Provides the final RGB color.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the selected color changes (RGB).")]
        public event Action<Color> ColorChanged;

        /// <summary>
        /// Occurs when any HSV component changes.
        /// Provides Hue, Saturation and Value.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the HSV components change.")]
        public event Action<float, float, float> HSVChanged;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        /// <summary>
        /// Initializes a new instance of the ColorPicker control.
        /// </summary>
        public ColorPicker()
        {
            _wheel = new ColorWheel { Dock = DockStyle.Fill };
            _saturSlider = new SaturationSlider { Dock = DockStyle.Right, Width = 28 };

            Controls.Add(_wheel);
            Controls.Add(_saturSlider);

            // Wheel event: updates hue and raw saturation
            _wheel.ColorChanged += (sender, e) =>
            {
                var wheel = (ColorWheel)sender;

                Hue = wheel.Hue;
                WheelSaturation = wheel.Saturation;

                _saturSlider.Hue = Hue;
                Recalculate();
            };

            // Slider event: updates saturation multiplier
            _saturSlider.ValueChanged += (sender, e) =>
            {
                var slider = (SaturationSlider)sender;

                SliderValue = slider.Value;
                Recalculate();
            };

            // Initial synchronization
            _saturSlider.Hue = _wheel.Hue;

            ApplySliderLayout();
            Recalculate();
        }

        // ============================================================
        // INTERNAL LOGIC
        // ============================================================

        /// <summary>
        /// Applies the layout rules for the saturation slider
        /// based on orientation and margin.
        /// </summary>
        private void ApplySliderLayout()
        {
            _saturSlider.Orientation = SliderOrientation;

            if (SliderOrientation == Orientation.Vertical)
            {
                _saturSlider.Dock = DockStyle.Right;
                _saturSlider.Width = 28;
                _saturSlider.Margin = new Padding(SliderMargin, 0, 0, 0);
            }
            else
            {
                _saturSlider.Dock = DockStyle.Bottom;
                _saturSlider.Height = 28;
                _saturSlider.Margin = new Padding(0, SliderMargin, 0, 0);
            }

            if (IsHandleCreated && !IsDisposed)
            {
                PerformLayout();
                Invalidate();
            }
        }

        /// <summary>
        /// Recalculates the final color based on wheel and slider values
        /// and raises change events.
        /// </summary>
        private void Recalculate()
        {
            Saturation = Math.Max(0f, Math.Min(1f, WheelSaturation * SliderValue));
            var rgb = HSVToRGB(Hue, Saturation, Value);

            ColorChanged?.Invoke(rgb);
            HSVChanged?.Invoke(Hue, Saturation, Value);
        }

        // ============================================================
        // CONVENIENCE PROPERTIES
        // ============================================================

        /// <summary>
        /// Gets the currently selected color in RGB format.
        /// </summary>
        [Browsable(false)]
        public Color Color => HSVToRGB(Hue, Saturation, Value);

        /// <summary>
        /// Gets the RGB components as a tuple.
        /// </summary>
        [Browsable(false)]
        public (int R, int G, int B) RGB => (Color.R, Color.G, Color.B);

        /// <summary>
        /// Gets the HSV components as a tuple.
        /// </summary>
        [Browsable(false)]
        public (float H, float S, float V) HSV => (Hue, Saturation, Value);

        /// <summary>
        /// Gets the selected color in hexadecimal string format.
        /// </summary>
        [Browsable(false)]
        public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

        // ============================================================
        // PUBLIC METHODS
        // ============================================================

        /// <summary>
        /// Sets the color programmatically using HSV components.
        /// </summary>
        /// <param name="h">Hue (0 to 360).</param>
        /// <param name="s">Saturation (0.0 to 1.0).</param>
        /// <param name="v">Value (brightness), default is 1.0.</param>
        public void SetHSV(float h, float s, float v = 1f)
        {
            Hue = h % 360f;
            if (Hue < 0) Hue += 360f;

            Value = Math.Max(0f, Math.Min(1f, v));

            WheelSaturation = Math.Max(0f, Math.Min(1f, s));
            _wheel.Hue = Hue;

            SliderValue = 1f;
            _saturSlider.Value = s;
            _saturSlider.Hue = Hue;

            Recalculate();
        }

        // ============================================================
        // HELPERS
        // ============================================================

        /// <summary>
        /// Converts HSV color values to an RGB Color.
        /// </summary>
        public static Color HSVToRGB(float h, float s, float v)
        {
            if (s <= 0f)
            {
                int l = Clamp((int)(v * 255));
                return Color.FromArgb(l, l, l);
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
                Clamp((int)(r * 255f)),
                Clamp((int)(g * 255f)),
                Clamp((int)(b * 255f)));
        }

        private static int Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }
    }
}
