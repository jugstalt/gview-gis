using gView.GraphicsEngine;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace gView.Framework.Symbology.UI
{
    /// <summary>
    /// Zusammenfassung für Form_UITypeEditor_Character.
    /// </summary>
    internal class Form_UITypeEditor_Character : System.Windows.Forms.Form
    {
        public System.Windows.Forms.Panel panelChars;
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private IWindowsFormsEditorService _wfes;
        private Panel panel1;
        private byte _c;

        public Form_UITypeEditor_Character(IWindowsFormsEditorService wfes, GraphicsEngine.Abstraction.IFont iFont, byte charakter)
        {
            //
            // Erforderlich für die Windows Form-Designerunterstützung
            //
            InitializeComponent();

            _wfes = wfes;
            _c = charakter;

            if (iFont != null)
            {
                int size = 50;
                panelChars.Width = panelChars.Height = size * 16;

                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        Button btn = new Button();
                        btn.Name = (y * 16 + x).ToString();

                        btn.Width = btn.Height = size;
                        btn.Top = y * size;
                        btn.Left = x * size;
                        btn.FlatStyle = FlatStyle.Popup;

                        btn.Paint += (sender, e) =>
                        {
                            var p = (Button)sender;
                            using (var bitmap = Current.Engine.CreateBitmap(size, size))
                            using (var canvas = bitmap.CreateCanvas())
                            using (var blackBrusn = Current.Engine.CreateSolidBrush(ArgbColor.Black))
                            using (var yellowBrush = Current.Engine.CreateSolidBrush(ArgbColor.Yellow))
                            using (var font = Current.Engine.CreateFont(iFont.Name, 12))
                            {
                                var c = (char)int.Parse(p.Name);

                                if (c == charakter)
                                {
                                    canvas.FillRectangle(yellowBrush, new CanvasRectangle(0, 0, size, size));
                                }

                                var drawTextFormat = Current.Engine.CreateDrawTextFormat();
                                drawTextFormat.Alignment = StringAlignment.Center;
                                drawTextFormat.LineAlignment = StringAlignment.Center;

                                canvas.DrawText(c.ToString(), font, blackBrusn,
                                     new CanvasPoint(16, 16), drawTextFormat);

                                using (var ms = new MemoryStream())
                                {
                                    bitmap.Save(ms, ImageFormat.Png);
                                    using (var bm = System.Drawing.Bitmap.FromStream(ms))
                                    {
                                        e.Graphics.DrawImage(bm, 0, 0);
                                    }
                                }
                            }
                        };

                        btn.Click += new EventHandler(btn_Click);
                        panelChars.Controls.Add(btn);
                    }
                }
            }
        }

        public byte Charakter
        {
            get { return _c; }
        }
        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code
        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelChars = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panelChars
            // 
            this.panelChars.BackColor = System.Drawing.Color.Silver;
            this.panelChars.Location = new System.Drawing.Point(26, 35);
            this.panelChars.Name = "panelChars";
            this.panelChars.Size = new System.Drawing.Size(1024, 256);
            this.panelChars.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(178, 443);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(261, 166);
            this.panel1.TabIndex = 1;
            // 
            // Form_UITypeEditor_Character
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(8, 19);
            this.ClientSize = new System.Drawing.Size(1358, 814);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelChars);
            this.Name = "Form_UITypeEditor_Character";
            this.Text = "Form_UITypeEditor_Character";
            this.ResumeLayout(false);

        }
        #endregion

        private void btn_Click(object sender, System.EventArgs e)
        {
            if (!(sender is Button))
            {
                return;
            }

            _c = (byte)int.Parse(((Button)sender).Name);
            _wfes.CloseDropDown();
        }
    }
}
