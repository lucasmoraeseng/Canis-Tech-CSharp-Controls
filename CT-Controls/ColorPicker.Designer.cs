
namespace CT_Controls
{
    partial class ColorPicker
    {
        /// <summary> 
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Designer de Componentes

        /// <summary> 
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this._circularPicker = new CT_Controls.ColorPickerCircular();
            this._wheel = new CT_Controls.ColorWheel();
            this._circularPicker.SuspendLayout();
            this.SuspendLayout();
            // 
            // _circularPicker
            // 
            this._circularPicker.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._circularPicker.BackColor = System.Drawing.Color.Transparent;
            this._circularPicker.Controls.Add(this._wheel);
            this._circularPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this._circularPicker.Hue = 0F;
            this._circularPicker.Location = new System.Drawing.Point(0, 0);
            this._circularPicker.MinimumSize = new System.Drawing.Size(50, 50);
            this._circularPicker.Name = "_circularPicker";
            this._circularPicker.Padding = new System.Windows.Forms.Padding(10);
            this._circularPicker.Size = new System.Drawing.Size(50, 50);
            this._circularPicker.TabIndex = 1;
            // 
            // _wheel
            // 
            this._wheel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._wheel.BackColor = System.Drawing.Color.Transparent;
            this._wheel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._wheel.Location = new System.Drawing.Point(10, 10);
            this._wheel.MinimumSize = new System.Drawing.Size(20, 20);
            this._wheel.Name = "_wheel";
            this._wheel.R = 6.857097E-19F;
            this._wheel.SelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(127)))), ((int)(((byte)(127)))));
            this._wheel.Size = new System.Drawing.Size(50, 50);
            this._wheel.TabIndex = 0;
            this._wheel.Theta = 18.43494F;
            // 
            // ColorPicker
            // 
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this._circularPicker);
            this.DoubleBuffered = true;
            this.Name = "ColorPicker";
            this.Size = new System.Drawing.Size(50, 50);
            this._circularPicker.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ColorWheel _wheel;
        private ColorPickerCircular _circularPicker;
    }
}
