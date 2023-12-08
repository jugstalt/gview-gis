using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Carto.Razor.Components.Controls.Renderers.Models;
internal class RangeClass
{
    public RangeClass(double min, double max)
        => (Min, Max) = (min, max);

    public double Min { get; set; }
    public double Max { get; set; }

    public override string ToString()
        => String.Format("{0:0.00} - {1:0.00}", Min, Max);
}
