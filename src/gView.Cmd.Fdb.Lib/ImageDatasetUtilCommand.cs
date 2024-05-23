using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.Cmd.Core.Extensions;
using gView.Cmd.Fdb.Lib.Model;
using gView.Cmd.Fdb.Lib.Raster;
using gView.Framework.Core.Data;
using gView.Framework.Core.Common;
using gView.Framework.Common;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gView.DataSources.Fdb.MSAccess;

namespace gView.Cmd.Fdb.Lib;
public class ImageDatasetUtilCommand : ICommand
{
    public string Name => "FDB.ImageDatasetUtil";

    public string Description => "Working wigh FDB Image Datasets";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions => new ICommandParameterDescription[]
    {
        new RequiredCommandParameter<IRasterDataset>("")
        {
            Description="FDB ImageDataset"
        },
        new RequiredCommandParameter<string>("job")
        {
            Description= """
                The image dataset job: [add|clean|truncate]

                   add:      add file or directory with filter
                   clean:    remove unexisting images
                   check:    check for not existing images
                   truncate: remove all items from dataset
                """
        },
        new CommandParameter<string>("filename")
        {
            Description = "   When 'add': filename of file to add"
        },
        new CommandParameter<string>("directory")
        {
            Description = "               or directory"
        },
        new CommandParameter<string>("filter")
        {
            Description = "                  wiht filter (*.jpg, ...)"
        },
        new CommandParameter<string>("provider")
        {
            Description = "               <first|gdal|raster>"
        }
    };

    public async Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        try
        {
            #region Dataset 

            var datasetBuilder = new FdbDatasetParameterBuilder("");
            var dataset = await datasetBuilder.Build<IFeatureDataset>(parameters);

            #endregion

            var job = parameters.GetRequiredValue<string>("job");

            switch (job.ToLower())
            {
                case "trucate":
                    await new Truncate().Run(dataset, $"{dataset.DatasetName}_IMAGE_POLYGONS");
                    break;
                case "clean":
                    var removeIfNotExists = new RemoveIfNotExists(cancelTracker);
                    removeIfNotExists.ReportAction += (sender, message) =>
                    {
                        if (message.StartsWith("."))
                        {
                            logger?.Log(message);
                        }
                        else
                        {
                            logger?.LogLine("");
                            logger?.LogLine(message);
                        }
                    };

                    await removeIfNotExists.Run(dataset);
                    break;
                case "check":
                    var checkIfExists = new CheckIfExists(cancelTracker);
                    checkIfExists.ReportAction += (sender, message) =>
                    {
                        if (message.StartsWith("."))
                        {
                            logger?.Log(message);
                        }
                        else
                        {
                            logger?.LogLine("");
                            logger?.LogLine(message);
                        }
                    };

                    await checkIfExists.Run(dataset);
                    break;
                case "add":
                    string? filename = parameters.GetValue<string>("filename");

                    var add = new Add(cancelTracker);
                    add.ReportAction += (sender, message) =>
                    {
                        if (message.StartsWith(".."))
                        {
                            logger?.Log(message);
                        }
                        else
                        {
                            logger?.LogLine("");
                            logger?.LogLine(message);
                        }
                    };

                    if (!String.IsNullOrEmpty(filename))
                    {
                        await add.RunAddFiles(
                                dataset, 
                                new[] { filename! }, 
                                GetProviders(parameters, logger)
                            );
                        await CalculateExtent(dataset);

                        break;
                    }

                    string? directory = parameters.GetValue<string>("directory");

                    if (!String.IsNullOrEmpty(directory))
                    {
                        string filters = parameters.GetRequiredValue<string>("filter");

                        await add.RunImportDirectory(
                                dataset, 
                                new System.IO.DirectoryInfo(directory), 
                                filters.Split(';'), 
                                GetProviders(parameters, logger)
                            );
                        await CalculateExtent(dataset);

                        break;
                    }

                    throw new Exception("Specify filename or directory+filter to add files to image dataset");
                default:
                    throw new Exception($"Unknown Job: {job}");
            }

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogLine($"Error: {ex.Message}");
            return false;
        }
    }

    #region Helper

    private Dictionary<string, Guid>? GetProviders(IDictionary<string, object> parameters, ICommandLogger? logger)
    {
        string? providerType = parameters.GetValue<string>("provider")?.Trim();

        if (String.IsNullOrEmpty(providerType) || providerType == "first")
        {
            providerType = "gdal";  // default
        }

        Dictionary<string, Guid>? providers = new Dictionary<string, Guid>();

        if (providerType!.StartsWith("[") && providerType.EndsWith("]"))
        {
            var providerModels = JsonSerializer.Deserialize<RasterProviderModel[]>(providerType);

            foreach (var providerModel in providerModels!)
            {
                var rds = PlugInManager.Create(providerModel.PluginGuid) as IRasterFileDataset;

                if (rds != null)
                {
                    string extension = providerModel.Format;

                    int pos = extension.LastIndexOf(".");
                    if (pos > 0)
                    {
                        extension = extension.Substring(pos);
                    }

                    providers.Add(extension, PlugInManager.PlugInID(rds));
                    logger?.LogLine($"Provider {extension}: {rds.ToString()} - {PlugInManager.PlugInID(rds).ToString()}");
                }
            }
        }
        else
        {

            IRasterFileDataset? rds = null;
            if (providerType == "gdal")
            {
                rds = PlugInManager.Create(new Guid("43DFABF1-3D19-438c-84DA-F8BA0B266592")) as IRasterFileDataset;
            }
            else if (providerType == "raster")
            {
                rds = PlugInManager.Create(new Guid("D4812641-3F53-48eb-A66C-FC0203980C79")) as IRasterFileDataset;
            }


            if (rds != null)
            {
                foreach (string format in rds.SupportedFileFilter.Split('|'))
                {
                    string extension = format;

                    int pos = format.LastIndexOf(".");
                    if (pos > 0)
                    {
                        extension = format.Substring(pos);
                    }

                    providers.Add(extension, PlugInManager.PlugInID(rds));
                    logger?.LogLine($"Provider {extension}: {rds.ToString()} - {PlugInManager.PlugInID(rds).ToString()}");
                }
            }
        }

        if (providers.Count == 0)
        {
            providers = null;
        }

        return providers;
    }


    async static Task<bool> CalculateExtent(IFeatureDataset? ds)
    {
        //Console.WriteLine("\nCalculete new Extent");
        var fdb = ds?.Database as AccessFDB;
        if (fdb is not null)
        {
            IFeatureClass fc = await fdb.GetFeatureclass(ds!.DatasetName, ds.DatasetName + "_IMAGE_POLYGONS");
            await fdb.CalculateExtent(fc);
            //fdb.ShrinkSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
            //fdb.RebuildSpatialIndex(ds.DatasetName + "_IMAGE_POLYGONS");
        }
        return true;
    }

    #endregion
}
