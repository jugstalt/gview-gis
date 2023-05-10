namespace gView.Blazor.Models.Tree;

public class TreeItem
{
    public string Text { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsExpanded { get; set; }
    public bool HasChildren { get; set; }
}
