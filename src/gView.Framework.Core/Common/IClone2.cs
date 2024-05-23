/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.Common
{
    public interface IClone2
    {
        object Clone(CloneOptions options);
        void Release();
    }
}
