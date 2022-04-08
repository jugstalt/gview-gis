using System.Drawing;

namespace gView.Framework.system.UI
{
    public interface IUIImageList
    {
        Image this[int index] { get; }
    }
}
