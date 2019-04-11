using System;
using System.Collections.Generic;
using gView.Framework.Data;
using gView.Framework.Carto;
using gView.Framework.system;
using gView.Framework.IO;
using System.Threading;
using System.Threading.Tasks;

namespace gView.Framework.UI
{
    public delegate void PropertyChangedEvent(object propertryObject);
    public interface IPropertyPageUI
    {
        event PropertyChangedEvent PropertyChanged;
    }

    public interface IPropertyPage
    {
        object PropertyPage(object initObject);
        object PropertyPageObject();
    }

    public interface ISelectionEnvironment
    {
        List<IDatasetElement> SelectableElements { get; }

        void AddToSelectableElements(IDatasetElement element);
        void RemoveFromSelectableElements(IDatasetElement element);
        void RemoveAll();
    }

    /// <summary>
    /// The type of a tool affect the mousereaction in the <c>MapViewControl</c>
    /// </summary>
    public enum ToolType { rubberband, pan, click, command, sketch, smartnavigation, userdefined }
    /// <summary>
    /// Privides access to members that define a tool.
    /// </summary>
    /// <remarks>
    /// Tools are the buttons in the commandbar of a Viewer. Use this interface when
    /// you develope your own tools.
    /// </remarks>

    public interface ITool
    {
        /// <summary>
        /// The Name of the tool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates if this command is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// The tooltip for this command.
        /// </summary>
        string ToolTip { get; }

        /// <summary>
        /// Indicates the mousereaction for the tool in the MapViewControl
        /// <see cref="gView.Framework.ToolType"/>
        /// </summary>
        ToolType toolType { get; }

        /// <summary>
        /// The image that is used as the icon on this command.
        /// </summary>
        object Image { get; }

        /// <summary>
        /// Called by the framework when the tool is created
        /// </summary>
        /// <param name="hook">An object that represents the hook.</param>
        /// <remarks>A hook can be a object thant implements <c>IMapDocument</c>, <c>IMap</c>, ... </remarks>
        void OnCreate(object hook);

        /// <summary>
        /// Called by the framework when the user make an interaction
        /// </summary>
        /// <param name="MapEvent"></param>
        /// <remarks>
        /// The time the framework calls the methode depands on the <c>toolType</c>.
        /// </remarks>
        Task<bool> OnEvent(object MapEvent);
    }

    public interface IExTool
    {
        /// <summary>
        /// The Name of the tool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates if this command is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// The tooltip for this command.
        /// </summary>
        string ToolTip { get; }

        /// <summary>
        /// The image that is used as the icon on this command.
        /// </summary>
        object Image { get; }

        /// <summary>
        /// Called by the framework when the tool is created
        /// </summary>
        /// <param name="hook">An object that represents the hook.</param>
        /// <remarks>A hook can be a object thant implements <c>IMapDocument</c>, <c>IMap</c>, ... </remarks>
        void OnCreate(object hook);

        /// <summary>
        /// Called by the framework when the user make an interaction
        /// </summary>
        /// <param name="MapEvent"></param>
        /// <remarks>
        /// The time the framework calls the methode depands on the <c>toolType</c>.
        /// </remarks>
        Task<bool> OnEvent(object MapEvent);
    }

    public interface IToolMenu : ITool
    {
        List<ITool> DropDownTools { get; }
        ITool SelectedTool { get; set; }
    }

    public interface IExToolMenu : IExTool
    {
        List<ITool> DropDownTools { get; }
        IExTool SelectedTool { get; set; }
    }

    public interface IToolButtonState
    {
        bool Checked { get; }
    }

    public interface IContextMenuItem : IOrder
    {
        bool ShowWith(object context);
    }

    public interface IContextType
    {
        string ContextName { get; }
        string ContextGroupName { get; }
        Type ContextType { get; }
        object ContextObject { get; }
    }

    public interface IToolControl
    {
        object Control { get; }
    }

    public interface IControl
    {
        void OnShowControl(object hook);
        void UnloadControl();
    }

    public interface IContextMenuTool : IOrder
    {
        /// <summary>
        /// The Name (Text) of the menu item.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Return true if the item in useable for a specific dataset element (ILayer, IFeatureLayer, IRasterLayer, ...)
        /// </summary>
        /// <param name="element">A class that impletemts IDatasetElement</param>
        /// <returns></returns>
        bool Enable(object element);
        bool Visible(object element);
        /// <summary>
        /// This member is called by the framework when the class is created. This is everytime when the user right click a layer in the TOC.
        /// </summary>
        /// <param name="hook">The <c>IMapDocument</c> class.</param>
        void OnCreate(object hook);
        /// <summary>
        /// This member is called by the framework when the menu item is seleced.
        /// </summary>
        /// <param name="dataset">The dataset</param>
        /// <param name="elelemt">The dataset element</param>
        Task<bool> OnEvent(object element, object parent);
        /// <summary>
        /// The image that is used as the icon on this command.
        /// </summary>
        object Image { get; }
    }

    public interface IScreenTip
    {
        string ScreenTip { get; }
    }

    /// <summary>
    /// Privides access to members that define a contextmenu item for the TOC.
    /// </summary>
    /// <remarks>
    /// A class that implements <c>IDatasetElementContexMenuItem</c> can be included to the TOC contextmenu. Everytime you right click a layer in the TOC the item will be selectable.
    /// </remarks>
    public interface IDatasetElementContextMenuItem : IContextMenuTool
    {
    }

    public interface IMapContextMenuItem : IContextMenuTool
    {
    }


    public class TocLegendItem : IDisposable
    {
        public string Label { get; set; }
        public System.Drawing.Bitmap Image { get; set; }

        public void Dispose()
        {
            if(Image != null)
            {
                Image.Dispose();
                Image = null;
            }
        }
    }
    public class TocLegendItems : IDisposable
    {
        public IEnumerable<TocLegendItem> Items { get; set; }

        public void Dispose()
        {
            if (Items != null)
            {
                foreach (var item in Items)
                    item.Dispose();
                Items = null;
            }
        }
    }

    public interface ITOC : IClone, IClone3
    {
        event EventHandler TocChanged;

        void Reset();

        ITOCElement NextVisibleElement { get; }

        List<ITOCElement> GroupedElements(ITOCElement group);

        /*
		void AddGroup(string GroupName,ITOCElement parent);
         * */
        void Add2Group(ITOCElement element, ITOCElement Group);

        ITOCElement GetTOCElement(string name, ITOCElement parent);
        ITOCElement GetTOCElement(ILayer element);
        ITOCElement GetTOCElementByLayerId(int layerId);
        //ITOCElement GetTOCElement(IClass element);

        List<ITOCElement> GroupElements { get; }

        void RenameElement(ITOCElement element, string newName);
        void MoveElement(ITOCElement element, ITOCElement refElement, bool insertAfter);

        void SplitMultiLayer(ITOCElement element);
        int CountGroupLayers(ITOCElement Group, bool subGroups);
        int CountVisibleGroupLayers(ITOCElement Group, bool subGroups);

        List<ILayer> Layers { get; }
        List<ILayer> VisibleLayers { get; }
        List<ITOCElement> Elements { get; }
        List<IWebServiceLayer> VisibleWebServiceLayers { get; }

        Task<global::System.Drawing.Bitmap> Legend();
        Task<global::System.Drawing.Bitmap> Legend(ITOCElement element);
        Task<global::System.Drawing.Bitmap> Legend(List<ITOCElement> elements);
        Task<TocLegendItems> LegendSymbol(ITOCElement element);
    }

    public enum TOCElementType { Layer, Legend, OpenedGroup, ClosedGroup }

    public interface ITOCElement
    {
        string Name { get; set; }
        TOCElementType ElementType { get; }
        List<ILayer> Layers { get; }
        void AddLayer(ILayer layer);
        void RemoveLayer(ILayer layer);
        ITOCElement ParentGroup { get; }
        bool LayerVisible { get; set; }
        bool LayerLocked { get; set; }
        bool LegendVisible { get; set; }
        void OpenCloseGroup(bool open);

        ITOC TOC { get; }
    }

    public delegate void MapAddedEvent(IMap map);
    public delegate void MapDeletedEvent(IMap map);
    public delegate void AfterSetFocusMapEvent(IMap map);

    public interface IMapDocument  // IMxDocument
    {
        event LayerAddedEvent LayerAdded;
        event LayerRemovedEvent LayerRemoved;
        event MapAddedEvent MapAdded;
        event MapDeletedEvent MapDeleted;
        event MapScaleChangedEvent MapScaleChanged;
        event AfterSetFocusMapEvent AfterSetFocusMap;

        IEnumerable<IMap> Maps { get; }
        IMap FocusMap { get; set; }

        bool AddMap(IMap map);
        bool RemoveMap(IMap map);

        IMap this[string mapName]
        {
            get;
        }

        IMap this[IDatasetElement layer]
        {
            get;
        }

        IApplication Application
        {
            get;
        }

        ITableRelations TableRelations { get; }
    }

    public interface IMapDocumentModules
    {
        IEnumerable<IMapApplicationModule> GetMapModules(IMap map);
    }

    public interface IContextTools
    {
        ITool[] ContextTools { get; }
    }

    public interface IApplication : ILicense
    {
        event EventHandler OnApplicationStart;

        string Title { get; set; }

        void Exit();
    }

    public delegate void ActiveMapToolChangedEvent(ITool OldTool, ITool NewTool);
    public delegate void OnCursorPosChangedEvent(double X, double Y);

    public class ProgressReport
    {
        public string Message = "";
        public int featureMax = 0;
        public int featurePos = 0;
    }

    public delegate void ProgressReporterEvent(ProgressReport progressEventReport);
    public interface IProgressReporterEvent
    {
        event ProgressReporterEvent ReportProgress;
    }

    public interface IProgressReporter : IProgressReporterEvent
    {
        ICancelTracker CancelTracker { get; }
    }


    public interface IOrder
    {
        int SortOrder { get; }
    }

    /*
    public interface IExplorerObjectContexMenu
    {
        void AppendContextMenuItems(System.Windows.Forms.ContextMenuStrip menu);
    }
    */
    public interface IExplorerCommand
    {
        global::System.Xml.XmlNodeList CommandDefs { get; }
        global::System.Guid ExplorerObjectGUID { get; }
    }

    public interface IModalDialog
    {
        bool OpenModal();
    }

    public interface IProgressDialog
    {
        string Text { get; set; }
        void ShowProgressDialog(IProgressReporter reporter, object argument, Thread thread);
        bool UserInteractive { get; }
    }


    public interface IConnectionStringDialog
    {
        string ShowConnectionStringDialog(string initConnectionString);
    }

    public class UIException : Exception
    {
        public UIException(string msg, Exception inner)
            : base(msg, inner)
        {
        }


        public override string Message
        {
            get
            {
                return base.Message + (base.InnerException != null ? "\n" + base.InnerException.Message : String.Empty);
            }
        }
    }

    
}