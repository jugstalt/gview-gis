using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Services.Abstraction;
using System.Threading.Tasks;

namespace gView.Framework.Carto.Abstraction;

public interface ICartoButton : Core.UI.IOrder, ICartoInteractiveButton
{
    bool IsVisible(ICartoApplicationScopeService scope);

    bool IsDisabled(ICartoApplicationScopeService scope);

    string ToolTip { get; }

    string Icon { get; }

    CartoToolTarget Target { get; }

    Task<bool> OnClick(ICartoApplicationScopeService scope);
}
