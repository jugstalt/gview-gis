using gView.Cmd.Core.Abstraction;
using gView.Cmd.MxlUtil.Lib;
using gView.DataExplorer.Razor.Components.Dialogs.Models;
using gView.Framework.Blazor;
using gView.Framework.Core.Common;
using gView.Framework.DataExplorer.Abstraction;
using gView.Framework.DataExplorer.Services.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.DataExplorer.Plugins.ExplorerToolCommands;

[RegisterPlugIn("E04607D9-F286-4D4E-8F0F-55EAB3552263")]
internal class MxlUtilTool : IExplorerToolCommand
{
    public string Name => "MxlUtil";

    public string ToolTip => "Run a mxl utility (manage datasets, copy to fdb, publish service, convert aprx)";

    public string Icon => String.Empty;

    async public Task<bool> OnEvent(IExplorerApplicationScopeService scope)
    {
        var model = await scope.ShowModalDialog(
                                typeof(Razor.Components.Dialogs.MxlUtilToolDialog),
                                "MXL Utilities",
                                new MxlUtilToolModel());

        if (model is null || String.IsNullOrEmpty(model.Utility))
        {
            return false;
        }

        IDictionary<string, object> parameters = new Dictionary<string, object>()
        {
            { "u", model.Utility }
        };

        switch (model.Utility)
        {
            case "MxlDatasets":
                if (String.IsNullOrWhiteSpace(model.MxlDatasets_Mxl))
                {
                    throw new Exception("MxlDatasets: 'Mxl file' is required");
                }

                parameters.Add("mxl", model.MxlDatasets_Mxl);
                parameters.Add("cmd", model.MxlDatasets_Command);

                if (model.MxlDatasets_Command == "modify-cs")
                {
                    if (String.IsNullOrWhiteSpace(model.MxlDatasets_OutMxl))
                    {
                        throw new Exception("MxlDatasets: 'Output mxl file' is required for command 'modify-cs'");
                    }
                    if (String.IsNullOrWhiteSpace(model.MxlDatasets_ParameterName) ||
                        String.IsNullOrWhiteSpace(model.MxlDatasets_NewValue))
                    {
                        throw new Exception("MxlDatasets: 'Parameter name' and 'New value' are required for command 'modify-cs'");
                    }

                    parameters.Add("out-mxl", model.MxlDatasets_OutMxl);
                    parameters.Add("dataset-index", model.MxlDatasets_DatasetIndex);
                    parameters.Add("parameter", model.MxlDatasets_ParameterName);
                    parameters.Add("new-value", model.MxlDatasets_NewValue);
                }
                break;

            case "MxlToFdb":
                if (String.IsNullOrWhiteSpace(model.MxlToFdb_Mxl))
                {
                    throw new Exception("MxlToFdb: 'Mxl file' is required");
                }
                if (String.IsNullOrWhiteSpace(model.MxlToFdb_TargetConnectionString))
                {
                    throw new Exception("MxlToFdb: 'Target connection string' is required");
                }

                parameters.Add("mxl", model.MxlToFdb_Mxl);
                parameters.Add("target-connectionstring", model.MxlToFdb_TargetConnectionString);
                parameters.Add("target-guid", model.MxlToFdb_TargetGuid);

                if (!String.IsNullOrWhiteSpace(model.MxlToFdb_GeometryStorage) &&
                    !model.MxlToFdb_GeometryStorage.Equals("Classic", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<gView.Framework.Core.Data.GeometryStorageType>(model.MxlToFdb_GeometryStorage, true, out var storage) &&
                        !gView.Cmd.MxlUtil.Lib.Utilities.MxlToFdbStorage.IsCompatible(model.MxlToFdb_TargetGuid, storage))
                    {
                        throw new Exception(
                            $"MxlToFdb: geometry storage '{model.MxlToFdb_GeometryStorage}' is not valid for target database '{model.MxlToFdb_TargetGuid}'.");
                    }

                    parameters.Add("geometry-storage", model.MxlToFdb_GeometryStorage);
                }

                if (!String.IsNullOrWhiteSpace(model.MxlToFdb_OutMxl))
                {
                    parameters.Add("out-mxl", model.MxlToFdb_OutMxl);
                }
                if (!String.IsNullOrWhiteSpace(model.MxlToFdb_DontCopyFeaturesFrom))
                {
                    parameters.Add("dont-copy-features-from", model.MxlToFdb_DontCopyFeaturesFrom);
                }
                break;

            case "PublishService":
                if (String.IsNullOrWhiteSpace(model.PublishService_Mxl))
                {
                    throw new Exception("PublishService: 'Mxl file' is required");
                }
                if (String.IsNullOrWhiteSpace(model.PublishService_Server) ||
                    String.IsNullOrWhiteSpace(model.PublishService_Service))
                {
                    throw new Exception("PublishService: 'Server url' and 'Service' are required");
                }

                parameters.Add("mxl", model.PublishService_Mxl);
                parameters.Add("server", model.PublishService_Server);
                parameters.Add("service", model.PublishService_Service);

                if (!String.IsNullOrWhiteSpace(model.PublishService_Client))
                {
                    parameters.Add("client", model.PublishService_Client);
                }
                if (!String.IsNullOrWhiteSpace(model.PublishService_Secret))
                {
                    parameters.Add("secret", model.PublishService_Secret);
                }
                break;

            case "ConvertAprx":
                if (String.IsNullOrWhiteSpace(model.ConvertAprx_Input))
                {
                    throw new Exception("ConvertAprx: 'Input' is required");
                }

                parameters.Add("input", model.ConvertAprx_Input);

                if (!String.IsNullOrWhiteSpace(model.ConvertAprx_Output))
                {
                    parameters.Add("output", model.ConvertAprx_Output);
                }
                if (model.ConvertAprx_Silent)
                {
                    parameters.Add("silent", true);
                }
                if (!String.IsNullOrWhiteSpace(model.ConvertAprx_Dataset))
                {
                    parameters.Add("dataset", model.ConvertAprx_Dataset);
                }
                if (!String.IsNullOrWhiteSpace(model.ConvertAprx_DatasetConnectionString))
                {
                    parameters.Add("dataset-connectionstring", model.ConvertAprx_DatasetConnectionString);
                }
                if (!String.IsNullOrWhiteSpace(model.ConvertAprx_AllowOverlappingLabelsPriority))
                {
                    parameters.Add("allow-overlapping-labels-priority", model.ConvertAprx_AllowOverlappingLabelsPriority);
                }
                if (!String.IsNullOrWhiteSpace(model.ConvertAprx_CompositionModeCopyLayers))
                {
                    parameters.Add("composition-mode-copy-layers", model.ConvertAprx_CompositionModeCopyLayers);
                }
                if (!String.IsNullOrWhiteSpace(model.ConvertAprx_GlyphCenteringCorrection))
                {
                    parameters.Add("glyph-centering-correction", model.ConvertAprx_GlyphCenteringCorrection);
                }
                break;

            default:
                throw new Exception($"Unknown utility: {model.Utility}");
        }

        ICommand command = new MxlUtilCommand();

        await scope.ShowKnownDialog(
                    KnownDialogs.ExecuteCommand,
                    $"Run MXL Utility {model.Utility}",
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
