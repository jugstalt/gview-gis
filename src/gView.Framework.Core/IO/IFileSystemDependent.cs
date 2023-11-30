namespace gView.Framework.Core.IO
{
    public interface IFileSystemDependent
    {
        bool FileChanged(string filename);
        bool FileDeleted(string filename);
    }
}
