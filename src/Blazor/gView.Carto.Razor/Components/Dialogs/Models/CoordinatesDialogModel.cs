using gView.Blazor.Models.Dialogs;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;

namespace gView.Carto.Razor.Components.Dialogs.Models;
public class CoordinatesDialogModel : IDialogResultItem
{
    public IPoint Coordinate { get; set; } = new Point();
    public double MapScaleDominator { get;set; }
    public ISpatialReference? SpatialReference { get; set; }
}
