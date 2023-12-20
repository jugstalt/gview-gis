using gView.Framework.Core.UI;

namespace gView.Blazor.Models.Tree;

public class TocElementWrapper(ITocElement tocElement)
{
    public ITocElement TocElement { get; } = tocElement;

    public bool Selected { get; set; } = false;
    public string Name { get; } = tocElement.Name;
}
