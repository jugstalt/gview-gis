using gView.Carto.Core.Models.MapEvents;
using gView.Carto.Core.Services.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.Carto.Core.Abstraction;

public interface ICartoInteractiveTool : ICartoInteractiveButton
{
    ToolType ToolType { get; }

    Type UIComponent { get; }

    object? ToolContext { get; }

    void InitializeScope(ICartoApplicationScopeService scope);

    Task<bool> OnEvent(ICartoApplicationScopeService scope, MapEvent mapEvent);
}
