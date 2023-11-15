using gView.Carto.Plugins.Extensions;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.OGC.WFS.Version_1_1_0;
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
        
        async public Task<bool> OnEvent(IApplicationScope scope)
        {
            var scopeService = scope.ToCartoScopeService();
            var original = scopeService.Document.Map as Map;
            var clone = original?.Clone() as Map;

            if (original is null || clone is null)
            {
                return false;
            }

            clone.ZoomTo(original.Display.Envelope);
            clone.Display.ImageWidth = original.Display.ImageWidth;
            clone.Display.ImageHeight = original.Display.ImageHeight;

            var layer = scopeService.SelectedTocTreeNode?.TocElement?.Layers?.FirstOrDefault();
            if (layer is null)  // todo: clone layer?
            {
                return false;
            }

            var model = await scopeService.ShowModalDialog(typeof(gView.Carto.Razor.Components.Dialogs.LayerSettingsDialog),
                                                                "Map Properties",
                                                                new Razor.Components.Dialogs.Models.LayerSettingsModel()
                                                                {
                                                                    Map = clone,
                                                                    Layer = layer
                                                                });

            if (model is null)
            {
                return false;
            }

            return true;
        }
    }
}
