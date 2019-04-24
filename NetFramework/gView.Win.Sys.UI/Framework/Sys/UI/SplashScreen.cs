using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.system.UI
{
    public partial class SplashScreen : Form
    {
        //private string _productName = "";
        public SplashScreen(string productName)
        {
            InitializeComponent();

            lblProductName.Text = productName;

            //_productName = ProductName;

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

        private EventHandler OnPluginsLoaded = null;

        public void SetOnPluginsLoadedHandler(EventHandler handler)
        {
            if (this.Visible == false)
            {
                handler(this, new EventArgs());
            }
            else
            {
                OnPluginsLoaded = handler;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                PlugInManager.Init(OnParseAssembly);
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //this.Close();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //timer1.Enabled = true;

        }

        private void OnParseAssembly(string assemblyName)
        {
            if (this.InvokeRequired)
            {
                PlugInManager.ParseAssemblyDelegate d = new PlugInManager.ParseAssemblyDelegate(OnParseAssembly);
                this.Invoke(d, new object[] { assemblyName });
            }
            else
            {
                if (String.IsNullOrWhiteSpace(assemblyName))
                {
                    this.Close();
                    if(OnPluginsLoaded!=null)
                    {
                        OnPluginsLoaded(this, new EventArgs());
                    }
                }
                else
                {
                    lblParseAssembly.Text = "Parse: " + assemblyName;
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            //if (e.Graphics != null)
            //{
            //    using (Font font = new Font("Verdana", 22, FontStyle.Bold))
            //    {
            //        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //        e.Graphics.DrawString("gView GIS", font, Brushes.Black, 5, 185);
            //    }
            //    using (Pen pen = new Pen(Color.Black))
            //    {
            //        e.Graphics.DrawLine(pen, 10, 225, 390, 225);
            //    }
            //    using (Font font = new Font("Verdana", 16, FontStyle.Bold))
            //    {
            //        float w = e.Graphics.MeasureString(_productName, font).Width;
            //        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            //        e.Graphics.DrawString(_productName, font, Brushes.Black, 395f - w, 225f);
            //    }
            //}
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}