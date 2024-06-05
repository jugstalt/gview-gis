using gView.Cmd.Core.Abstraction;
using gView.Cmd.LuceneServer.Lib;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("FC112A48-B1ED-4E38-81E6-EE49A51D02D1")]
internal class LuceneServerFill : IExplorerToolCommand
{
    public string Name => "LuceneServer.Fill";

    public string ToolTip => "Lucene (Search) Server - fill index, ...";

    public string Icon => "";

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                                typeof(Razor.Components.Dialogs.LuceneServerFillToolDialog),
                                "LuceneServer Fill Tool",
                                new LuceneServerFillToolModel());

        if (model is null) return false;

        if (string.IsNullOrEmpty(model.JsonFile))
        {
            throw new System.Exception("No json file defined");
        }
        if (model.PackageSize <= 0)
        {
            throw new System.Exception($"Invalid package size: {model.PackageSize}");
        }

        ICommand command = new FillCommand();
        IDictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "json", model.JsonFile },
            { "package-size", model.PackageSize }
        };

        if (!string.IsNullOrEmpty(model.BasicAuthenticationUser)
            && !string.IsNullOrEmpty(model.BasicAuthenticationPassword))
        {
            parameters.Add("basic-auth-user", model.BasicAuthenticationUser);
            parameters.Add("basic-auth-pwd", model.BasicAuthenticationPassword);
        }

        if (!string.IsNullOrEmpty(model.ProxyUrl))
        {
            parameters.Add("proxy-url", model.ProxyUrl);
            parameters.Add("proxy-user", model.ProxyUsername);
            parameters.Add("proxy-pwd", model.ProxyPassword);
        }

        await scope.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    $"Fill LuceneServer Index",
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
}
