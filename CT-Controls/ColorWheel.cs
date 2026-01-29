
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

        private float _r = 0f;          // 0..1 (normalizado)
        private float _theta = 0f;      // 0..360
        private Color _color = new Color();

        // ===== Selector size/scaling =====
        private float _selectorDiameterRatio = 0.06f; // 6% do menor lado do controle
        private float _selectorStrokeRatio = 0.006f; // 0.6% do menor lado do controle

        [Category("Appearance")]
        [Description("Proporção do diâmetro do seletor em relação ao menor lado do controle (0..1). Ex.: 0.06 = 6%.")]
        [DefaultValue(0.06f)]
        public float SelectorDiameterRatio
        {
            get => _selectorDiameterRatio;
            set
            {
                float v = Math.Max(0.0f, Math.Min(1.0f, value));
                if (Math.Abs(_selectorDiameterRatio - v) < 0.0001f) return;
                _selectorDiameterRatio = v;
                Invalidate();
            }
        }

        [Category("Appearance")]
        [Description("Proporção da espessura do traço do seletor em relação ao menor lado do controle (0..1). Ex.: 0.006 = 0.6%.")]
        [DefaultValue(0.006f)]
        public float SelectorStrokeRatio
        {
            get => _selectorStrokeRatio;
            set
            {
                float v = Math.Max(0.0f, Math.Min(0.1f, value));
                if (Math.Abs(_selectorStrokeRatio - v) < 0.0001f) return;
                _selectorStrokeRatio = v;
                Invalidate();
            }
        }

        public ColorWheel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            DoubleBuffered = true;
            MinimumSize = new Size(64, 64);

            Resize += (s, e) => RecreateBitmap();
            HandleCreated += (s, e) => RecreateBitmap();
        }

        // =========================
        // Properties
        // =========================

        [Category("Color")]
        [Description("Get color selected from controller")]
        public Color SelectedColor
        {
            get => _color;
            set
            {
                _color = value;
                RThetaFromColor(_color, out _r, out _theta);
                OnColorChanged();
                Invalidate();
            }
        }

        [Category("Color")]
        [Description("R value from 0 to 1")]
        [DefaultValue(0f)]
        public float R
        {
            get => _r;
            set
            {
                float v = Math.Max(0f, Math.Min(1f, value));

                if (Math.Abs(_r - v) < float.Epsilon)
                    return;

                _r = v;
                _color = RGBFromRTheta(_r, _theta);
                OnColorChanged();
                Invalidate();
            }
        }

        [Category("Color")]
        [Description("Theta value from 0 to 360")]
        [DefaultValue(0f)]
        public float Theta
        {
            get => _theta;
            set
            {
                float v = ((value % 360f) + 360f) % 360f;

                if (Math.Abs(_theta - v) < float.Epsilon)
                    return;

                _theta = v;
                _color = RGBFromRTheta(_r, _theta);
                OnColorChanged();
                Invalidate();
            }
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

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            RecreateBitmap();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
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

            // Mesmo raio que você usa para a imagem
            float radius = Math.Min(Width, Height) * 0.5f * 0.95f;
            float satRadius = _r * radius;

            float rad = (float)(_theta * Math.PI / 180.0);

            float x = center.X + (float)Math.Cos(rad) * satRadius;

            // Mantém seu comportamento atual (Y invertido) para não alterar a UX:
            float y = center.Y + ((float)Math.Sin(rad) * satRadius * -1f);
            // Se quiser a convenção matemática (positivo para baixo), troque por:
            // float y = center.Y + (float)Math.Sin(rad) * satRadius;

            // --- Escala do seletor conforme o tamanho do controle ---
            float baseSize = Math.Min(Width, Height);

            // diâmetro do seletor (mínimo 6px para visibilidade)
            float selDiameter = Math.Max(6f, baseSize * _selectorDiameterRatio);

            // espessura das bordas (mínimo 1px)
            float outerStroke = Math.Max(1f, baseSize * _selectorStrokeRatio);
            float innerStroke = Math.Max(1f, (baseSize * _selectorStrokeRatio) * 0.7f);

            float outerRadius = selDiameter / 2f;

            // diâmetro interno ligeiramente menor para criar um aro duplo
            float innerDiameter = Math.Max(2f, selDiameter - Math.Max(2f, outerStroke));
            float innerRadius = innerDiameter / 2f;

            using (var outer = new Pen(Color.FromArgb(220, Color.Black), outerStroke))
            using (var inner = new Pen(Color.FromArgb(200, Color.White), innerStroke))
            {
                var oldSmoothing = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // círculo externo
                g.DrawEllipse(
                    outer,
                    x - outerRadius,
                    y - outerRadius,
                    selDiameter,
                    selDiameter
                );

                // círculo interno (menor)
                g.DrawEllipse(
                    inner,
                    x - innerRadius,
                    y - innerRadius,
                    innerDiameter,
                    innerDiameter
                );

                g.SmoothingMode = oldSmoothing;
            }
        }

        // =========================
        // Mouse
        // =========================

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
                UpdateFromPoint(e.Location);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _mouseDown = false;
            Capture = false;
        }

        private void UpdateFromPoint(Point p)
        {
            var center = new PointF(Width / 2f, Height / 2f);

            double dx = p.X - center.X;
            double dy = (p.Y - center.Y) * -1f;

            double angle = Math.Atan2(dy, dx);
            double radius = Math.Sqrt(dx * dx + dy * dy);

            // Normaliza 0..1 para acompanhar proporcionalmente após resize
            double maxR = Math.Min(Width, Height) * 0.5 * 0.95;
            _r = (float)Math.Max(0.0, Math.Min(1.0, radius / maxR));

            _theta = (float)(angle * 180.0 / Math.PI);
            _color = RGBFromRTheta(_r, _theta);
            // Se quiser normalizar para 0..360:
            // if (_theta < 0) _theta += 360f;

            OnColorChanged();
            Invalidate();
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

            // Constantes dos deslocamentos de fase (120° e 240°)
            const double SHIFT_BM = Math.PI / 2.0;      // 90°
            const double SHIFT_G = 2.0 * Math.PI / 3.0; // 120°
            const double SHIFT_B = 4.0 * Math.PI / 3.0; // 240°

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (dist > maxR)
                        continue;

                    // Ângulo em radianos
                    double theta = Math.Atan2(dy, dx); // [-π, π]

                    // r normalizado (0 no centro, 1 na borda)
                    float sat = Math.Min(1f, dist / maxR);
                    double r = sat;

                    // --- Mistura por cosseno (modelo Python convertido) ---
                    double r_ = Math.Cos(theta + SHIFT_BM) * r + 0.5;
                    double g_ = Math.Cos(theta - SHIFT_G + SHIFT_BM) * r + 0.5;
                    double b_ = Math.Cos(theta - SHIFT_B + SHIFT_BM) * r + 0.5;

                    // Clamping 0..1
                    r_ = (r_ < 0) ? 0 : (r_ > 1 ? 1 : r_);
                    g_ = (g_ < 0) ? 0 : (g_ > 1 ? 1 : g_);
                    b_ = (b_ < 0) ? 0 : (b_ > 1 ? 1 : b_);

                    int R = (int)Math.Round(r_ * 255.0);
                    int G = (int)Math.Round(g_ * 255.0);
                    int B = (int)Math.Round(b_ * 255.0);

                    _bitmap.SetPixel(x, y, Color.FromArgb(255, R, G, B));
                }
            }

            Invalidate();
        }

        // =========================
        // Helpers
        // =========================

        private static int Clamp(int v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return v;
        }

        private static double Rad2Deg(double rad)
        {
            return rad / Math.PI * 180.0;
        }

        private static double Deg2Rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        private Color RGBFromRTheta(float r, float theta)
        {
            float fr = (float)((Math.Cos(Deg2Rad(theta - 90f)) * r) + 0.5f);
            float fg = (float)((Math.Cos(Deg2Rad(theta + 120f - 90f)) * r) + 0.5f);
            float fb = (float)((Math.Cos(Deg2Rad(theta + 240f - 90f)) * r) + 0.5f);

            int eR = fr < 0 ? 0 : (fr > 1 ? 255 : (byte)(fr * 255));
            int eG = fg < 0 ? 0 : (fg > 1 ? 255 : (byte)(fg * 255));
            int eB = fb < 0 ? 0 : (fb > 1 ? 255 : (byte)(fb * 255));

            return Color.FromArgb(eR, eG, eB);
        }

        private void RThetaFromColor(Color color, out float r, out float theta)
        {
            double R = color.R / 255.0;
            double G = color.G / 255.0;
            double B = color.B / 255.0;

            double R1 = (R - 0.5) * 2.0;
            double G1 = (G - 0.5) * 2.0;
            double B1 = (B - 0.5) * 2.0;

            double off_angle = Deg2Rad(90);
            double Rtheta = Deg2Rad(0) + off_angle;
            double Btheta = Deg2Rad(120) + off_angle;
            double Gtheta = Deg2Rad(240) + off_angle;

            double Rx = R1 * Math.Cos(Rtheta);
            double Ry = R1 * Math.Sin(Rtheta);

            double Gx = G1 * Math.Cos(Gtheta);
            double Gy = G1 * Math.Sin(Gtheta);

            double Bx = B1 * Math.Cos(Btheta);
            double By = B1 * Math.Sin(Btheta);

            double Rx_total = Rx + Gx + Bx;
            double Ry_total = Ry + Gy + By;

            double r1 = Math.Sqrt(Rx_total * Rx_total + Ry_total * Ry_total) / 2.0;
            double theta1 = Math.Atan2(Ry_total, Rx_total);

            r = (float)r1;                      // 0..1
            theta = (float)Rad2Deg(theta1);     // graus
            if (theta < 0) theta += 360f;
        }
    }
}
