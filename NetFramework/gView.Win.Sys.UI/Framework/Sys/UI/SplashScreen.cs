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
            lblParseAssembly.Text = lblAddPluginType.Text = String.Empty;

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
            : this(ProductName, demo)
        {
            lblVersion.Visible = true;
            lblVersion.Text = "Version: " + version.Major.ToString() + "." + version.Minor.ToString();

            lblBuild.Text = "Build: " + version.Build.ToString();
            lblBit.Text = (gView.Framework.system.Wow.Is64BitProcess ? "64" : "32") + " Bit";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PlugInManager.OnParseAssembly += OnParseAssembly;
            PlugInManager.OnAddPluginType += OnAddPluginType;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
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
                    //this.Close();
                    lblAddPluginType.Text = lblParseAssembly.Text = String.Empty;
                    timer1.Enabled = true;
                }
                else
                {
                    lblParseAssembly.Text = "Parse: " + assemblyName;
                    lblParseAssembly.Refresh();
                    //Task.Delay(1).Wait();
                }
            }
        }

        private void OnAddPluginType(string pluginType)
        {
            if (this.InvokeRequired)
            {
                PlugInManager.ParseAssemblyDelegate d = new PlugInManager.ParseAssemblyDelegate(OnParseAssembly);
                this.Invoke(d, new object[] { pluginType });
            }
            else
            {
                lblAddPluginType.Text = "Add: " + pluginType;
                //lblAddPluginType.Update();
                lblAddPluginType.Refresh();
                //Task.Delay(1).Wait();
            }
        }
    }
}