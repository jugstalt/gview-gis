using gView.Carto.Core.Abstraction;
using System.Collections.Generic;

namespace gView.Carto.Core.Services.Abstraction;

public interface ICartoInteractiveToolService
{
    public ICartoInteractiveTool? CurrentTool { get; set; }

    public T? GetCurrentToolContext<T>() where T : class;

    public IEnumerable<ICartoInteractiveTool> ScopedTools { get; }
}
