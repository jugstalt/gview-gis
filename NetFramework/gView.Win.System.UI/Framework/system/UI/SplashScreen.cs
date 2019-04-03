using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace gView.Framework.system.UI
{
    public partial class SplashScreen : Form
    {
        private string _productName = "";
        public SplashScreen(string ProductName)
        {
            InitializeComponent();

            _productName = ProductName;

            //lblUsername.Text += " " + SystemVariables.InsallationUsername;
            //lblCompany.Text += " " + SystemVariables.InsallationCompanyname;
        }
        public SplashScreen(String ProductName, bool demo)
            : this(ProductName)
        {
            lblDemo.Visible = demo;
        }
        public SplashScreen(string ProductName, bool demo, Version version)
            :
            this(ProductName, demo)
        {
            lblVersion.Visible = true;
            lblVersion.Text = "Version: " + version.Major.ToString() + "." + version.Minor.ToString();

            lblBuild.Text = "Build: " + version.Build.ToString();
            lblBit.Text = (gView.Framework.system.Wow.Is64BitProcess ? "64" : "32") + " Bit";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //timer1.Enabled = true;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (e.Graphics != null)
            {
                using (Font font = new Font("Verdana", 22, FontStyle.Bold))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawString("gView GIS", font, Brushes.Black, 5, 185);
                }
                using (Pen pen = new Pen(Color.Black))
                {
                    e.Graphics.DrawLine(pen, 10, 225, 390, 225);
                }
                using (Font font = new Font("Verdana", 16, FontStyle.Bold))
                {
                    float w = e.Graphics.MeasureString(_productName, font).Width;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawString(_productName, font, Brushes.Black, 395f - w, 225f);
                }
            }
        }


    }
}