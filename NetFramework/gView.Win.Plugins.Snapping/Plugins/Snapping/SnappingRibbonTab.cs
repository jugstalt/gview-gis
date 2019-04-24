using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using System.Windows;
using gView.Framework.system.UI;

namespace gView.Plugins.Snapping
{
    [gView.Framework.system.RegisterPlugIn("5d4501c0-bc51-493a-a87f-a7a35747611a")]
    public class SnappingRibbonTab : ICartoRibbonTab
    {
        private List<RibbonGroupBox> _groups;

        public SnappingRibbonTab()
        {
            _groups = new List<RibbonGroupBox>(
                new RibbonGroupBox[]{
                    new RibbonGroupBox(String.Empty,
                        new RibbonItem[] {
                            new RibbonItem(new Guid("9CDE6BD1-317E-478b-8828-B169A6688CC5")),  
                            new RibbonItem(new Guid("D678C377-79A0-49e5-88A3-6635FD7B522C"))
                        }
                       ) { OnLauncherClick=OnSnappingLauncherClick }
                }
                );
        }

        #region ICartoRibbonTab Member

        public string Header
        {
            get { return "Snapping"; }
        }

        public List<RibbonGroupBox> Groups
        {
            get { return _groups; }
        }

        public int SortOrder
        {
            get { return 310; }
        }

        public bool IsVisible(IMapDocument mapDocument)
        {
            return true;
        }

        #endregion

        private void OnSnappingLauncherClick(object sender, RoutedEventArgs e)
        {
            if (e is RibbonGroupBox.LauncherClickEventArgs && ((RibbonGroupBox.LauncherClickEventArgs)e).Hook is IMapDocument)
            {
                IMapDocument mapDocument = (IMapDocument)((RibbonGroupBox.LauncherClickEventArgs)e).Hook;
                IMapApplication mapApplication = mapDocument.Application as IMapApplication;
                if (mapApplication != null)
                {
                    Module module = mapApplication.IMapApplicationModule(new Guid(gView.Framework.Snapping.Core.Globals.ModuleGuidString)) as Module;

                    if (!AppUIGlobals.IsAppReadOnly(mapDocument.Application))
                    {
                        FormSnappingLauncher dlg = new FormSnappingLauncher(module);
                        dlg.ShowDialog();
                    }
                }
            }
        }
    }
}
