namespace gView.Framework.Core.UI
{
    /*
    public interface IExplorerObjectContexMenu
    {
        void AppendContextMenuItems(System.Windows.Forms.ContextMenuStrip menu);
    }
    */
    public interface IExplorerCommand
    {
        System.Xml.XmlNodeList CommandDefs { get; }
        System.Guid ExplorerObjectGUID { get; }
    }


}