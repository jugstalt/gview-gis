using System;
using System.Windows;

namespace gView.Desktop.Wpf.Controls
{
    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            try
            {
                //uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
    }
}
