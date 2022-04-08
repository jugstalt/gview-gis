using System.Threading.Tasks;

namespace gView.Framework.UI
{
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


}