using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace gView.Win.DataExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static gView.Framework.system.UI.SplashScreen SplashScreen = new gView.Framework.system.UI.SplashScreen("gView DataExplorer");


        protected override void OnStartup(StartupEventArgs e)
        {
            Framework.system.PlugInManager.Usage = Framework.system.PluginUsage.Desktop;
            gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.GdiPlus.GraphicsEngine();

            SplashScreen.Show();
            SplashScreen.Refresh();

            base.OnStartup(e);
        }
    }
}
