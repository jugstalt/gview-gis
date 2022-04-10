using System.Threading.Tasks;

namespace gView.Framework.Data.Cursors
{
    /// <summary>
    /// Provide access to members an properties that hand out enumerated features.
    /// </summary>
    public interface IFeatureCursor : ICursor
    {
        // <summary>
        /// Advance the position of the cursor by one and return the Feature object at that position.
        /// </summary>
        /// /// <remarks>
        /// Return <c>null</c> after the last row is reached.
        /// </remarks>
        Task<IFeature> NextFeature();
    }

    

    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
