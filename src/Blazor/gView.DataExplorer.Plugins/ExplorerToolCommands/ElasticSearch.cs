using gView.Cmd.Core.Abstraction;
using gView.Cmd.ElasticSearch.Lib;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("3A8FB727-679A-4B91-81FB-1C8F8EB8665E")]
internal class ElasticSearch : IExplorerToolCommand
{
    public string Name => "ElasticSearch";

    public string ToolTip => "ElasticSearch - fill index, ...";

    public string Icon => "";

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                                typeof(Razor.Components.Dialogs.ElasticSearchToolDialog),
                                "ElasticSearch Tools",
                                new ElasticSearchToolModel());

        if (model is null) return false;

        if (string.IsNullOrEmpty(model.JsonFile))
        {
            throw new System.Exception("No json file defined");
        }

        ICommand command = new FillCommand();
        IDictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "json", model.JsonFile }
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
                    $"Fill ElasticSearch Index",
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
