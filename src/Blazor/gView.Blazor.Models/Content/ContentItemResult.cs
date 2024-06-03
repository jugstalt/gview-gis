using gView.Framework.DataExplorer.Abstraction;

namespace gView.Blazor.Models.Content;
public class ContentItemResult : IContentItemResult
{
    public IContentItem? Item { get; set; }

    #region Static Members

    private static ContentItemResult _empty = new ContentItemResult();
    public static ContentItemResult Empty => _empty;

    #endregion
}
