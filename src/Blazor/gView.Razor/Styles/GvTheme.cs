using MudBlazor;

namespace gView.Razor.Styles;

/// <summary>
/// The one MudBlazor theme of the gView web apps (Carto, Explorer, Home, Login).
/// Colours are the tokens of the gView.Server v9 admin UI (see wwwroot/gview-theme.css).
/// </summary>
public static class GvTheme
{
    private static readonly string[] FontFamily = ["Segoe UI", "system-ui", "-apple-system", "Helvetica Neue", "Arial", "sans-serif"];

    public static MudTheme Theme { get; } = Create();

    private static MudTheme Create()
    {
        var theme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = "#2563eb",
                PrimaryContrastText = "#ffffff",
                Secondary = "#475569",
                SecondaryContrastText = "#ffffff",
                Tertiary = "#0f766e",
                Info = "#2563eb",
                Success = "#1f8a4c",
                Warning = "#b7791f",
                Error = "#d13b3b",
                Dark = "#101722",
                Background = "#f5f6f8",
                BackgroundGray = "#f0f2f5",
                Surface = "#ffffff",
                AppbarBackground = "#101722",
                AppbarText = "#b8c2d1",
                DrawerBackground = "#ffffff",
                DrawerText = "#18202b",
                TextPrimary = "#18202b",
                TextSecondary = "#677284",
                ActionDefault = "#677284",
                LinesDefault = "#e2e5ea",
                LinesInputs = "#c9ced6",
                Divider = "#e2e5ea",
                DividerLight = "#eef0f3",
                TableLines = "#e2e5ea",
                TableHover = "#f0f2f5",
                TableStriped = "#f7f8fa",
                OverlayDark = "rgba(8,11,16,0.45)"
            },
            PaletteDark = new PaletteDark()
            {
                Primary = "#5b93ff",
                PrimaryContrastText = "#0c1017",
                Secondary = "#8b97a8",
                SecondaryContrastText = "#0c1017",
                Tertiary = "#2dd4bf",
                Info = "#5b93ff",
                Success = "#4ccb7f",
                Warning = "#f0b542",
                Error = "#ff6b6b",
                Dark = "#080b10",
                Background = "#0c1017",
                BackgroundGray = "#1a212c",
                Surface = "#141a23",
                AppbarBackground = "#080b10",
                AppbarText = "#9aa6b8",
                DrawerBackground = "#141a23",
                DrawerText = "#e4e9f0",
                TextPrimary = "#e4e9f0",
                TextSecondary = "#8b97a8",
                TextDisabled = "rgba(228,233,240,0.38)",
                ActionDefault = "#8b97a8",
                ActionDisabled = "rgba(228,233,240,0.26)",
                ActionDisabledBackground = "rgba(228,233,240,0.12)",
                LinesDefault = "#273041",
                LinesInputs = "#3a4558",
                Divider = "#273041",
                DividerLight = "#1f2733",
                TableLines = "#273041",
                TableHover = "#1a212c",
                TableStriped = "#11161e",
                OverlayDark = "rgba(8,11,16,0.6)"
            },
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "6px",
                AppbarHeight = "40px"
            }
        };

        var typography = theme.Typography;
        foreach (var t in new BaseTypography[] {
            typography.Default, typography.H1, typography.H2, typography.H3, typography.H4, typography.H5, typography.H6,
            typography.Subtitle1, typography.Subtitle2, typography.Body1, typography.Body2,
            typography.Button, typography.Caption, typography.Overline })
        {
            t.FontFamily = FontFamily;
            t.LetterSpacing = "normal";
        }

        typography.Default.FontSize = typography.Body1.FontSize = ".875rem";
        typography.Body2.FontSize = ".8125rem";
        typography.Button.TextTransform = "none";
        typography.Button.FontWeight = "500";
        typography.H5.FontWeight = typography.H6.FontWeight = "600";
        typography.H6.FontSize = "1rem";

        return theme;
    }
}
