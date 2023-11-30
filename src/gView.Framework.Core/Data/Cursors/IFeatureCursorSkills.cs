namespace gView.Framework.Core.Data.Cursors
{
    public interface IFeatureCursorSkills : IFeatureCursor
    {
        bool KnowsFunctions { get; }
    }



    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
