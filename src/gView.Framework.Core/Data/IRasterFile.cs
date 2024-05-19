namespace gView.Framework.Core.Data
{
    public interface IRasterFile
    {
        string Filename { get; }
        IRasterWorldFile WorldFile { get; }
    }
}