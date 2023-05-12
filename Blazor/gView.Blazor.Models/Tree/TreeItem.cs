using System.Collections.Generic;

namespace gView.Blazor.Models.Tree;

public class TreeItem<T>
    where T : TreeItem<T>
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsExpanded { get; set; }
    public bool HasChildren { get; set; }
    public HashSet<T>? Children { get; set; }
        
}
