using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CT_Controls
{
    public partial class NumericDisplay : UserControl
    {
        // propriedades internas
        private string unitText = "V";
        private string valueText = "0";
        private string titleText = "Voltage";
        private Color accentColor = Color.FromArgb(0, 123, 255); // azul Bootstrap-like
        private Color borderColor = Color.FromArgb(220, 220, 220);
        private Color valueBackColor = Color.White;
        private Color valueForeColor = Color.Black;
        private Color unitForeColor = Color.White;
        private Color titleForeColor = Color.White;
        private int cornerRadius = 10;
        private int borderWidth = 1;
        private Padding innerPadding = new Padding(8, 4, 8, 4);
        private bool showUnit = true;
        private bool showValue = true;
        private bool showTitle = true;

        public NumericDisplay()
        {
            InitializeComponent();
            InitializeControls();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            UpdateLayout();
        }

        private void InitializeControls()
        {
            // prepara labels (fornecidos pelo Designer) para desenho transparente
            lblTitle.BackColor = Color.Transparent;
            lblTitle.ForeColor = titleForeColor;
            lblTitle.Text = titleText;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            lblValue.BackColor = Color.Transparent;
            lblValue.ForeColor = valueForeColor;
            lblValue.Text = valueText;
            lblValue.TextAlign = ContentAlignment.MiddleCenter;

            lblUnit.BackColor = Color.Transparent;
            lblUnit.ForeColor = unitForeColor;
            lblUnit.Text = unitText;
            lblUnit.TextAlign = ContentAlignment.MiddleCenter;

            // escuta redimensionamento / fonte
            this.SizeChanged += (s, e) => UpdateLayout();
            this.FontChanged += (s, e) => UpdateLayout();
        }

        // --- Propriedades públicas ----------------

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Texto do título exibido à esquerda.")]
        public string TitleText
        {
            get => titleText;
            set
            {
                titleText = value ?? string.Empty;
                lblTitle.Text = titleText;
                UpdateLayout();
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor do texto do título.")]
        public Color TitleForeColor
        {
            get => titleForeColor;
            set
            {
                titleForeColor = value;
                lblTitle.ForeColor = titleForeColor;
                lblTitle.Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Texto da unidade exibida à direita.")]
        public string UnitText
        {
            get => unitText;
            set
            {
                unitText = value ?? string.Empty;
                lblUnit.Text = unitText;
                UpdateLayout();
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Texto do valor exibido no centro.")]
        public string ValueText
        {
            get => valueText;
            set
            {
                valueText = value ?? string.Empty;
                lblValue.Text = valueText;
                UpdateLayout();
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor de destaque (usada no bloco do título).")]
        public Color AccentColor
        {
            get => accentColor;
            set { accentColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor da borda externa.")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor de fundo do campo central (valor).")]
        public Color ValueBackColor
        {
            get => valueBackColor;
            set { valueBackColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor do texto do valor.")]
        public Color ValueForeColor
        {
            get => valueForeColor;
            set { valueForeColor = value; lblValue.ForeColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor do texto da unidade.")]
        public Color UnitForeColor
        {
            get => unitForeColor;
            set { unitForeColor = value; lblUnit.ForeColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Raio das bordas (em pixels).")]
        [DefaultValue(10)]
        public int CornerRadius
        {
            get => cornerRadius;
            set { cornerRadius = Math.Max(0, value); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Largura da borda externa.")]
        [DefaultValue(1)]
        public int BorderWidth
        {
            get => borderWidth;
            set { borderWidth = Math.Max(0, value); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Mostrar ou ocultar o título (bloco esquerdo).")]
        [DefaultValue(true)]
        public bool ShowTitle
        {
            get => showTitle;
            set { showTitle = value; lblTitle.Visible = showTitle; UpdateLayout(); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Mostrar ou ocultar a unidade (bloco direito).")]
        [DefaultValue(true)]
        public bool ShowUnit
        {
            get => showUnit;
            set { showUnit = value; lblUnit.Visible = showUnit; UpdateLayout(); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Mostrar ou ocultar o valor central.")]
        [DefaultValue(true)]
        public bool ShowValue
        {
            get => showValue;
            set { showValue = value; lblValue.Visible = showValue; UpdateLayout(); Invalidate(); }
        }

        // --- Layout / desenho ---------------------

        private void UpdateLayout()
        {
            // calcula área interna (dentro da borda)
            var client = ClientRectangle;
            var inner = new Rectangle(client.X + borderWidth, client.Y + borderWidth,
                Math.Max(0, client.Width - borderWidth * 2), Math.Max(0, client.Height - borderWidth * 2));

            using (var g = CreateGraphics())
            {
                // medidas do título (auto width)
                int leftWidth = 0;
                if (showTitle && !string.IsNullOrEmpty(titleText))
                {
                    var sz = TextRenderer.MeasureText(g, titleText, this.Font);
                    leftWidth = sz.Width + innerPadding.Left + innerPadding.Right;
                }

                // medidas da unidade (auto width)
                int rightWidth = 0;
                if (showUnit && !string.IsNullOrEmpty(unitText))
                {
                    var sz = TextRenderer.MeasureText(g, unitText, this.Font);
                    rightWidth = sz.Width + innerPadding.Left + innerPadding.Right;
                }

                // espaço para valor central
                int centerX = inner.X + leftWidth;
                int centerW = Math.Max(0, inner.Width - leftWidth - rightWidth);

                // posiciona labels centralizados verticalmente
                if (showTitle)
                {
                    var titleRect = new Rectangle(inner.X, inner.Y, leftWidth, inner.Height);
                    lblTitle.Bounds = titleRect;
                    lblTitle.Font = this.Font;
                    lblTitle.TextAlign = ContentAlignment.MiddleCenter;
                }

                if (showValue)
                {
                    var valueRect = new Rectangle(centerX, inner.Y, centerW, inner.Height);
                    lblValue.Bounds = valueRect;
                    lblValue.Font = this.Font;
                    lblValue.TextAlign = ContentAlignment.MiddleCenter;
                }

                if (showUnit)
                {
                    var unitRect = new Rectangle(inner.X + inner.Width - rightWidth, inner.Y, rightWidth, inner.Height);
                    lblUnit.Bounds = unitRect;
                    lblUnit.Font = this.Font;
                    lblUnit.TextAlign = ContentAlignment.MiddleCenter;
                }
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            // desenha borda externa arredondada (preencha com BackColor)
            using (var pathOuter = RoundedRectPath(rect, cornerRadius))
            using (var brushBackground = new SolidBrush(this.BackColor))
            using (var penBorder = new Pen(borderColor, borderWidth))
            {
                g.FillPath(brushBackground, pathOuter);
                if (borderWidth > 0)
                {
                    g.DrawPath(penBorder, pathOuter);
                }
            }

            // inner area (dentro da borda)
            var innerRect = new Rectangle(rect.X + borderWidth, rect.Y + borderWidth,
                Math.Max(0, rect.Width - borderWidth * 2), Math.Max(0, rect.Height - borderWidth * 2));

            if (innerRect.Width <= 0 || innerRect.Height <= 0) return;

            // calcula larguras do título/unidade (mesma lógica do UpdateLayout)
            int leftWidth = 0;
            if (showTitle && !string.IsNullOrEmpty(titleText))
            {
                var sz = TextRenderer.MeasureText(g, titleText, this.Font);
                leftWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            }

            int rightWidth = 0;
            if (showUnit && !string.IsNullOrEmpty(unitText))
            {
                var sz = TextRenderer.MeasureText(g, unitText, this.Font);
                rightWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            }

            // desenha bloco do título (esquerdo) com cantos arredondados à esquerda
            if (leftWidth > 0)
            {
                var leftRect = new Rectangle(innerRect.X, innerRect.Y, leftWidth, innerRect.Height);
                using (var pathLeft = RoundedRectPath(leftRect, cornerRadius, true, false, false, true))
                using (var brush = new SolidBrush(accentColor))
                {
                    g.FillPath(brush, pathLeft);
                }
            }

            // desenha bloco da unidade (direito) com cantos arredondados à direita
            if (rightWidth > 0)
            {
                var rightRect = new Rectangle(innerRect.X + innerRect.Width - rightWidth, innerRect.Y, rightWidth, innerRect.Height);
                using (var pathRight = RoundedRectPath(rightRect, cornerRadius, false, true, true, false))
                using (var brush = new SolidBrush(accentColor))
                {
                    g.FillPath(brush, pathRight);
                }
            }

            // desenha área central (sem arredondamento)
            var centerRect = new Rectangle(innerRect.X + leftWidth, innerRect.Y, Math.Max(0, innerRect.Width - leftWidth - rightWidth), innerRect.Height);
            if (centerRect.Width > 0)
            {
                using (var brush = new SolidBrush(valueBackColor))
                {
                    g.FillRectangle(brush, centerRect);
                }
            }
        }

        // cria GraphicsPath com cantos arredondados seletivos
        private GraphicsPath RoundedRectPath(Rectangle rect, int radius,
            bool roundTopLeft = true, bool roundTopRight = true, bool roundBottomRight = true, bool roundBottomLeft = true)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                path.CloseFigure();
                return path;
            }

            float r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f);
            var tl = new RectangleF(rect.Left, rect.Top, r * 2, r * 2);
            var tr = new RectangleF(rect.Right - r * 2, rect.Top, r * 2, r * 2);
            var br = new RectangleF(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2);
            var bl = new RectangleF(rect.Left, rect.Bottom - r * 2, r * 2, r * 2);

            // top-left
            if (roundTopLeft)
                path.AddArc(tl, 180, 90);
            else
                path.AddLine(rect.Left, rect.Top, rect.Left, rect.Top);

            // top edge to top-right
            if (roundTopRight)
                path.AddArc(tr, 270, 90);
            else
                path.AddLine(rect.Right, rect.Top, rect.Right, rect.Top);

            // right edge
            if (roundBottomRight)
                path.AddArc(br, 0, 90);
            else
                path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);

            // bottom edge
            if (roundBottomLeft)
                path.AddArc(bl, 90, 90);
            else
                path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom);

            path.CloseFigure();
            return path;
        }
    }
}
