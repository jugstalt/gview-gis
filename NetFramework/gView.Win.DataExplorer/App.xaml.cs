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
            SplashScreen.Show();
            SplashScreen.Refresh();

            base.OnStartup(e);
        }
    }
}
