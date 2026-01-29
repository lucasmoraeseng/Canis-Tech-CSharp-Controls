
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CT_Controls
{
    [ToolboxItem(true)]
    [DefaultEvent(nameof(CursorPositionChanged))]
    [DefaultProperty(nameof(CursorPosition))]
    public partial class ColorPickerCircular : UserControl
    {
        // =========================
        // Fields
        // =========================

        private float _cursorPosition = 0f;     // Ângulo cursor (0..360)
        private float _circularWidth = 0.15f;   // Proporção da espessura (0..1)
        private Color _selectedColor = Color.Red;       // Cor selecionada
        private Color _inputColor = Color.Red;       // Cor de entrada

        // =========================
        // Constructor
        // =========================

        public ColorPickerCircular()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            DoubleBuffered = true;
            BackColor = Color.Transparent;
            MinimumSize = new Size(40, 40);
        }

        // =========================
        // Properties
        // =========================

        [Category("Behavior")]
        [Description("")]
        public Color SelectedColor
        {
            get => _selectedColor;
        }

        [Category("Behavior")]
        [Description("")]
        public Color InputColor
        {
            get => _inputColor;
            set
            {
                if (_inputColor == value)
                    return;

                _inputColor = value;

                // Recalcula a cor atual baseada no ângulo do cursor
                var newColor = ColorAtAngle(_cursorPosition, _inputColor);
                if (newColor != _selectedColor)
                {
                    _selectedColor = newColor;
                    OnValueChanged();
                }

                Invalidate();
            }
        }


        [Category("Behavior")]
        [Description("Cursor position in degrees (0..360).")]
        [DefaultValue(0f)]
        public float CursorPosition
        {
            get => _cursorPosition;
            set
            {
                float v = value % 360f;
                if (v < 0f)
                    v += 360f;

                if (Math.Abs(_cursorPosition - v) < float.Epsilon)
                    return;

                _cursorPosition = v;
                OnCursorPositionChanged();
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Circular ring thickness proportion (0..1).")]
        [DefaultValue(0.15f)]
        public float CircularWidth
        {
            get => _circularWidth;
            set
            {
                float v = Math.Max(0.01f, Math.Min(1f, value));
                if (Math.Abs(_circularWidth - v) < float.Epsilon)
                    return;

                _circularWidth = v;
                Invalidate();
            }
        }

        // =========================
        // Event
        // =========================

        [Category("Action")]
        public event EventHandler CursorPositionChanged;

        protected virtual void OnCursorPositionChanged()
        {
            UpdateSelectedColor();
            CursorPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        [Category("Action")]
        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged()
        {
            UpdateSelectedColor();
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
            float cx = Width / 2f;
            float cy = Height / 2f;

            float dx = p.X - cx;
            float dy = (p.Y - cy) * -1f; // você inverte o Y no desenho; mantive

            double ang = Math.Atan2(dy, dx) * 180f / Math.PI;
            if (ang < 0) ang += 360;

            CursorPosition = (float)ang;
            Invalidate();
        }

        private void UpdateSelectedColor()
        {
            var newColor = ColorAtAngle(CursorPosition, _inputColor);
            if (newColor != _selectedColor)
            {
                _selectedColor = newColor;
                OnValueChanged();
            }
        }

        // =========================
        // Painting
        // =========================

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0)
                return;

            float outerDiameter = Math.Min(rect.Width, rect.Height);
            float outerRadius = outerDiameter / 2f;

            RectangleF outer = new RectangleF(
                rect.X + (rect.Width - outerDiameter) / 2f,
                rect.Y + (rect.Height - outerDiameter) / 2f,
                outerDiameter,
                outerDiameter
            );

            float thickness = Math.Max(1f, outerDiameter * _circularWidth);
            thickness = Math.Min(thickness, outerDiameter / 2f - 2f);

            float innerDiameter = outerDiameter - 2f * thickness;
            float innerRadius = innerDiameter / 2f;

            RectangleF inner = new RectangleF(
                outer.X + (outer.Width - innerDiameter) / 2f,
                outer.Y + (outer.Height - innerDiameter) / 2f,
                innerDiameter,
                innerDiameter
            );
            Graphics g = e.Graphics;

            using (GraphicsPath path = new GraphicsPath())
            {
                path.FillMode = FillMode.Alternate;
                path.AddEllipse(outer); // anel externo
                path.AddEllipse(inner); // “furo” interno

                // Cores do gradiente vertical: topo preto, meio = hue, base branco
                Color colorTop = Color.Black;
                Color colorMiddle = _inputColor; // ✅ usa hue em graus
                Color colorBottom = Color.White;

                using (var lgb = new LinearGradientBrush(outer, colorTop, colorBottom, LinearGradientMode.Vertical))
                {
                    var blend = new ColorBlend(3)
                    {
                        Colors = new[] { colorTop, colorMiddle, colorBottom },
                        Positions = new[] { 0f, 0.5f, 1f }
                    };

                    lgb.InterpolationColors = blend;
                    lgb.WrapMode = WrapMode.TileFlipXY;

                    g.FillPath(lgb, path);
                }
            }

            DrawCursor(e.Graphics, outerDiameter, thickness, rect);
        }

        // =========================
        // Draw cursor
        // =========================

        private void DrawCursor(Graphics g, float diameter, float thickness, Rectangle rect)
        {
            var oldSmoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float cx = rect.X + rect.Width / 2f;
            float cy = rect.Y + rect.Height / 2f;

            float outerRadius = diameter / 2f;
            float innerRadius = outerRadius - thickness;
            float midRadius = innerRadius + thickness / 2f;

            double angle_rad = (_cursorPosition / 360f) * 2.0 * Math.PI;

            float cxCursor = cx + (float)(midRadius * Math.Cos(angle_rad));
            float cyCursor = cy + (float)(midRadius * Math.Sin(angle_rad) * -1f);

            float cursorOuterDia = thickness;
            float cursorInnerDia = thickness * 0.7f;

            using (var pen = new Pen(Color.Black, 0.5f))
            using (var brush = new SolidBrush(Color.White))
            {
                float margin = pen.Width / 2f;

                float outerRad = (cursorOuterDia / 2f) - margin;
                float innerRad = cursorInnerDia / 2f;

                RectangleF rOuter = new RectangleF(
                    cxCursor - outerRad, cyCursor - outerRad,
                    outerRad * 2f, outerRad * 2f);

                RectangleF rInner = new RectangleF(
                    cxCursor - innerRad, cyCursor - innerRad,
                    innerRad * 2f, innerRad * 2f);

                using (var path = new GraphicsPath())
                {
                    path.FillMode = FillMode.Alternate;
                    path.AddEllipse(rOuter);
                    path.AddEllipse(rInner);
                    g.FillPath(brush, path);
                }

                g.DrawEllipse(pen, rOuter);
                g.DrawEllipse(pen, rInner);
            }

            g.SmoothingMode = oldSmoothing;
        }

        // =========================
        // Helpers
        // =========================

        private static Color HSVToRGB(float h, float s, float v)
        {
            if (s <= 0f)
            {
                int val = (int)(v * 255);
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

        private static int Clamp(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);

        private static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            int r = (int)Math.Round(a.R + (b.R - a.R) * t);
            int g = (int)Math.Round(a.G + (b.G - a.G) * t);
            int bC = (int)Math.Round(a.B + (b.B - a.B) * t);
            return Color.FromArgb(r, g, bC);
        }

        private Color ColorAtAngle(float angleDeg, Color inputColor)
        {
            // Normaliza 0..360
            float a = ((angleDeg % 360f) + 360f) % 360f;

            Color black = Color.Black;
            Color white = Color.White;

            if (a <= 90f)
            {
                // 0..90: inputColor -> black
                float t = a / 90f;
                return Lerp(inputColor, black, t);
            }
            else if (a <= 180f)
            {
                // 90..180: black -> inputColor
                float t = (a - 90f) / 90f;
                return Lerp(black, inputColor, t);
            }
            else if (a <= 270f)
            {
                // 180..270: inputColor -> white
                float t = (a - 180f) / 90f;
                return Lerp(inputColor, white, t);
            }
            else
            {
                // 270..360: white -> inputColor
                float t = (a - 270f) / 90f;
                return Lerp(white, inputColor, t);
            }
        }

    }
}
