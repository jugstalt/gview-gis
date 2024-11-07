using gView.Carto.Core.Models.ToolEvents;
using gView.Carto.Core.Services.Abstraction;
using System;
using System.Threading.Tasks;

namespace gView.Carto.Core.Abstraction;

public interface ICartoInteractiveTool : ICartoInteractiveButton
{
    ToolType ToolType { get; }

    Type UIComponent { get; }

    object? ToolContext { get; }

    string ToolBoxTitle(ICartoApplicationScopeService scope);

    void InitializeScope(ICartoApplicationScopeService scope);

    Task<bool> OnEvent(ICartoApplicationScopeService scope, ToolEventArgs mapEvent);
}
