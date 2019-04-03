using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using gView.Framework.UI;
using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.IO;
using gView.Framework.Geometry;
using gView.Framework.Globalisation;
using System.Threading;
using gView.Plugins.ExTools.Dialogs;
using gView.Framework.UI.Controls.Filter;
using gView.Framework.UI.Dialogs;

namespace gView.Plugins.MapTools
{
    [gView.Framework.system.RegisterPlugIn("97F70651-C96C-4b6e-A6AD-E9D3A22BFD45")]
    public class Copy : ITool, IExTool, IShortCut, IContextMenuItem
    {
        protected IMapDocument _doc = null;
        protected IExplorerApplication _exapp = null;
        protected List<IExplorerObject> _context = null;

        #region ITool Member

        virtual public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Copy", "Copy");
            }
        }

        public bool Enabled
        {
            get
            {
                if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
                {

                    foreach (IGraphicElement element in _doc.FocusMap.Display.GraphicsContainer.SelectedElements)
                    {
                        if (element is IGraphicElement2 && element is IPersistable) return true;
                    }
                }
                if (_context != null && _context.Count > 0)
                {
                    return true;
                }
                else if (_exapp != null && _exapp.SelectedObjects != null && _exapp.SelectedObjects.Count > 0)
                {
                    foreach (var selected in _exapp.SelectedObjects)
                    {
                        if (selected is ISerializableExplorerObject)
                            return true;
                    }
                    //return true;
                }

                return false;
            }
        }

        virtual public string ToolTip
        {
            get { return "Copy"; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        virtual public object Image
        {
            get
            {
                return global::gView.Plugins.Tools.Properties.Resources.copy_;
            }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
            else if (hook is IExplorerApplication)
                _exapp = hook as IExplorerApplication;
        }

        public void OnEvent(object MapEvent)
        {
            //Thread thread = new Thread(new ThreadStart(OnEvent));
            //thread.TrySetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();

            OnEvent();
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.C;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+C";
            }
        }

        #endregion

        #region IContextMenuItem Member

        public bool ShowWith(object context)
        {
            if (context is List<IExplorerObject> && ((List<IExplorerObject>)context).Count > 0)
            {
                _context = (List<IExplorerObject>)context;
                return true;
            }
            _context = null;
            return false;
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 52; }
        }

        #endregion

        virtual protected void OnEvent()
        {
            if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
            {
                List<ExtensionSerializer> elements = new List<ExtensionSerializer>();
                foreach (IGraphicElement element in _doc.FocusMap.Display.GraphicsContainer.SelectedElements)
                {
                    if (element is IGraphicElement2 && element is IPersistable)
                    {
                        ExtensionSerializer ser = new ExtensionSerializer(element);
                        elements.Add(ser);
                    }
                }
                if (elements.Count == 0) return;

                Clipboard.Clear();
                Clipboard.SetDataObject(new DataObject(elements.ToArray()));
            }
            if (_context != null)
            {
                List<ExplorerObjectSerialization> exObjects = new List<ExplorerObjectSerialization>();
                foreach (IExplorerObject exObject in _context)
                {
                    ExplorerObjectSerialization ser = ExplorerObjectManager.SerializeExplorerObject(exObject);
                    if (ser == null) continue;

                    exObjects.Add(ser);
                }

                _context = null;

                Clipboard.Clear();
                Clipboard.SetDataObject(new DataObject(exObjects.ToArray()));

            }
            else if (_exapp != null && _exapp.SelectedObjects != null)
            {
                List<IExplorerObjectSerialization> exObjects = new List<IExplorerObjectSerialization>();
                foreach (IExplorerObject exObject in _exapp.SelectedObjects)
                {
                    IExplorerObjectSerialization ser = ExplorerObjectManager.SerializeExplorerObject(exObject);
                    if (ser == null) continue;

                    exObjects.Add(ser);
                }

                Clipboard.Clear();
                Clipboard.SetDataObject(new DataObject(exObjects.ToArray()));
            }
        }
    }

    [gView.Framework.system.RegisterPlugIn("18fed44c-a8e5-4a80-ad0c-32b9acc21a05")]
    public class CopyTo : Copy
    {
        public override string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.CopyTo", "Copy To...");
            }
        }

        public override object Image
        {
            get
            {
                return global::gView.Plugins.Tools.Properties.Resources.copyto;
            }
        }

        protected override void OnEvent()
        {
            base.OnEvent();

            List<ExplorerDialogFilter> filters = new List<ExplorerDialogFilter>();
            filters.Add(new OpenFeatureDatasetOrFolder());

            ExplorerDialog exDlg = new ExplorerDialog("New Target Featureclass",
                filters,
                true);

            if (exDlg.ShowDialog() == DialogResult.OK &&
                exDlg.ExplorerObjects.Count == 1)
            {
                if (_exapp.MoveToTreeNode(exDlg.ExplorerObjects[0].FullName))
                {
                    Paste paste = new Paste();
                    paste.OnCreate(_exapp);

                    paste.OnEvent(null);
                }
            }
        }
    }

    [gView.Framework.system.RegisterPlugIn("3E36C2AD-2C58-42ca-A662-EE0C2DC1369D")]
    public class Cut : ITool, IExTool, IShortCut, IContextMenuItem
    {
        IMapDocument _doc = null;
        List<IExplorerObject> _context = null;

        #region ITool Member

        public string Name
        {
            get
            {
                return LocalizedResources.GetResString("Tools.Cut", "Cut");
            }
        }

        public bool Enabled
        {
            get
            {
                if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
                {
                    foreach (IGraphicElement element in _doc.FocusMap.Display.GraphicsContainer.SelectedElements)
                    {
                        if (element is IGraphicElement2 && element is IPersistable) return true;
                    }
                }
                return false;
            }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.cut; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
        }

        public void OnEvent(object MapEvent)
        {
            Thread thread = new Thread(new ThreadStart(OnEvent));
            thread.TrySetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.X;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+X";
            }
        }

        #endregion

        #region IContextMenuItem Member

        public bool ShowWith(object context)
        {
            if (context is List<IExplorerObject> && ((List<IExplorerObject>)context).Count > 0)
            {
                _context = (List<IExplorerObject>)context;
                return true;
            }
            _context = null;
            return false;
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 51; }
        }

        #endregion

        private void OnEvent()
        {
            if (_doc == null || _doc.FocusMap == null || _doc.FocusMap.Display == null) return;

            List<ExtensionSerializer> elements = new List<ExtensionSerializer>();
            foreach (IGraphicElement element in _doc.FocusMap.Display.GraphicsContainer.SelectedElements.Clone())
            {
                if (element is IGraphicElement2 && element is IPersistable)
                {
                    ExtensionSerializer ser = new ExtensionSerializer(element);
                    elements.Add(ser);

                    _doc.FocusMap.Display.GraphicsContainer.SelectedElements.Remove(element);
                    _doc.FocusMap.Display.GraphicsContainer.Elements.Remove(element);

                }
            }
            if (elements.Count == 0) return;

            Clipboard.Clear();
            Clipboard.SetDataObject(new DataObject(elements.ToArray()));

            if (_doc.Application is IMapApplication)
                ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
        }
    }

    [gView.Framework.system.RegisterPlugIn("0F8673B3-F1C9-4f5f-86C4-2B41FCBE535B")]
    public class Paste : ITool, IExTool, IShortCut, IContextMenuItem
    {
        IMapDocument _doc = null;
        IExplorerApplication _exapp = null;
        List<IExplorerObject> _context = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Paste", "Paste"); }
        }

        public bool Enabled
        {
            get
            {
                BooleanClass cls = new BooleanClass(false);
                //Thread thread = new Thread(new ParameterizedThreadStart(IsEnabled));
                //thread.TrySetApartmentState(ApartmentState.STA);
                //thread.Start(cls);
                //thread.Join();
                IsEnabled(cls);
                return cls.Value;
            }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.paste_; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
            else if (hook is IExplorerApplication)
                _exapp = hook as IExplorerApplication;
        }

        public void OnEvent(object MapEvent)
        {
            //Thread thread = new Thread(new ThreadStart(OnEvent));
            //thread.TrySetApartmentState(ApartmentState.STA);
            //thread.Start();
            //thread.Join();   
            OnEvent();
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Control | Keys.V;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "Ctrl+V";
            }
        }

        #endregion

        #region IContextMenuItem Member

        public bool ShowWith(object context)
        {
            _context = context as List<IExplorerObject>;
            return true;
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 53; }
        }

        #endregion

        private void OnEvent()
        {
            if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
            {
                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) return;

                foreach (string format in iData.GetFormats())
                {
                    if (!format.Contains("gView.Plugins.MapTools.ExtensionSerializer") &&
                        !format.Contains("gView.Framework.system.ExplorerObjectSerialization") &&
                        !format.Contains("gView.Framework.UI.IExplorerObjectSerialization")) continue;

                    object ob = iData.GetData(format);
                    if (ob is IEnumerable<ExtensionSerializer>)
                    {
                        foreach (ExtensionSerializer ser in (IEnumerable<ExtensionSerializer>)ob)
                        {
                            object extension = ser.Object;

                            if (extension is IGraphicElement2)
                            {
                                IGraphicElement2 element = extension as IGraphicElement2;

                                if (element.Geometry != null)
                                {
                                    Point point = new Point(
                                        (_doc.FocusMap.Display.Envelope.minx + _doc.FocusMap.Display.Envelope.maxx) / 2.0 - (element.Geometry.Envelope.Width) / 2.0,
                                        (_doc.FocusMap.Display.Envelope.miny + _doc.FocusMap.Display.Envelope.maxy) / 2.0 - (element.Geometry.Envelope.Height) / 2.0);

                                    element.Translation(point.X, point.Y);
                                }
                                _doc.FocusMap.Display.GraphicsContainer.Elements.Add(element);
                                _doc.FocusMap.Display.GraphicsContainer.SelectedElements.Add(element);
                            }
                        }
                    }
                }

                if (_doc.Application is IMapApplication)
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
            }
            if (_context != null && _context.Count == 1)
            {
                IExplorerObjectContentDragDropEvents ddEvents = _context[0] as IExplorerObjectContentDragDropEvents;
                _context = null;
                if (ddEvents == null) return;

                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) return;

                foreach (string format in iData.GetFormats())
                {
                    object ob = iData.GetData(format);
                    if (ob is List<IExplorerObjectSerialization>)
                    {
                        DragEventArgs e = new DragEventArgs(iData, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
                        ddEvents.Content_DragDrop(e);
                    }
                }

                if (_exapp != null) _exapp.RefreshContents();
            }
            else if (_exapp != null && _exapp.SelectedObjects != null && _exapp.SelectedObjects.Count == 1)
            {
                IExplorerObjectContentDragDropEvents ddEvents = _exapp.SelectedObjects[0] as IExplorerObjectContentDragDropEvents;
                if (ddEvents == null) return;

                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) return;

                foreach (string format in iData.GetFormats())
                {
                    object ob = iData.GetData(format);
                    if (ob is IEnumerable<IExplorerObjectSerialization>)
                    {
                        DragEventArgs e = new DragEventArgs(iData, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
                        ddEvents.Content_DragDrop(e);
                    }
                }
                _exapp.RefreshContents();
            }
        }

        private void IsEnabled(object obj)
        {
            if (!(obj is BooleanClass))
            {
                return;
            }

            BooleanClass boolean = (BooleanClass)obj;

            IDataObject iData;
            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch
            {
                boolean.Value = false;
                return;
            }
            if (iData == null)
            {
                boolean.Value = false;
                return;
            }
            foreach (string format in iData.GetFormats())
            {
                if (!format.Contains("gView.Plugins.MapTools.ExtensionSerializer") &&
                    !format.Contains("gView.Framework.system.ExplorerObjectSerialization") &&
                    !format.Contains("gView.Framework.UI.IExplorerObjectSerialization")) continue;

                object ob = null;
                try
                {
                    ob = iData.GetData(format);
                }
                catch { }
                if (ob is IEnumerable<ExtensionSerializer> && _doc != null)
                {
                    boolean.Value = true;
                    return;
                }
                else if (ob is IEnumerable<IExplorerObjectSerialization> && _exapp != null && _exapp.SelectedObjects != null && _exapp.SelectedObjects.Count == 1)
                {
                    IExplorerObject exObject = _exapp.SelectedObjects[0];
                    if (exObject is IExplorerObjectContentDragDropEvents)
                    {
                        IExplorerObjectContentDragDropEvents ddEvents = exObject as IExplorerObjectContentDragDropEvents;
                        DragEventArgs e = new DragEventArgs(iData, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
                        ddEvents.Content_DragEnter(e);
                        if (e.Effect == DragDropEffects.Copy)
                        {
                            boolean.Value = true;
                            return;
                        }
                    }
                }
            }

            boolean.Value = false;
            return;
        }
    }

    [gView.Framework.system.RegisterPlugIn("80A04810-62D1-4DD9-94EF-8F67C30CC44B")]
    public class PasteSchema : IExTool, IContextMenuItem
    {
        IExplorerApplication _exapp = null;
        List<IExplorerObject> _context = null;

        #region IExTool Member

        string IExTool.Name
        {
            get { return LocalizedResources.GetResString("Tools.PasteSchema", "Paste Schema"); }
        }

        bool IExTool.Enabled
        {
            get
            {
                BooleanClass cls = new BooleanClass(false);
                IsEnabled(cls);
                return cls.Value;
            }
        }

        string IExTool.ToolTip
        {
            get { return String.Empty; }
        }

        object IExTool.Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.paste; }
        }

        void IExTool.OnCreate(object hook)
        {
            if (hook is IExplorerApplication)
                _exapp = hook as IExplorerApplication;
        }

        void IExTool.OnEvent(object MapEvent)
        {
            OnEvent();
        }

        #endregion

        #region IContextMenuItem Member

        bool IContextMenuItem.ShowWith(object context)
        {
            _context = context as List<IExplorerObject>;
            return true;
        }

        #endregion

        #region IOrder Member

        int IOrder.SortOrder
        {
            get { return 53; }
        }

        #endregion

        private void OnEvent()
        {
            if (_context != null && _context.Count == 1)
            {
                IExplorerObjectContentDragDropEvents2 ddEvents = _context[0] as IExplorerObjectContentDragDropEvents2;
                _context = null;
                if (ddEvents == null) return;

                UserData userData = new UserData();
                userData.SetUserData("gView.Framework.UI.BaseTools.PasteSchema", true);

                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) return;

                foreach (string format in iData.GetFormats())
                {
                    object ob = iData.GetData(format);
                    if (ob is List<IExplorerObjectSerialization>)
                    {
                        DragEventArgs e = new DragEventArgs(iData, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
                        ddEvents.Content_DragDrop(e, userData);
                    }
                }

                if (_exapp != null) _exapp.RefreshContents();
            }
            else if (_exapp != null && _exapp.SelectedObjects != null && _exapp.SelectedObjects.Count == 1)
            {
                IExplorerObjectContentDragDropEvents2 ddEvents = _exapp.SelectedObjects[0] as IExplorerObjectContentDragDropEvents2;
                if (ddEvents == null) return;

                UserData userData = new UserData();
                userData.SetUserData("gView.Framework.UI.BaseTools.PasteSchema", true);

                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) return;

                foreach (string format in iData.GetFormats())
                {
                    object ob = iData.GetData(format);
                    if (ob is IEnumerable<IExplorerObjectSerialization>)
                    {
                        DragEventArgs e = new DragEventArgs(iData, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
                        ddEvents.Content_DragDrop(e, userData);
                    }
                }
                _exapp.RefreshContents();
            }
        }

        private void IsEnabled(object obj)
        {
            if (!(obj is BooleanClass))
            {
                return;
            }

            BooleanClass boolean = (BooleanClass)obj;

            IDataObject iData;
            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch
            {
                boolean.Value = false;
                return;
            }
            if (iData == null)
            {
                boolean.Value = false;
                return;
            }
            foreach (string format in iData.GetFormats())
            {
                if (!format.Contains("gView.Plugins.MapTools.ExtensionSerializer") &&
                    !format.Contains("gView.Framework.system.ExplorerObjectSerialization") &&
                    !format.Contains("gView.Framework.UI.IExplorerObjectSerialization")) continue;

                object ob = null;
                try
                {
                    ob = iData.GetData(format);
                }
                catch { }
                if (ob is IEnumerable<IExplorerObjectSerialization> && _exapp != null && _exapp.SelectedObjects != null && _exapp.SelectedObjects.Count == 1)
                {
                    IExplorerObject exObject = _exapp.SelectedObjects[0];
                    if (exObject is IExplorerObjectContentDragDropEvents)
                    {
                        IExplorerObjectContentDragDropEvents ddEvents = exObject as IExplorerObjectContentDragDropEvents;
                        DragEventArgs e = new DragEventArgs(iData, 0, 0, 0, DragDropEffects.Copy, DragDropEffects.Copy);
                        ddEvents.Content_DragEnter(e);
                        if (e.Effect == DragDropEffects.Copy)
                        {
                            boolean.Value = true;
                            return;
                        }
                    }
                }
            }

            boolean.Value = false;
            return;
        }
    }

    [gView.Framework.system.RegisterPlugIn("4F54D455-1C22-469e-9DBB-78DBBEF6078D")]
    public class Delete : ITool, IExTool, IShortCut, IContextMenuItem
    {
        IMapDocument _doc = null;
        IExplorerApplication _exapp = null;
        private List<IExplorerObject> _context = null;

        #region ITool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Delete", "Delete"); }
        }

        public bool Enabled
        {
            get
            {
                if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
                {

                    if (_doc.FocusMap.Display.GraphicsContainer.SelectedElements.Count != 0)
                        return true;
                }
                if (_context != null)
                {
                    foreach (IExplorerObject exObject in _context)
                    {
                        if (exObject is IExplorerObjectDeletable) return true;
                    }
                }
                else if (_exapp != null && _exapp.SelectedObjects != null)
                {
                    foreach (IExplorerObject exObject in _exapp.SelectedObjects)
                    {
                        if (exObject is IExplorerObjectDeletable) return true;
                    }
                }
                return false;
            }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public ToolType toolType
        {
            get { return ToolType.command; }
        }

        public object Image
        {
            get { return global::gView.Plugins.Tools.Properties.Resources.delete; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IMapDocument)
                _doc = hook as IMapDocument;
            else if (hook is IExplorerApplication)
                _exapp = hook as IExplorerApplication;
        }

        public void OnEvent(object MapEvent)
        {
            if (_doc != null && _doc.FocusMap != null && _doc.FocusMap.Display != null)
            {

                foreach (IGraphicElement element in _doc.FocusMap.Display.GraphicsContainer.SelectedElements)
                {
                    _doc.FocusMap.Display.GraphicsContainer.Elements.Remove(element);
                }
                _doc.FocusMap.Display.GraphicsContainer.SelectedElements.Clear();

                if (_doc.Application is IMapApplication)
                    ((IMapApplication)_doc.Application).RefreshActiveMap(DrawPhase.Graphics);
            }
            if (_context != null)
            {
                FormDeleteExObjects dlg = new FormDeleteExObjects(_context);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (IExplorerObject exObject in dlg.Selected)
                    {
                        if (exObject is IExplorerObjectDeletable)
                            ((IExplorerObjectDeletable)exObject).DeleteExplorerObject(new ExplorerObjectEventArgs());
                    }
                }
                _context = null;
            }
            else if (_exapp != null && _exapp.SelectedObjects != null)
            {
                FormDeleteExObjects dlg = new FormDeleteExObjects(_exapp.SelectedObjects);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (IExplorerObject exObject in dlg.Selected)
                    {
                        if (exObject is IExplorerObjectDeletable)
                            ((IExplorerObjectDeletable)exObject).DeleteExplorerObject(new ExplorerObjectEventArgs());
                    }
                }
            }

            if (_exapp != null)
                _exapp.RefreshContents();
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get
            {
                return Keys.Delete;
            }
        }

        public string ShortcutKeyDisplayString
        {
            get
            {
                return "DEL";
            }
        }

        #endregion

        #region IContextMenuItem Member

        public bool ShowWith(object context)
        {
            if (context is List<IExplorerObject> && ((List<IExplorerObject>)context).Count > 0)
            {
                _context = (List<IExplorerObject>)context;
                return true;
            }
            _context = null;
            return false;
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 54; }
        }

        #endregion
    }

    [gView.Framework.system.RegisterPlugIn("91F17A8E-D859-4840-A287-7FDE14152CB1")]
    public class Rename : IExTool, IShortCut, IContextMenuItem
    {
        private IExplorerApplication _exapp = null;
        private List<IExplorerObject> _context = null;

        #region IExTool Member

        public string Name
        {
            get { return LocalizedResources.GetResString("Tools.Rename", "Rename"); }
        }

        public bool Enabled
        {
            get
            {
                if (_context != null && _context.Count == 1)
                {
                    return _context[0] is IExplorerObjectRenamable;
                }
                else if (_exapp != null && _exapp.SelectedObjects != null &&
                    _exapp.SelectedObjects.Count == 1)
                {
                    foreach (IExplorerObject exObject in _exapp.SelectedObjects)
                    {
                        if (exObject is IExplorerObjectRenamable) return true;
                    }
                }
                return false;
            }
        }

        public string ToolTip
        {
            get { return ""; }
        }

        public object Image
        {
            get { return null; }
        }

        public void OnCreate(object hook)
        {
            if (hook is IExplorerApplication)
                _exapp = hook as IExplorerApplication;
        }

        public void OnEvent(object MapEvent)
        {
            if (_context != null && _context.Count == 1)
            {
                FormRenameExObject dlg = new FormRenameExObject(_context[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {

                }
                _context = null;
            }
            else if (_exapp != null && _exapp.SelectedObjects != null &&
                _exapp.SelectedObjects.Count == 1)
            {
                FormRenameExObject dlg = new FormRenameExObject(_exapp.SelectedObjects[0]);
                if (dlg.ShowDialog() == DialogResult.OK)
                {

                }
            }

            if (_exapp != null)
                _exapp.RefreshContents();
        }

        #endregion

        #region IShortCut Member

        public Keys ShortcutKeys
        {
            get { return Keys.F2; }
        }

        public string ShortcutKeyDisplayString
        {
            get { return "F2"; }
        }

        #endregion

        #region IContextMenuItem Member

        public bool ShowWith(object context)
        {
            if (context is List<IExplorerObject> && ((List<IExplorerObject>)context).Count == 1)
            {
                _context = (List<IExplorerObject>)context;
                return true;
            }
            _context = null;
            return false;
        }

        #endregion

        #region IOrder Member

        public int SortOrder
        {
            get { return 61; }
        }

        #endregion
    }

    [Serializable]
    public class ExtensionSerializer
    {
        private string _string = "";
        private System.Guid _guid;

        public ExtensionSerializer(object extension)
        {
            if (PlugInManager.IsPlugin(extension) &&
                extension is IPersistable)
            {
                XmlStream stream = new XmlStream("");

                ((IPersistable)extension).Save(stream);

                MemoryStream ms = new MemoryStream();
                stream.WriteStream(ms);
                ms.Position = 0;
                byte[] b = new byte[ms.Length];
                ms.Read(b, 0, (int)ms.Length);

                _string = System.Text.Encoding.Unicode.GetString(b).Trim();
                _guid = PlugInManager.PlugInID(extension);
            }
        }

        public object Object
        {
            get
            {
                try
                {
                    if (_string == "" || _string == null) return null;

                    PlugInManager compMan = new PlugInManager();
                    object extension = compMan.CreateInstance(_guid);
                    if (!(extension is IPersistable)) return null;

                    byte[] b = System.Text.Encoding.Unicode.GetBytes(_string);
                    XmlStream stream = new XmlStream("");

                    MemoryStream ms = new MemoryStream();
                    ms.Write(b, 0, b.Length);
                    ms.Position = 0;

                    stream.ReadStream(ms);
                    ((IPersistable)extension).Load(stream);
                    return extension;
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    internal class BooleanClass
    {
        private bool _bool;

        public BooleanClass(bool val)
        {
            _bool = val;
        }

        public bool Value
        {
            get { return _bool; }
            set { _bool = value; }
        }
    }
}
