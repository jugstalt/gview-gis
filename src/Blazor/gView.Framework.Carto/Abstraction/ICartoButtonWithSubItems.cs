using gView.Carto.Core.Services.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Carto.Abstraction;

public record CartoButtonSubItem(int Id, string Title, string Icon)
{

}

public interface ICartoButtonWithSubItems : ICartoButton
{
    IEnumerable<CartoButtonSubItem> SubItems(ICartoApplicationScopeService scope);

    Task<bool> OnClick(ICartoApplicationScopeService scope, int subItemId);
}