using gView.Carto.Core;
using gView.Carto.Core.Services.Abstraction;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;

namespace gView.Carto.Plugins.CartoTools;

internal class ZoomBack
{
    [RegisterPlugIn("BD25444D-BA0A-44D3-8B02-86EC2784ECDC")]
    internal class ZoomToLayer : ICartoButton
    {
        public string Name => "Zoom Back";

        public string ToolTip => "";

        public string Icon => "basic:round-left";

        public CartoToolTarget Target => CartoToolTarget.Map;

        public int SortOrder => 0;

        public bool IsVisible(ICartoApplicationScopeService scope) => true;

        public bool IsDisabled(ICartoApplicationScopeService scope) => !scope.ZoomHistory.HasItems;

        async public Task<bool> OnClick(ICartoApplicationScopeService scope)
        {
            var bounds = scope.ZoomHistory.Pop();

            if(bounds is not null)
            {
                scope.ZoomHistory.SuppressOnce = true;
                await scope.EventBus.FireMapZoomToAsync(bounds);
                return true;
            }

            return false;
        }
    }
}
