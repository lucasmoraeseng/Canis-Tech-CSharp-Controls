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
        //private readonly ColorWheel _wheel;
        //private readonly ColorPickerCircular _circularPicker;
        private Color _color = Color.Red;

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        /// <summary>
        /// Initializes a new instance of the ColorPicker control.
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();

            // Initial synchronization
            _circularPicker.InputColor = _wheel.SelectedColor;
            _color = _circularPicker.SelectedColor;
        }

        // ============================================================
        // HSV STATE (read-only from outside)
        // ============================================================

        [Category("Color")]
        [Description("Get color selected from controller")]
        public Color SelectedColor
        {
            get => _color;
            set
            {
                _color = value;
                OnColorChanged();
                Invalidate();
            }
        }


        // ============================================================
        // EVENTS
        // ============================================================

        /// <summary>
        /// Occurs when the selected color changes.
        /// Provides the final RGB color.
        /// </summary>
        [Category("Action")]
        public event EventHandler ColorChanged;
        protected virtual void OnColorChanged()
        {
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
                      

        // ============================================================
        // INTERNAL LOGIC
        // ============================================================
        private void Wheel_ColorChanged(object sender, EventArgs e)
        {
            _circularPicker.InputColor = _wheel.SelectedColor;
            _color = _circularPicker.SelectedColor;
            OnColorChanged();
        }

        private void CircularPicker_CursorPositionChanged(object sender, EventArgs e)
        {
            _color = _circularPicker.SelectedColor;
            OnColorChanged();
        }


        // ============================================================
        // CONVENIENCE PROPERTIES
        // ============================================================

        /// <summary>
        /// Gets the currently selected color in RGB format.
        /// </summary>
        [Browsable(false)]
        public Color Color => _color;

        /// <summary>
        /// Gets the RGB components as a tuple.
        /// </summary>
        [Browsable(false)]
        public (int R, int G, int B) RGB => (Color.R, Color.G, Color.B);

        /// <summary>
        /// Gets the selected color in hexadecimal string format.
        /// </summary>
        [Browsable(false)]
        public string Hex => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

        // ============================================================
        // Pass-through PROPERTIES
        // ============================================================
        [Category("Style")]
        [Description("Width of circular picker")]
        public float CircularPickerWidth
        {
            get => _circularPicker.CircularWidth;
            set => _circularPicker.CircularWidth = value;
        }


        // ============================================================
        // PUBLIC METHODS
        // ============================================================


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

        private void _circularPicker_Resize(object sender, EventArgs e)
        {
            base.OnResize(e);

            int size = Math.Min(Width, Height);

            int ringThickness = (int)(size * 0.2f);

           _circularPicker.Padding = new Padding(ringThickness);

            Invalidate();
        }
    }
}
