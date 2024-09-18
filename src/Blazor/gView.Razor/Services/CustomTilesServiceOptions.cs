namespace gView.Razor.Services;

public class CustomTilesServiceOptions
{
    public IEnumerable<CustomTileModel>? CustomTiles { get; set; }

    #region Classes

    public class CustomTileModel
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string TargetUrl { get; set; } = "";

        public string FontColor { get; set; } = "";
        public string BackgroundColor { get; set; } = "";

    }

    #endregion
}
