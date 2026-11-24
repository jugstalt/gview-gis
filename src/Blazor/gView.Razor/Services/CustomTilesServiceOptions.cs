namespace gView.Razor.Services;

/// <summary>
/// Start page settings, bound from the root of the web apps config:
/// "CustomTiles": [ ... ] and "Home": { ... }
/// </summary>
public class CustomTilesServiceOptions
{
    public IEnumerable<CustomTileModel>? CustomTiles { get; set; }

    public HomeModel? Home { get; set; }

    /// <summary>
    /// Set by the host when a login (OIDC or forms) is configured. Without it everyone is administrator
    /// and the start page shows a warning to administrators. Not read from the config.
    /// </summary>
    public bool AuthenticationConfigured { get; set; }

    #region Classes

    public class CustomTileModel
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string TargetUrl { get; set; } = "";

        public string FontColor { get; set; } = "";
        public string BackgroundColor { get; set; } = "";

        /// <summary>
        /// Who sees the tile: "admin" (default), "user" (signed in users with a gView role) or "public" (everyone)
        /// </summary>
        public string Visibility { get; set; } = "admin";
    }

    public class HomeModel
    {
        public string Title { get; set; } = "";
        public string WelcomeText { get; set; } = "";
        public string LogoUrl { get; set; } = "";

        /// <summary>
        /// Shown to signed in users without a gView role, e.g. who to ask for access
        /// </summary>
        public string NoAccessText { get; set; } = "";

        /// <summary>
        /// Footer links; replace the default links (documentation, GitHub) when set
        /// </summary>
        public IEnumerable<LinkModel>? Links { get; set; }
    }

    public class LinkModel
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
    }

    #endregion
}
