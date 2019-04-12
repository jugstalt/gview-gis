using gView.Framework.Carto;
using gView.Framework.Data;
using gView.Framework.Geometry;
using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gView.Framework.UI
{
    public interface IPropertyDialog
    {
        System.Windows.Forms.DialogResult Show();
        object PropertyDialogObject();
    }

    public interface IPage
    {
        void Commit();
    }

    public interface IMapOptionPage : IPage
    {
        System.Windows.Forms.Panel OptionPage(IMapDocument document);

        string Title { get; }
        System.Drawing.Image Image { get; }

        bool IsAvailable(IMapDocument document);
    }

    public interface ILayerPropertyPage : IPage
    {
        System.Windows.Forms.Panel PropertyPage(IDataset dataset, ILayer layer);

        bool ShowWith(IDataset dataset, ILayer layer);
        string Title { get; }
    }

    public interface IExplorerOptionPage : IPage
    {
        System.Windows.Forms.Panel OptionPage();

        string Title { get; }
        System.Drawing.Image Image { get; }
    }

    public interface IShortCut
    {
        Keys ShortcutKeys { get; }
        string ShortcutKeyDisplayString { get; }
    }

    public interface IToolMouseActions
    {
        void MouseDown(IDisplay display, MouseEventArgs e, IPoint world);
        void MouseUp(IDisplay display, MouseEventArgs e, IPoint world);
        void MouseClick(IDisplay display, MouseEventArgs e, IPoint world);
        void MouseDoubleClick(IDisplay display, MouseEventArgs e, IPoint world);
        void MouseMove(IDisplay display, MouseEventArgs e, IPoint world);
        void MouseWheel(IDisplay display, MouseEventArgs e, IPoint world);
    }
    public interface IToolMouseActions2
    {
        void BeforeShowContext(IDisplay display, MouseEventArgs e, IPoint world);
    }

    public interface IToolKeyActions
    {
        void KeyDown(IDisplay display, KeyEventArgs e);
        void KeyPress(IDisplay display, KeyPressEventArgs e);
        void KeyUp(IDisplay display, KeyEventArgs e);
    }

    public interface IConstructable
    {
        void ConstructMouseDown(IMapDocument doc, MouseEventArgs e);
        void ConstructMouseUp(IMapDocument doc, MouseEventArgs e);
        void ConstructMouseClick(IMapDocument doc, MouseEventArgs e);
        void ConstructMouseDoubleClick(IMapDocument doc, MouseEventArgs e);
        void ConstructMouseMove(IMapDocument doc, MouseEventArgs e);
        void ConstructMouseWheel(IMapDocument doc, MouseEventArgs e);

        bool hasVertices { get; }
    }

    public interface IToolItem
    {
        System.Windows.Forms.ToolStripItem ToolItem { get; }
    }

    public enum ToolItemLabelPosition { top = 0, bottom = 1,left=2,right=3 }
    public interface IToolItemLabel
    {
        string Label { get; }
        ToolItemLabelPosition LabelPosition { get; }
    }

    //public enum ToolbarPanel { top = 0, bottom = 1, left = 2, right = 3, flow = 4 }

    public interface IToolbar : IPersistable
    {
        //bool Visible { get; set; }
        //ToolbarPanel ParentPanel { get; set; }
        string Name { get; }

        List<System.Guid> GUIDs { get; set; }
    }

    public class RibbonItem
    {
        public RibbonItem(Guid guid)
            : this(guid, String.Empty)
        {
        }
        public RibbonItem(Guid guid, string sizeDefinition)
        {
            this.Guid = guid;
            SizeDefinition = sizeDefinition;
        }

        public Guid Guid { get; private set; }
        public string SizeDefinition { get; private set; }
    }
    public class RibbonGroupBox
    {
        public System.Windows.RoutedEventHandler OnLauncherClick = null;

        public RibbonGroupBox(string header)
        {
            Header = header;
            Items = new List<RibbonItem>();
        }
        public RibbonGroupBox(string header, RibbonItem[] items)
        {
            Header = header;
            Items = new List<RibbonItem>(items);
        }

        public string Header { get; private set; }
        public List<RibbonItem> Items;

        public class LauncherClickEventArgs : System.Windows.RoutedEventArgs
        {
            public LauncherClickEventArgs(object hook)
            {
                Hook = hook;
            }

            public object Hook { get; private set; }
        }
    }

    public interface ICartoRibbonTab : IOrder
    {
        string Header { get; }
        List<RibbonGroupBox> Groups { get; }
        bool IsVisible(IMapDocument mapDocument);
    }
    public interface IExplorerRibbonTab
    {
        string Header { get; }
        List<RibbonGroupBox> Groups { get; }
        int Order { get; }
    }

    public interface IExToolbar : IPersistable
    {
        //bool Visible { get; set; }
        //ToolbarPanel ParentPanel { get; set; }
        string Name { get; }

        List<System.Guid> GUIDs { get; set; }
    }

    public interface IToolContextMenu
    {
        ContextMenuStrip ContextMenu { get; }
    }

    public interface IToolWindow
    {
        IDockableWindow ToolWindow { get; }
    }

    public interface IDocumentWindow
    {
        void RefreshControls(DrawPhase phase);
        void AddChildWindow(System.Windows.Forms.Form child);
        System.Windows.Forms.Form GetChildWindow(string Title);
    }

    public interface IGUIAppWindow : IWin32Window
    {

    }

    public interface IGUIApplication : IApplication
    {
        event DockWindowAddedEvent DockWindowAdded;
        event OnShowDockableWindowEvent OnShowDockableWindow;

        void AddDockableWindow(IDockableWindow window, DockWindowState state);
        void AddDockableWindow(IDockableWindow window, string ParentDockableWindowName);
        void ShowDockableWindow(IDockableWindow window);

        List<IDockableWindow> DockableWindows
        {
            get;
        }

        //List<object> ToolStrips
        //{
        //    get;
        //}
        ITool Tool(Guid guid);
        List<ITool> Tools { get; }
        ITool ActiveTool { get; set; }

        IToolbar Toolbar(Guid guid);
        List<IToolbar> Toolbars { get; }

        IGUIAppWindow ApplicationWindow { get; }
        void ValidateUI();

        IStatusBar StatusBar { get; }

        void SetCursor(object cursor);

        void AppendContextMenuItems(ContextMenuStrip strip, object context);

        void ShowBackstageMenu();
        void HideBackstageMenu();
    }

    public interface IMapApplication : IGUIApplication
    {
        event AfterLoadMapDocumentEvent AfterLoadMapDocument;
        event ActiveMapToolChangedEvent ActiveMapToolChanged;
        event OnCursorPosChangedEvent OnCursorPosChanged;

        Task LoadMapDocument(string filename);
        void SaveMapDocument();
        void SaveMapDocument(string filename, bool performEncryption);

        string DocumentFilename
        {
            get;
            set;
        }
        
        Task RefreshActiveMap(DrawPhase drawPhase);
        Task RefreshTOC();
        void RefreshTOCElement(IDatasetElement element);

        IDocumentWindow DocumentWindow
        {
            get;
        }

        IMapApplicationModule IMapApplicationModule(Guid guid);

        bool ReadOnly { get; }

        //void DrawReversibleGeometry(IGeometry geometry, System.Drawing.Color color);
    }

    public delegate Task ExplorerExecuteBatchCallback();
    public interface IExplorerApplication : IGUIApplication
    {
        Task ExecuteBatch(string batch, ExplorerExecuteBatchCallback callback);

        List<IExplorerObject> SelectedObjects { get; }
        bool MoveToTreeNode(string path);
        void RefreshContents();
    }

    public interface IExplorerObjectContextMenu
    {
        ToolStripItem[] ContextMenuItems { get; }
    }

    public delegate Task<bool> RefreshContextDelegate();
    public interface IExplorerObjectContextMenu2
    {
        ToolStripItem[] ContextMenuItems(RefreshContextDelegate callback);
    }

    public interface IExplorerObjectContentDragDropEvents
    {
        void Content_DragDrop(System.Windows.Forms.DragEventArgs e);
        void Content_DragEnter(System.Windows.Forms.DragEventArgs e);
        void Content_DragLeave(System.EventArgs e);
        void Content_DragOver(System.Windows.Forms.DragEventArgs e);
        void Content_GiveFeedback(System.Windows.Forms.GiveFeedbackEventArgs e);
        void Content_QueryContinueDrag(System.Windows.Forms.QueryContinueDragEventArgs e);
    }

    public interface IExplorerObjectContentDragDropEvents2 : IExplorerObjectContentDragDropEvents
    {
        void Content_DragDrop(System.Windows.Forms.DragEventArgs e, IUserData userdata);
        void Content_DragEnter(System.Windows.Forms.DragEventArgs e, IUserData userdata);
        void Content_DragLeave(System.EventArgs e, IUserData userdata);
        void Content_DragOver(System.Windows.Forms.DragEventArgs e, IUserData userdata);
        void Content_GiveFeedback(System.Windows.Forms.GiveFeedbackEventArgs e, IUserData userdata);
        void Content_QueryContinueDrag(System.Windows.Forms.QueryContinueDragEventArgs e, IUserData userdata);
    }

    public interface IExplorerTabPage : IOrder
    {
        System.Windows.Forms.Control Control { get; }

        //System.Guid ExplorerObjectGUID { get; }

        void OnCreate(object hook);

        Task<bool> OnShow();
        void OnHide();

        IExplorerObject ExplorerObject
        {
            get;
            set;
        }

        Task<bool> ShowWith(IExplorerObject exObject);
        string Title { get; }

        Task<bool> RefreshContents();
    }

    public interface IStatusBar
    {
        bool ProgressVisible { get; set; }
        int ProgressValue { get; set; }
        string Text { set; }
        global::System.Drawing.Image Image { set; }
        void Refresh();
    }
    public delegate void DockWindowAddedEvent(IDockableWindow window, string parentDockableWindowName);
    public delegate void OnShowDockableWindowEvent(IDockableWindow window);
    public delegate void AfterLoadMapDocumentEvent(IMapDocument mapDocument);

    public interface IDockableWindowContainer
    {
        string Name { get; }
    }

    public enum DockWindowState { right = 1, left = 2, top = 4, bottom = 8, none = 16, child = 32 }

    public interface IDockableWindow
    {
        bool Visible { get; set; }
        string Name { get; set; }
        DockWindowState DockableWindowState { get; set; }
        global::System.Drawing.Image Image { get; }
    }

    public interface IDockableToolWindow : IDockableWindow
    {

    }
}