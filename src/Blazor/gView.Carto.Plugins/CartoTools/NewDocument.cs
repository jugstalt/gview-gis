using gView.Carto.Core;
using gView.Carto.Core.Abstraction;
using gView.Carto.Core.Reflection;
using gView.Carto.Core.Services.Abstraction;
using gView.Carto.Plugins.Extensions;
using gView.Carto.Plugins.Services;
using gView.Framework.Blazor.Services.Abstraction;
using gView.Framework.Carto;
using gView.Framework.Carto.Abstraction;
using gView.Framework.Core.Common;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;

namespace gView.Carto.Plugins.CartoTools;

[RegisterPlugIn("D6A44B2E-1883-4D69-984B-2468E98216CD")]
[RestorePointAction(RestoreAction.SetRestorePointOnClick)]
public class NewDocument : ICartoInitialButton
{
    public string Name => "New Map";

    public string ToolTip => "Create a new empty map";

    public string Icon => "basic:bulb-shining";

    public CartoToolTarget Target => CartoToolTarget.File;

    public int SortOrder => 15;

    public void Dispose()
    {

    }

    public bool IsEnabled(ICartoApplicationScopeService scope)
    {
        return true;
    }

    async public Task<bool> OnClick(ICartoApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(typeof(gView.Carto.Razor.Components.Dialogs.NewMapDialog),
                                                    "Create New Map",
                                                    new Razor.Components.Dialogs.Models.NewMapModel());

        if (model != null)
        {
            var newDocument = new CartoDocument(scope, model.Name.Trim());
            newDocument.Map.Display.SpatialReference ??= scope.Document.Map.Display.SpatialReference.Clone() as ISpatialReference;

            ((CartoApplicationScopeService)scope).Document = newDocument;
        }

        return true;
    }
}
