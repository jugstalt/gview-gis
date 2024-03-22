using gView.Cmd.Core.Abstraction;
using gView.Cmd.MxlUtil.Lib;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("B7400D7B-4CB2-405F-8B7F-A8919DE7F912")]
internal class MxlFromVectorTilesGLStyles : IExplorerToolCommand
{
    public string Name => "VectorTileCache.ImportGLStyles";

    public string ToolTip => "Import GLStyles to a gView Map MXL File";

    public string Icon => string.Empty;

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                                typeof(Razor.Components.Dialogs.ImportVectorTileGLStylesDialog),
                                "Create MXL from GL Styles",
                                new ImportVectorTileGLStylesModel());

        if (!string.IsNullOrEmpty(model?.GLStylesJsonUrl)
            && !string.IsNullOrWhiteSpace(model?.TargetFolder)
            && !string.IsNullOrWhiteSpace(model?.MapName))
        {
            ICommand command = new FromGLStylesJsonCommand();
            IDictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "uri", model.GLStylesJsonUrl },
                { "target-path", model.TargetFolder },
                { "map-name", model.MapName }
            };

            await scope.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    $"Render Service Tiles",
                    new ExecuteCommandModel()
                    {
                        CommandItems = new[]
                        {
                            new CommandItem()
                            {
                                Command = command,
                                Parameters = parameters
                            }
                        }
                    });

            return true;
        }

        return false;
    }
}
