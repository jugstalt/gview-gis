using gView.Cmd.Core.Abstraction;
using gView.Cmd.LuceneServer.Lib;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("4D4C2699-43EB-4727-ADFB-20E508B794E8")]
internal class LuceneServerInit : IExplorerToolCommand
{
    public string Name => "LuceneServer.Init";

    public string ToolTip => "Initialize a LuceneServer.Fill Json config file";

    public string Icon => "";

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                                typeof(Razor.Components.Dialogs.LuceneServerInitToolDialog),
                                "LuceneServer Init Tool",
                                new LuceneServerInitToolModel());

        if (model is null)
        {
            return false;
        }

        ICommand command = new InitCommand();
        IDictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "output", model.JsonFile },
            { "url", model.LuceneServerUrl },
            { "index", model.IndexName },
            { "delete_index", model.DeleteIndex },
            { "phon_alg", model.PhoneticAlgorithm }
        };

        if (model.SourceFeatureClasses?.Any() == true)
        {
            var dataset = model.SourceFeatureClasses.First().Dataset;

            if(dataset is null)
            {
                throw new Exception("FeatureClass has no dataset!");
            }

            parameters.Add("source_connstr", dataset.ConnectionString);
            parameters.Add("source_guid", PlugInManager.PlugInID(dataset));
            parameters.Add("source_fc", string.Join(",", model.SourceFeatureClasses.Select(fc => fc.Name)));
        }

        await scope.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    $"Init LuceneServer Json",
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
