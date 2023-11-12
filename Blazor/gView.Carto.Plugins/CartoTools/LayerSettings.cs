using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.Carto.Plugins.CartoTools
{
    [RegisterPlugIn("EB3EDA0D-1B27-4314-B1DE-915E27B27982")]
    internal class LayerSettings : ICartoTool
    {
        public string Name => "Layer Settings";

        public string ToolTip => "";

        public ToolType ToolType => ToolType.Click;

        public string Icon => "basic:settings";

        public CartoToolTarget Target => CartoToolTarget.SelectedTocItem;

        public int SortOrder => 99;

        public void Dispose()
        {
            
        }

        public bool IsEnabled(IApplicationScope scope)
            =>  scope.ToCartoScopeService().SelectedTocTreeNode != null;
        
        public Task<bool> OnEvent(IApplicationScope scope)
        {
           return Task.FromResult(false);
        }
    }
}
