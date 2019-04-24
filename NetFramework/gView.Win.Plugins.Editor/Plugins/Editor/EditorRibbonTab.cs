using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Data;
using gView.Framework.FDB;
using System.Windows;
using gView.Framework.Editor.Core;
using gView.Plugins.Editor.Dialogs;
using gView.Framework.system.UI;

namespace gView.Plugins.Editor.Editor
{
    [gView.Framework.system.RegisterPlugIn("5434ca46-fc3a-466f-a9d5-88e8d5452bd6")]
    public class EditorRibbonTab : ICartoRibbonTab
    {
        private List<RibbonGroupBox> _groups;

        public EditorRibbonTab()
        {
            _groups = new List<RibbonGroupBox>(
                new RibbonGroupBox[]{
                    new RibbonGroupBox("Edit",
                        new RibbonItem[] {
                            //new RibbonItem(new Guid("784148EB-04EA-413d-B11A-1A0F9A7EA4A0")),  
                            new RibbonItem(new Guid("3C8A7ABC-B535-43d8-8F2D-B220B298CB17")),  
                            //new RibbonItem(new Guid("19396559-C13C-486c-B5F7-73DD5B12D5A8")), 
                            new RibbonItem(new Guid("9B7D5E0E-88A5-40e2-977B-8A2E21875221"))
                        }
                        ) { OnLauncherClick=OnEditLauncherClick },
                    new RibbonGroupBox(String.Empty,
                        new RibbonItem[]{
                            new RibbonItem(new Guid("91392106-2C28-429c-8100-1E4E927D521C")),
                            new RibbonItem(new Guid("3B64107F-00C8-4f4a-B781-163FE9DA2D4B"),"Middle"),
                            new RibbonItem(new Guid("FD340DE3-0BC1-4b3e-99D2-E8DCD55A46F2"),"Middle"),  
                            new RibbonItem(new Guid("B576D3F9-F7C9-46d5-8A8C-16B3974F1BD7"),"Middle") 
                        }
                        ),
                    new RibbonGroupBox(String.Empty, 
                        new RibbonItem[]{
                            new RibbonItem(new Guid("96099E8C-163E-46ec-BA33-41696BFAE4D5")),
                            new RibbonItem(new Guid("AC4620D4-3DE4-49ea-A902-0B267BA46BBF")),
                            new RibbonItem(new Guid("11DEE52F-F241-406e-BB40-9F247532E43D"))
                            
                        }
                        )
                }
                );
        }

        #region ICartoRibbonTab Member

        public string Header
        {
            get { return "Editor"; }
        }

        public List<RibbonGroupBox> Groups
        {
            get { return _groups; }
        }

        public int SortOrder
        {
            get { return 300; }
        }

        public bool IsVisible(IMapDocument mapDocument)
        {
            if (mapDocument == null || mapDocument.FocusMap == null)
                return false;

            foreach (IDataset dataset in mapDocument.FocusMap.Datasets)
            {
                if (dataset.Database is IFeatureUpdater)
                    return true;
            }

            return false;
        }

        #endregion

        private void OnEditLauncherClick(object sender, RoutedEventArgs e)
        {
            if (e is RibbonGroupBox.LauncherClickEventArgs && ((RibbonGroupBox.LauncherClickEventArgs)e).Hook is IMapDocument)
            {
                IMapDocument mapDocument = (IMapDocument)((RibbonGroupBox.LauncherClickEventArgs)e).Hook;
                IMapApplication mapApplication = mapDocument.Application as IMapApplication;
                if (mapApplication != null)
                {
                    Module module=mapApplication.IMapApplicationModule(Globals.ModuleGuid) as Module;

                    if (!AppUIGlobals.IsAppReadOnly(mapDocument.Application))
                    {
                        FormEditLauncher dlg = new FormEditLauncher(module);
                        dlg.ShowDialog();
                    }
                }
            }
        }
    }
}
