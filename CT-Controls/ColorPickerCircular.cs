
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

        private float _hue = 0f;                // Matiz (0..360)
        private float _cursorPosition = 0f;     // Ângulo cursor (0..360)
        private float _circularWidth = 0.15f;   // Proporção da espessura (0..1)
        private Color _color = Color.Red;    // Cor selecionada

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
            get => _color;
        }

        [Category("Behavior")]
        [Description("")]
        public float Hue
        {
            get => _hue;
            set
            {
                float v = Math.Max(0f, Math.Min(1f, value));
                if (Math.Abs(_hue - v) < float.Epsilon)
                    return;

                _hue = v;
                OnValueChanged();
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
            CursorPositionChanged?.Invoke(this, EventArgs.Empty);
        }

        [Category("Action")]
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
            float cx = Width / 2f;
            float cy = Height / 2f;

            float dx = p.X - cx;
            float dy = (p.Y - cy) * -1f;

            double ang = Math.Atan2(dy, dx) * 180f / Math.PI;
            if (ang < 0) ang += 360;

            CursorPosition = (float)ang;
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

                // Defina as cores (você pode parametrizar se quiser)
                Color colorTop = Color.Black;                   // topo (0%)
                Color colorMiddle = HSVToRGB(_hue, 1f, 1f);        // meio (50%)
                Color colorBottom = Color.White;                   // base (100%)

                // Gradiente linear vertical no bounding box externo
                using (var lgb = new LinearGradientBrush(outer, colorTop, colorBottom, LinearGradientMode.Vertical))
                {
                    // Define 3 pontos de interpolação: 0% (top), 50% (meio), 100% (bottom)
                    //var blend = new ColorBlend(5)
                    //{
                    //    Colors = new[] { colorTop, colorTop, colorMiddle, colorBottom, colorBottom },
                    //    Positions = new[] {0f,(_circularWidth / 2), 0.5f, 1-(_circularWidth/2),1f}
                    //};

                    var blend = new ColorBlend(3)
                    {
                        Colors = new[] { colorTop,  colorMiddle, colorBottom },
                        Positions = new[] { 0f, 0.5f, 1f }
                    };

                    lgb.InterpolationColors = blend;

                    // Evita artefatos nas bordas do gradiente
                    lgb.WrapMode = WrapMode.TileFlipXY;

                    // Preenche SOMENTE a área da rosca
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
            float cursorRing = Math.Max(2f, cursorOuterDia * 0.45f);

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


    }
}
