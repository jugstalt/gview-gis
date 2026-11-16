using gView.Framework.Core.Geometry;
using System.Collections.Generic;

namespace gView.Framework.Core.Carto
{
    public interface ISmartLabelPoint : IPoint
    {
        IMultiPoint AlernativeLabelPoints(IDisplay display);

        /// <summary>The same points in the same order, created lazily: the caller can stop at the first one that fits.</summary>
        IEnumerable<IPoint> AlternativeLabelPointsLazy(IDisplay display);
    }
}