using gView.Carto.Core.Abstraction;
using System.Collections.Generic;

namespace gView.Carto.Core.Services.Abstraction;

public interface ICartoInteractiveToolService
{
    public ICartoInteractiveTool? CurrentTool { get; set; }

    public IEnumerable<ICartoInteractiveTool> ScopedTools { get; }
}
