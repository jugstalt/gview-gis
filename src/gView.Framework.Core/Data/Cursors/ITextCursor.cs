namespace gView.Framework.Core.Data.Cursors
{
    public interface ITextCursor : ICursor
    {
        string Text { get; }
    }



    /*
    public interface IFeatureBuffer 
    {
        IFeature CreateFeature();
        bool Store();
    }
    */
}
