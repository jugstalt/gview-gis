using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Core.UI;
using System.Threading.Tasks;

namespace gView.Framework.Shared.Abstraction;

public interface ICartoTool : IOrder
{
    string Name { get; }

    bool IsEnabled(ICartoApplicationScopeService scope);

    string ToolTip { get; }

    ToolType ToolType { get; }

    string Icon { get; }

    CartoToolTarget Target { get; }

    Task<bool> OnEvent(ICartoApplicationScopeService scope);
}
