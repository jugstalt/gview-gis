using gView.Framework.Core.Carto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Blazor.Core.Extensions;
static public class DrawPhaseExtensions
{
    static public IEnumerable<DrawPhase> Normalize(this DrawPhase drawPhase)
        => drawPhase == DrawPhase.All
            ? [DrawPhase.Geography, DrawPhase.Selection, DrawPhase.Highlighing, DrawPhase.Graphics]
            : [drawPhase];

    static public void WithNormalized(this DrawPhase drawPhase, Action<DrawPhase> action)
        => drawPhase.Normalize()
                    .ToList()
                    .ForEach(d => action(d));
}
