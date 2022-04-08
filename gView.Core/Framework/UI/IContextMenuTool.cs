using System.Threading.Tasks;

namespace gView.Framework.UI
{
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

    
}