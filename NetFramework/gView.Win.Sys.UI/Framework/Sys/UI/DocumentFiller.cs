using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
//using WeifenLuo.WinFormsUI.Docking;

//namespace gView.Framework.system.UI
//{
//    public class DocumentFiller
//    {
//        private DocumentTabList _tabPages;
//        private DocumentTab _selectedTab = null;
//        private DockPanel _dockPanel;

//        public DocumentFiller(DockPanel dockPanel)
//        {
//            _dockPanel = dockPanel;

//            _tabPages = new DocumentTabList(dockPanel);
//        }

//        public DocumentTabList TabPages
//        {
//            get { return _tabPages; }
//        }

//        public DocumentTab SelectedTab
//        {
//            get
//            {
//                if (_selectedTab == null && _tabPages.Count > 0)
//                    return _tabPages[0];

//                return _selectedTab;
//            }
//        }

//        public class DocumentTabList
//        {
//            private DockPanel _dockPanel;
//            private List<DocumentTab> _list = new List<DocumentTab>();

//            public DocumentTabList(DockPanel dockPanel)
//            {
//                _dockPanel = dockPanel;
//            }

//            public void Add(DocumentTab tab)
//            {
//                if (tab == null || _list.Contains(tab)) return;

//                //tab.ShowDialog();
//                if (_list.Count == 0)
//                {
//                    tab.Show(_dockPanel, DockState.Document);
//                }
//                else
//                {
//                    tab.Show(_list[0].Pane, null);
//                }
//                _list.Add(tab);
//            }

//            public void Clear()
//            {
//                foreach (DocumentTab tab in _list)
//                {
//                    tab.Hide();
//                }
//                _list.Clear();
//            }

//            public DocumentTab this[int index]
//            {
//                get
//                {
//                    if (index < 0 || index >= _list.Count)
//                        throw new ArgumentException("Index out of bounds");

//                    return _list[index];
//                }
//            }

//            public int Count
//            {
//                get { return _list.Count; }
//            }
//        }
//    }
//}
