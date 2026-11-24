using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.Symbology;
using gView.Framework.Symbology.Models;
using gView.GraphicsEngine;

namespace gView.Carto.Razor.Components.Dialogs.Models;

public class SetSymbolPropertiesModel : IDialogResultItem
{
    // info (set by caller)
    public GeometryType GeometryType { get; set; }
    public int ClassCount { get; set; }
    public int SkippedCount { get; set; }
    public bool HasAllOtherValues { get; set; }
    /// <summary>Number of selected classes the dialog applies to (0 = no selection, all classes).</summary>
    public int SelectedCount { get; set; }

    public bool IncludeAllOtherValues { get; set; }

    public bool ApplyColor { get; set; }
    public ArgbColor Color { get; set; } = ArgbColor.Black;

    public bool ApplyWidth { get; set; }
    public float Width { get; set; } = 1f;

    public bool ApplyDashStyle { get; set; }
    public LineDashStyle DashStyle { get; set; } = LineDashStyle.Solid;

    public bool ApplyOpacity { get; set; }
    public float Opacity { get; set; } = 100f;

    public bool ApplySize { get; set; }
    public float Size { get; set; } = 5f;

    public bool ApplySmoothing { get; set; }
    public SymbolSmoothing Smoothing { get; set; } = SymbolSmoothing.AntiAlias;

    public bool IsPolygon => GeometryType is GeometryType.Polygon or GeometryType.Envelope;
    public bool IsLine => GeometryType == GeometryType.Polyline;
    public bool IsPoint => GeometryType is GeometryType.Point or GeometryType.Multipoint;

    public void SetStartValues(SymbolPropertyValues values)
    {
        Color = values.Color ?? Color;
        Width = values.Width ?? Width;
        DashStyle = values.DashStyle ?? DashStyle;
        Opacity = values.Opacity ?? Opacity;
        Size = values.Size ?? Size;
        Smoothing = values.Smoothing ?? Smoothing;
    }

    public SymbolPropertyValues ToValues() => new()
    {
        Color = ApplyColor ? Color : null,
        Width = ApplyWidth && (IsLine || IsPolygon) ? Width : null,
        DashStyle = ApplyDashStyle && (IsLine || IsPolygon) ? DashStyle : null,
        Opacity = ApplyOpacity && IsPolygon ? Opacity : null,
        Size = ApplySize && IsPoint ? Size : null,
        Smoothing = ApplySmoothing ? Smoothing : null
    };
}
