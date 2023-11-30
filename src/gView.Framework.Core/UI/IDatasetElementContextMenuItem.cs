namespace gView.Framework.Core.UI
{
    /// <summary>
    /// Privides access to members that define a contextmenu item for the TOC.
    /// </summary>
    /// <remarks>
    /// A class that implements <c>IDatasetElementContexMenuItem</c> can be included to the TOC contextmenu. Everytime you right click a layer in the TOC the item will be selectable.
    /// </remarks>
    public interface IDatasetElementContextMenuItem : IContextMenuTool
    {
    }


}