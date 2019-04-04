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
        private void Application_Startup(object sender, StartupEventArgs e)
        {

#if (!DEBUG)
            gView.Framework.system.UI.SplashScreen splash = 
                new gView.Framework.system.UI.SplashScreen("Data Explorer", false, SystemVariables.gViewVersion);
            Thread thread = new Thread(new ParameterizedThreadStart(App.SplashScreen));

            thread.Start(splash);
#endif
        }

        static private System.Windows.Forms.Form _splash = null;
        static private void SplashScreen(object splash)
        {
            if (splash is System.Windows.Forms.Form)
            {
                _splash = (System.Windows.Forms.Form)splash;
                _splash.ShowDialog();
            }
        }

        static public void CloseSplash()
        {
            if (_splash != null)
            {
                if (_splash.InvokeRequired)
                {
                    _splash.Invoke(new System.Windows.Forms.MethodInvoker(CloseSplash));
                }
                else
                {
                    _splash.Close();
                    _splash = null;
                }
            }
        }
    }
}
