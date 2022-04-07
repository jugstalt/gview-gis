namespace gView.Framework.Carto
{
    public interface IScreen
    {
        void RefreshSettings(bool forceReloadAll = true);
        float LargeFontsFactor { get; }
    }
}