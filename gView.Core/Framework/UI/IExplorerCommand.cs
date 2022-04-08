namespace gView.Framework.UI
{
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

    
}