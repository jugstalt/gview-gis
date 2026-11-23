using gView.Framework.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Pipe.Abstraction;

namespace gView.Pipe.Plugins.Gis;

/// <summary>Opens the dataset of a gView Datasource credential and finds its feature classes.</summary>
static internal class GisDatasets
{
    static public async Task<IFeatureDataset> OpenAsync(GisDatasourceCredential credential)
    {
        var guid = credential.DatasetPluginGuid();
        var plugin = PlugInManager.Create(guid);
        if (plugin is null)
        {
            throw new PipeException($"gView has no dataset plugin {guid}. Is the plugin loaded from a gView folder that contains it?");
        }

        if (PlugInManager.HasUnsolvedDependencies(guid))
        {
            throw new PipeException($"The gView dataset plugin {plugin.GetType().Name} misses dependencies (e.g. native libraries) in this installation");
        }

        if (plugin is IFileFeatureDatabase fileDatabase)
        {
            if (!await fileDatabase.Open(credential.ConnectionString))
            {
                throw new PipeException($"Can't open the data source: {fileDatabase.LastErrorMessage}");
            }

            return await fileDatabase.GetDataset(credential.ConnectionString)
                ?? throw new PipeException("The data source has no dataset");
        }

        if (plugin is IFeatureDataset dataset)
        {
            await dataset.SetConnectionString(credential.ConnectionString);
            if (!await dataset.Open())
            {
                dataset.Dispose();
                throw new PipeException($"Can't open the data source: {dataset.LastErrorMessage}");
            }

            return dataset;
        }

        (plugin as IDisposable)?.Dispose();
        throw new PipeException($"The gView plugin {plugin.GetType().Name} is not a feature dataset");
    }

    static public async Task<IFeatureClass> FeatureClassAsync(IFeatureDataset dataset, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new PipeException("Enter the name of a feature class");
        }

        var element = await dataset.Element(name.Trim());
        if (element?.Class is IFeatureClass featureClass)
        {
            return featureClass;
        }

        var names = (await dataset.Elements())
            .Where(e => e.Class is IFeatureClass)
            .Select(e => e.Title)
            .Order(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();
        throw new PipeException(element is null
            ? $"The data source has no feature class '{name}'" + (names.Count > 0 ? $"; it has e.g. {string.Join(", ", names)}" : "")
            : $"'{name}' is not a feature class");
    }

    static public IFeatureUpdater Updater(IFeatureClass featureClass)
        => featureClass.Dataset?.Database as IFeatureUpdater
           ?? throw new PipeException($"The data source of '{featureClass.Name}' can't be written to");
}
