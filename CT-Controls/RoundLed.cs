using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CT_Controls
{
    [ToolboxItem(true)]
    [DefaultProperty("IsOn")]
    public partial class RoundLed : UserControl
    {
        private Color onColor = Color.LimeGreen;
        private Color offColor;
        private bool offColorSet;
        private bool isOn;

        public RoundLed()
        {
            InitializeComponent();
            // default off color is a darker variant of onColor unless user sets OffColor explicitly
            offColor = ControlPaint.Dark(onColor);
            DoubleBuffered = true;
        }

        [Category("LED"), Description("Color used when the LED is ON.")]
        [DefaultValue(typeof(Color), "LimeGreen")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color OnColor
        {
            get => onColor;
            set
            {
                if (onColor == value) return;
                onColor = value;
                if (!offColorSet)
                {
                    offColor = ControlPaint.Dark(onColor);
                }
                Invalidate();
            }
        }

        [Category("LED"), Description("Color used when the LED is OFF. If not set explicitly, it will be calculated as a darker variant of OnColor.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(true)]
        public Color OffColor
        {
            get => offColor;
            set
            {
                if (offColor == value) return;
                offColor = value;
                offColorSet = true;
                Invalidate();
            }
        }

        // Designer helper: serialize OffColor only when user actually changed it.
        public bool ShouldSerializeOffColor() => offColorSet;
        public void ResetOffColor()
        {
            offColorSet = false;
            offColor = ControlPaint.Dark(onColor);
            Invalidate();
        }

        [Category("LED"), Description("LED state. True = ON, False = OFF.")]
        [DefaultValue(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsOn
        {
            get => isOn;
            set
            {
                if (isOn == value) return;
                isOn = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Protect designer: swallow any drawing-time exceptions so designer won't crash.
            try
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Calculate drawing rect with a small padding so border is visible
                var rect = ClientRectangle;
                const int padding = 4;
                rect.Inflate(-padding, -padding);

                if (rect.Width <= 0 || rect.Height <= 0) return;

                // Keep it circular: use smallest side
                int diameter = Math.Min(rect.Width, rect.Height);
                var ledRect = new Rectangle(
                    rect.Left + (rect.Width - diameter) / 2,
                    rect.Top + (rect.Height - diameter) / 2,
                    diameter,
                    diameter);

                Color baseColor = isOn ? onColor : offColor;

                // create a soft highlight using a PathGradientBrush for a nice LED look
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(ledRect);

                    using (var pgb = new PathGradientBrush(path))
                    {
                        // lighter center for glow
                        pgb.CenterColor = ControlPaint.LightLight(baseColor);
                        pgb.SurroundColors = new Color[] { baseColor };
                        // slight offset to simulate light source
                        pgb.CenterPoint = new PointF(
                            ledRect.Left + ledRect.Width * 0.35f,
                            ledRect.Top + ledRect.Height * 0.35f);

                        g.FillEllipse(pgb, ledRect);
                    }

                    // subtle inner ring
                    using (var innerPen = new Pen(ControlPaint.Light(baseColor), Math.Max(1f, diameter * 0.04f)))
                    {
                        g.DrawEllipse(innerPen, ledRect);
                    }

                    // outer border
                    using (var borderPen = new Pen(ControlPaint.Dark(baseColor), 1f))
                    {
                        g.DrawEllipse(borderPen, ledRect);
                    }
                }
            }
            catch
            {
                // swallow exceptions in design-time to avoid breaking Visual Studio designer
            }
        }
    }
}
