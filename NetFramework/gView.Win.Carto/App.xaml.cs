using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace gView.Win.Carto
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static gView.Framework.system.UI.SplashScreen SplashScreen = new gView.Framework.system.UI.SplashScreen("gView Carto");

        protected override void OnStartup(StartupEventArgs e)
        {
            SplashScreen.Show();

            base.OnStartup(e);
        }
    }
}
