namespace gView.Framework.Core.Carto
{
    public interface IScreen
    {
        void RefreshSettings(bool forceReloadAll = true);
        float LargeFontsFactor { get; }
    }
}