using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gView.Framework.UI;
using gView.Framework.UI.Events;

namespace gView.Plugins.MapTools.Controls
{
    /// <summary>
    /// Interaktionslogik für NewToolControl.xaml
    /// </summary>
    public partial class NewToolControl : UserControl
    {
        private IMapDocument _mapDocument;
        public event EventHandler OnButtonClick = null;

        public NewToolControl(IMapDocument mapDocument)
        {
            InitializeComponent();

            _mapDocument = mapDocument;
        }

        private void btnNewDocument_Click(object sender, RoutedEventArgs e)
        {
            if (_mapDocument == null)
                return;

            if (OnButtonClick == null)
            {
                NewDocument tool = new NewDocument();
                tool.OnCreate(_mapDocument);

                tool.OnEvent(new MapEvent(_mapDocument.FocusMap));
            }

            HideBackstageMenu();
        }

        private void btnLoadMapDocument_Click(object sender, RoutedEventArgs e)
        {
            if (_mapDocument == null)
                return;

            HideBackstageMenu();

            LoadDocument tool = new LoadDocument();
            tool.OnCreate(_mapDocument);

            tool.OnEvent(new MapEvent(_mapDocument.FocusMap));
        }

        private void btnNewMap_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddData_Click(object sender, RoutedEventArgs e)
        {
            if (_mapDocument == null)
                return;

            HideBackstageMenu();

            AddData tool = new AddData();
            tool.OnCreate(_mapDocument);

            tool.OnEvent(new MapEvent(_mapDocument.FocusMap));
        }

        private void HideBackstageMenu()
        {
            if (OnButtonClick != null)
                OnButtonClick(this, new EventArgs());
            else
            {
                if (_mapDocument != null && _mapDocument.Application is IGUIApplication)
                {
                    ((IGUIApplication)_mapDocument.Application).HideBackstageMenu();
                }
            }
        }    
    }
}
