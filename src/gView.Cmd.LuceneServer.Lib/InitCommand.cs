using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.Cmd.Core.Extensions;
using gView.Cmd.LuceneServer.Lib.Models;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using System.Text.Json;

namespace gView.Cmd.LuceneServer.Lib;
internal class InitCommand : ICommand
{
    public string Name => "LuceneServer.Init";

    public string Description => "Creates a json file template";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
        [
            new RequiredCommandParameter<string>("output")
            {
                Description = "Json file to create"
            },
            new CommandParameter<string>("url") 
            {
                Description="Optional: Url of the lucene server"
            },
            new CommandParameter<string>("index")
            {
                Description="Optional: Name of the index"
            },
            new CommandParameter<bool>("delete_index") 
            {
                Description="Optional: Delete index on refill (default: true)"
            },
            new CommandParameter<IFeatureClass>("source")
            {
                Description="Optional: FeatureClass to import..."
            },
        ];

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        var outputFile = parameters.GetRequiredValue<string>("output");
        var fcBuilder = new FeatureClassParameterBuilder("fc");

        var dataset = await fcBuilder.TryBuildAsync<IFeatureDataset>(parameters);
        var fc = dataset is null
            ? null 
            : await fcBuilder.BuildFeatureClass(dataset, parameters);

        var model = new ImportConfig()
        {
            Connection = new ImportConfig.LuceneServerConnectionConnection()
            {
                Url = parameters.GetValueOrDefault<string>("url", "")!,
                DefaultIndex = parameters.GetValueOrDefault<string>("index", "")!,
                DeleteIndex = parameters.GetValueOrDefault<bool>("delete_index", true)
            },
            Datasets =
            [
                new ImportConfig.DatasetConnection()
                {
                    FeatureClasses =
                    [
                        new ImportConfig.FeatureClassDefinition()
                    ]
                }
            ]
        };

        var json = JsonSerializer.Serialize(model,
            new JsonSerializerOptions()
            {
                WriteIndented = true,
            });

        await File.WriteAllTextAsync(outputFile, json);

        return true;
    }
}
