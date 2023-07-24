using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.Cmd.Core.Extensions;
using gView.Cmd.Fdb.Lib.Raster;
using gView.Framework.Data;
using gView.Framework.Network;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.Fdb.Lib;
internal class ImageDatasetUtilCommand : ICommand
{
    public string Name => "FDB.ImageDatasetUtil";

    public string Description => "Working wigh FDB Image Datasets";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions => new ICommandParameterDescription[]
    {
        new RequiredCommandParameter<IRasterDataset>("")
        {
            Description="FDB ImageDadtaset"
        },
        new RequiredCommandParameter<string>("job")
        {
            Description= """
                The image dataset job: [add|clean|truncate]

                   add:      add file or directory with filter
                   clean:    remove unexisting images
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
                    await new RemoveIfNotExists(cancelTracker).Run(dataset);
                    break;
                case "add":
                    string? filename = parameters.GetValue<string>("filename");

                    if (!String.IsNullOrEmpty(filename))
                    {
                        await new Add(cancelTracker).RunAddFiles(dataset, new[] { filename! }, GetProviders(parameters));
                        break;
                    }

                    string? directory = parameters.GetValue<string>("directory");
                    
                    if (!String.IsNullOrEmpty(directory))
                    {
                        string filters = parameters.GetRequiredValue<string>("filter");

                        await new Add(cancelTracker).RunImportDirectory(dataset, new System.IO.DirectoryInfo(directory), filters.Split(';'), GetProviders(parameters));
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

    private Dictionary<string, Guid>? GetProviders(IDictionary<string, object> parameters)
    {
        string? provider = parameters.GetValue<string>("provider");

        IRasterFileDataset? rds = null;
        if (provider == "gdal")
        {
            rds = PlugInManager.Create(new Guid("43DFABF1-3D19-438c-84DA-F8BA0B266592")) as IRasterFileDataset;
        }
        else if (provider == "raster")
        {
            rds = PlugInManager.Create(new Guid("D4812641-3F53-48eb-A66C-FC0203980C79")) as IRasterFileDataset;
        }

        Dictionary<string, Guid>? providers = new Dictionary<string, Guid>();
        if (rds != null)
        {
            foreach (string format in rds.SupportedFileFilter.Split('|'))
            {
                string extension = format;

                int pos = format.LastIndexOf(".");
                if (pos > 0)
                {
                    extension = format.Substring(pos, format.Length - pos);
                }

                providers.Add(extension, PlugInManager.PlugInID(rds));
                Console.WriteLine("Provider " + extension + ": " + rds.ToString() + " {" + PlugInManager.PlugInID(rds).ToString() + "}");
            }
        }

        if (providers.Count == 0)
        {
            providers = null;
        }

        return providers;
    }

    #endregion
}
