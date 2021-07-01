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
            Framework.system.PlugInManager.Usage = Framework.system.PluginUsage.Desktop;
            //GraphicsEngine.Current.Engine = new gView.GraphicsEngine.GdiPlus.GraphicsEngine();
            GraphicsEngine.Current.Engine = new gView.GraphicsEngine.Skia.SkiaGraphicsEngine();

            SplashScreen.Show();
            SplashScreen.Refresh();

            base.OnStartup(e);
        }
    }
}