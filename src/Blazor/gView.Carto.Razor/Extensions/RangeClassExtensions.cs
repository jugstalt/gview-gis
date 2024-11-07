using gView.Carto.Razor.Components.Controls.Renderers.Models;

namespace gView.Carto.Razor.Extensions;
static internal class RangeClassExtensions
{
    static public bool Overlaps(this RangeClass r1, RangeClass r2)
        => r1.Min < r2.Max && r1.Max > r2.Min;

    static public bool Overlaps(this RangeClass r1, IEnumerable<RangeClass> rangeClasses)
        => rangeClasses.Any(r => r.Overlaps(r1));
}
