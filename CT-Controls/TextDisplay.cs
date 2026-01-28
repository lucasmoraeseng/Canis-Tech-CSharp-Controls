
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CT_Controls
{
    public partial class TextDisplay : UserControl
    {
        // propriedades internas
        private string valueText = "0";
        private string titleText = "Voltage";
        private Color accentColor = Color.FromArgb(0, 123, 255); // azul estilo Bootstrap
        private Color borderColor = Color.FromArgb(220, 220, 220);
        private Color valueBackColor = Color.White;
        private Color valueForeColor = Color.Black;
        private Color titleForeColor = Color.White;
        private int cornerRadius = 10;
        private int borderWidth = 1;
        private Padding innerPadding = new Padding(8, 4, 8, 4);
        private bool showValue = true;
        private bool showTitle = true;

        public TextDisplay()
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

            // escuta redimensionamento / fonte
            this.SizeChanged += (s, e) => UpdateLayout();
            this.FontChanged += (s, e) => UpdateLayout();
        }

        // --- Propriedades públicas ----------------

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Texto do título exibido no bloco colorido (esquerda).")]
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
        [Description("Texto do valor exibido no bloco branco (direita).")]
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
        [Description("Cor de destaque do bloco esquerdo.")]
        public Color AccentColor
        {
            get => accentColor;
            set { accentColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor da borda externa do controle.")]
        public Color BorderColor
        {
            get => borderColor;
            set { borderColor = value; Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor de fundo do bloco direito (valor).")]
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
        [Description("Mostrar/ocultar o bloco esquerdo (título).")]
        [DefaultValue(true)]
        public bool ShowTitle
        {
            get => showTitle;
            set { showTitle = value; lblTitle.Visible = showTitle; UpdateLayout(); Invalidate(); }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [Description("Mostrar/ocultar o bloco direito (valor).")]
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
            var inner = new Rectangle(
                client.X + borderWidth,
                client.Y + borderWidth,
                Math.Max(0, client.Width - borderWidth * 2),
                Math.Max(0, client.Height - borderWidth * 2)
            );

            // Larguras de texto + padding (sem gap)
            int leftWidth = 0;
            if (showTitle && !string.IsNullOrEmpty(titleText))
            {
                var sz = TextRenderer.MeasureText(titleText, this.Font);
                leftWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            }

            int rightWidth = 0;
            if (showValue && !string.IsNullOrEmpty(valueText))
            {
                var sz = TextRenderer.MeasureText(valueText, this.Font);
                rightWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            }

            int totalNeeded = leftWidth + rightWidth;

            // Se não couber, compacta proporcionalmente SEM gap
            if (totalNeeded > inner.Width && totalNeeded > 0)
            {
                float scale = inner.Width / (float)Math.Max(1, totalNeeded);
                if (showTitle) leftWidth = (int)Math.Max(0, leftWidth * scale);
                if (showValue) rightWidth = (int)Math.Max(0, rightWidth * scale);
            }

            // Posiciona labels (encostados, sem gap)
            int x = inner.X;
            if (showTitle && leftWidth > 0)
            {
                var titleRect = new Rectangle(x, inner.Y, leftWidth, inner.Height);
                lblTitle.Bounds = titleRect;
                lblTitle.Font = this.Font;
                lblTitle.TextAlign = ContentAlignment.MiddleCenter;
                x += leftWidth; // encostado
            }

            if (showValue && rightWidth > 0)
            {
                var valueRect = new Rectangle(inner.Right - rightWidth, inner.Y, rightWidth, inner.Height);
                lblValue.Bounds = valueRect;
                lblValue.Font = this.Font;
                lblValue.TextAlign = ContentAlignment.MiddleCenter;
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

            // desenha borda externa arredondada (fundo do controle)
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
            var innerRect = new Rectangle(
                rect.X + borderWidth,
                rect.Y + borderWidth,
                Math.Max(0, rect.Width - borderWidth * 2),
                Math.Max(0, rect.Height - borderWidth * 2)
            );

            if (innerRect.Width <= 0 || innerRect.Height <= 0) return;

            // calcula larguras do título/valor (mesma lógica do UpdateLayout)
            int leftWidth = 0;
            if (showTitle && !string.IsNullOrEmpty(titleText))
            {
                var sz = TextRenderer.MeasureText(titleText, this.Font);
                leftWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            }

            int rightWidth = rect.Width - leftWidth;
            //if (showValue && !string.IsNullOrEmpty(valueText))
            //{
            //    var sz = TextRenderer.MeasureText(valueText, this.Font);
            //    rightWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            //}

            int totalNeeded = leftWidth + rightWidth;
            if (totalNeeded > innerRect.Width && totalNeeded > 0)
            {
                float scale = innerRect.Width / (float)Math.Max(1, totalNeeded);
                if (showTitle) leftWidth = (int)Math.Max(0, leftWidth * scale);
                if (showValue) rightWidth = (int)Math.Max(0, rightWidth * scale);
            }

            // desenha bloco do título (esquerdo) com cantos arredondados em todas as extremidades
            if (showTitle && leftWidth > 0)
            {
                var leftRect = new Rectangle(innerRect.X, innerRect.Y, leftWidth, innerRect.Height);
                using (var pathLeft = RoundedRectPath(leftRect, cornerRadius, true, false, false, true))
                using (var brush = new SolidBrush(accentColor))
                {
                    g.FillPath(brush, pathLeft);
                }
            }

            // desenha bloco do valor (direito) com cantos arredondados em todas as extremidades
            if (showValue && rightWidth > 0)
            {
                var rightRect = new Rectangle(innerRect.Right - rightWidth, innerRect.Y, rightWidth, innerRect.Height);
                using (var pathRight = RoundedRectPath(rightRect, cornerRadius, false, true, true, false))
                using (var brush = new SolidBrush(valueBackColor))
                {
                    g.FillPath(brush, pathRight);
                }
            }

            // Observação:
            // Agora não há gap: os blocos encostam. Como você corrigiu o arredondamento
            // no encontro, o resultado visual fica como desejado.
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
            if (roundTopLeft) path.AddArc(tl, 180, 90);
            else path.AddLine(rect.Left, rect.Top, rect.Left, rect.Top);

            // top-right
            if (roundTopRight) path.AddArc(tr, 270, 90);
            else path.AddLine(rect.Right, rect.Top, rect.Right, rect.Top);

            // bottom-right
            if (roundBottomRight) path.AddArc(br, 0, 90);
            else path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);

            // bottom-left
            if (roundBottomLeft) path.AddArc(bl, 90, 90);
            else path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom);

            path.CloseFigure();
            return path;
        }
    }
}
