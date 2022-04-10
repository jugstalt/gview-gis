using gView.Framework.Data;
using gView.Framework.Data.Filters;
using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.Carto
{
    /// <summary>
    /// Porvide access to members and properties that control the functionality of renderers.
    /// </summary>
    public interface IFeatureRenderer : IRenderer, IPersistable, IClone, IClone2
    {
        /// <summary>
        /// Draws features from the specified Featurecursor on the given display.
        /// </summary>
        /// <param name="disp"></param>
        /// <param name="fCursor"></param>
        /// <param name="drawPhase"></param>
        /// <param name="cancelTracker"></param>
        //void Draw(IDisplay disp,IFeatureCursor fCursor,DrawPhase drawPhase,ICancelTracker cancelTracker);
        void Draw(IDisplay disp, IFeature feature);

        void StartDrawing(IDisplay display);

        void FinishDrawing(IDisplay disp, ICancelTracker cancelTracker);

        /// <summary>
        /// Prepares the query filter for the rendering process. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// This member is called by the framework befor querying the features.
        /// <param name="layer"></param>
        /// <param name="filter">The filter for querying the features</param>
        void PrepareQueryFilter(IFeatureLayer layer, IQueryFilter filter);
        /// <summary>
        /// Indicates if the specified feature class can be rendered on the given display. 
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        bool CanRender(IFeatureLayer layer, IMap map);

        bool HasEffect(IFeatureLayer layer, IMap map);

        bool UseReferenceScale { get; set; }

        /// <summary>
        /// The name of the renderer.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The category for the renderer.
        /// </summary>
        string Category { get; }

        bool RequireClone();
    }
}