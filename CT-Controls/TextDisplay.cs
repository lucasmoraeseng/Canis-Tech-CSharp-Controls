
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
            // Usa labels do Designer (sem recriar)
            lblTitle.AutoSize = false;
            lblValue.AutoSize = false;

            lblTitle.BackColor = Color.Transparent;
            lblValue.BackColor = Color.Transparent;

            lblTitle.ForeColor = titleForeColor;
            lblValue.ForeColor = valueForeColor;

            lblTitle.Text = titleText;
            lblValue.Text = valueText;

            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblValue.TextAlign = ContentAlignment.MiddleCenter;

            lblTitle.Padding = innerPadding;
            lblValue.Padding = innerPadding;

            // reflow em size/font change
            this.SizeChanged += (s, e) => UpdateLayout();
            this.FontChanged += (s, e) => UpdateLayout();

            UpdateLayout();
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
        [Description("Fonte do texto do título exibido no bloco colorido ")]
        public Font TitleTextFont
        {
            get =>lblTitle.Font;
            set
            {
                lblTitle.Font = value;
                UpdateLayout();
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Alinhamento do texto do título.")]
        public ContentAlignment TitleTextAlign
        {
            get => lblTitle.TextAlign;
            set { lblTitle.TextAlign = value; lblTitle.Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor do texto do título.")]
        public Color TitleForeColor
        {
            get => titleForeColor;
            set { titleForeColor = value; lblTitle.ForeColor = titleForeColor; lblTitle.Invalidate(); }
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
        [Description("Fonte do texto do título exibido no bloco colorido ")]
        public Font ValueTextFont
        {
            get => lblValue.Font;
            set
            {
                lblValue.Font = value;
                UpdateLayout();
                Invalidate();
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Alinhamento do texto do valor.")]
        public ContentAlignment ValueTextAlign
        {
            get => lblValue.TextAlign;
            set { lblValue.TextAlign = value; lblValue.Invalidate(); }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Cor do texto do valor.")]
        public Color ValueForeColor
        {
            get => valueForeColor;
            set { valueForeColor = value; lblValue.ForeColor = valueForeColor; Invalidate(); }
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
        [Category("Layout")]
        [Description("Padding interno para os textos (aplicado em ambos os campos).")]
        public Padding InnerPadding
        {
            get => innerPadding;
            set
            {
                innerPadding = value;
                lblTitle.Padding = innerPadding;
                lblValue.Padding = innerPadding;
                UpdateLayout();
                Invalidate();
            }
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
            var client = ClientRectangle;
            var inner = new Rectangle(
                client.X + borderWidth,
                client.Y + borderWidth,
                Math.Max(0, client.Width - borderWidth * 2),
                Math.Max(0, client.Height - borderWidth * 2)
            );

            // Largura do título = medida do texto + padding
            int leftWidth = 0;
            if (showTitle && !string.IsNullOrEmpty(titleText))
            {
                var sz = TextRenderer.MeasureText(titleText, this.Font);
                leftWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            }

            // Se o título não couber, compacta só o título
            if (leftWidth > inner.Width)
            {
                leftWidth = inner.Width;
            }

            // Largura do valor = resto da área interna
            int rightWidth = 0;
            if (showValue)
            {
                rightWidth = Math.Max(0, inner.Width - leftWidth);
            }

            // Posiciona os labels
            if (showTitle && leftWidth > 0)
            {
                lblTitle.Bounds = new Rectangle(inner.X, inner.Y, leftWidth, inner.Height);
                lblTitle.Font = this.Font;
            }
            else
            {
                lblTitle.Bounds = Rectangle.Empty;
            }

            if (showValue && rightWidth > 0)
            {
                lblValue.Bounds = new Rectangle(inner.Right - rightWidth, inner.Y, rightWidth, inner.Height);
                lblValue.Font = this.Font;
            }
            else
            {
                lblValue.Bounds = Rectangle.Empty;
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

            // borda/fundo externo arredondado
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

            // área interna (dentro da borda)
            var innerRect = new Rectangle(
                rect.X + borderWidth,
                rect.Y + borderWidth,
                Math.Max(0, rect.Width - borderWidth * 2),
                Math.Max(0, rect.Height - borderWidth * 2)
            );
            if (innerRect.Width <= 0 || innerRect.Height <= 0) return;

            // Mesma lógica que UpdateLayout: leftWidth medido; rightWidth = restante
            int leftWidth = 0;
            if (showTitle && !string.IsNullOrEmpty(titleText))
            {
                var sz = TextRenderer.MeasureText(titleText, this.Font);
                leftWidth = sz.Width + innerPadding.Left + innerPadding.Right;
            }
            if (leftWidth > innerRect.Width)
            {
                leftWidth = innerRect.Width;
            }

            int rightWidth = showValue ? Math.Max(0, innerRect.Width - leftWidth) : 0;

            // Título (esquerda) com cantos arredondados à esquerda
            if (showTitle && leftWidth > 0)
            {
                var leftRect = new Rectangle(innerRect.X, innerRect.Y, leftWidth, innerRect.Height);
                using (var pathLeft = RoundedRectPath(leftRect, cornerRadius, true, false, false, true))
                using (var brush = new SolidBrush(accentColor))
                {
                    g.FillPath(brush, pathLeft);
                }
            }

            // Valor (direita) com cantos arredondados à direita
            if (showValue && rightWidth > 0)
            {
                var rightRect = new Rectangle(innerRect.Right - rightWidth, innerRect.Y, rightWidth, innerRect.Height);
                using (var pathRight = RoundedRectPath(rightRect, cornerRadius, false, true, true, false))
                using (var brush = new SolidBrush(valueBackColor))
                {
                    g.FillPath(brush, pathRight);
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

            if (roundTopLeft) path.AddArc(tl, 180, 90); else path.AddLine(rect.Left, rect.Top, rect.Left, rect.Top);
            if (roundTopRight) path.AddArc(tr, 270, 90); else path.AddLine(rect.Right, rect.Top, rect.Right, rect.Top);
            if (roundBottomRight) path.AddArc(br, 0, 90); else path.AddLine(rect.Right, rect.Bottom, rect.Right, rect.Bottom);
            if (roundBottomLeft) path.AddArc(bl, 90, 90); else path.AddLine(rect.Left, rect.Bottom, rect.Left, rect.Bottom);

            path.CloseFigure();
            return path;
        }
    }
}
