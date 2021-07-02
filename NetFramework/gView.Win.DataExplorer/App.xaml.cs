using gView.Framework.system;
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
            PlugInManager.Usage = Framework.system.PluginUsage.Desktop;
            GraphicsEngine.Current.Engine = new gView.GraphicsEngine.GdiPlus.GdiGraphicsEngine();

            SplashScreen.Show();
            SplashScreen.Refresh();

            base.OnStartup(e);
        }
    }
}
