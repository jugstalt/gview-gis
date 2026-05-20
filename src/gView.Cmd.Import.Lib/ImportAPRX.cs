using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Cmd.Import.Aprx;
using gView.Framework.Cartography;
using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.IO;
using gView.GraphicsEngine;

namespace gView.Cmd.Import;

public class ImportAPRX : ICommand
{
    public string Name => "ImportAPRX";

    public string Description => "Import an ESRI APRX File and save it as gView MXL";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions =>
    [
        new RequiredCommandParameter<string>("input")
        {
            Description = "Path to an ESRI APRX file or a directory containing APRX files"
        },
        new CommandParameter<string>("output")
        {
            Description = "Output path for the MXL file, or output directory when input is a directory (default: same location as input)"
        },
        new CommandParameter<bool>("silent")
        {
            Description = "If true, only the input file, errors, and the final result are printed (default: false)"
        },
        new CommandParameter<string>("dataset")
        {
            Description = "Plugin GUID of the dataset to use for all imported feature classes (optional). When omitted, an UnknownFeatureDataset is used."
        },
        new CommandParameter<string>("dataset-connectionstring")
        {
            Description = "Connection string for the dataset plugin specified by 'dataset' (optional)."
        }
    ];

    public async Task<bool> Run(
            IDictionary<string, object> parameters,
            ICancelTracker? cancelTracker = null,
            ICommandLogger? logger = null)
    {
        try
        {
            var input = parameters.GetRequiredValue<string>("input");
            var output = parameters.GetValueOrDefault<string>("output", null);
            var silent = parameters.GetValueOrDefault<bool>("silent", false);
            var datasetGuidStr = parameters.GetValueOrDefault<string>("dataset", null);
            var datasetCs = parameters.GetValueOrDefault<string>("dataset-connectionstring", null);

            DatasetPluginOptions? datasetOptions = null;
            if (!string.IsNullOrWhiteSpace(datasetGuidStr))
            {
                if (!Guid.TryParse(datasetGuidStr, out var datasetGuid))
                {
                    logger?.LogLine($"[ERROR] 'dataset' parameter is not a valid GUID: {datasetGuidStr}");
                    return false;
                }
                datasetOptions = new DatasetPluginOptions(datasetGuid, datasetCs ?? string.Empty);
            }

            var log = new AprxLogger(logger, silent);

            // Ensure graphics engine is available (required for symbol creation)
            if (Current.Engine == null)
            {
                SystemInfo.RegisterDefaultGraphicEngines();
            }

            if (Directory.Exists(input))
            {
                var aprxFiles = Directory.GetFiles(input, "*.aprx", SearchOption.TopDirectoryOnly);
                if (aprxFiles.Length == 0)
                {
                    log.Warning($"No APRX files found in directory: {input}");
                    return false;
                }

                var allSucceeded = true;
                foreach (var aprxFile in aprxFiles)
                {
                    var mxlFile = string.IsNullOrEmpty(output)
                        ? Path.ChangeExtension(aprxFile, ".mxl")
                        : Path.Combine(output, Path.ChangeExtension(Path.GetFileName(aprxFile), ".mxl"));

                    var success = await ConvertAprxAsync(aprxFile, mxlFile, log, datasetOptions, cancelTracker);
                    if (!success) allSucceeded = false;
                }

                log.PrintSummary();
                return allSucceeded;
            }
            else
            {
                var mxlFile = output ?? Path.ChangeExtension(input, ".mxl");
                var result = await ConvertAprxAsync(input, mxlFile, log, datasetOptions, cancelTracker);
                log.PrintSummary();
                return result;
            }
        }
        catch (Exception ex)
        {
            logger?.LogLine($"ERROR: {ex.Message}");
            logger?.LogLine(ex.StackTrace!);
            return false;
        }
    }

    private async Task<bool> ConvertAprxAsync(
            string aprxFile,
            string mxlFile,
            AprxLogger log,
            DatasetPluginOptions? datasetOptions,
            ICancelTracker? cancelTracker = null)
    {
        try
        {
            log.SetCurrentFile(aprxFile);
            log.Info($"Reading APRX: {aprxFile}", alwaysPrint: true);

            var reader = new AprxReader(aprxFile, warn: log.Warning);
            var mapResults = await reader.ReadMapsAsync();

            if (mapResults.Count == 0)
            {
                log.Warning("No maps found in the APRX file.");
                return false;
            }

            log.Info($"Found {mapResults.Count} map(s) in APRX.");

            var converter = new AprxMapConverter(warn: log.Warning, datasetPlugin: datasetOptions);

            var mapResult = mapResults[0];
            log.Info($"Converting map: '{mapResult.Map.Name}' ({mapResult.Layers.Count} layer(s))");

            var map = converter.Convert(mapResult);

            foreach (var layer in map.MapElements)
            {
                log.Info($"  Layer: {layer.Title}");
            }

            foreach (var dataset in map.Datasets)
            {
                log.Info($"  Dataset: {dataset.DatasetName}");
                var cs = dataset.ConnectionString;
                log.Info($"    ConnectionString={cs.Substring(0, Math.Min(50, cs.Length))}...");
            }

            // Persist as MXL
            var outDir = Path.GetDirectoryName(mxlFile);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            var doc = new gView.Cmd.MxlUtil.Lib.MxlDocument();
            doc.Readonly = true;
            doc.AddMap(map);
            doc.FocusMap = map;

            var xmlStream = new XmlStream("");
            xmlStream.Save("MapDocument", doc);

            xmlStream.WriteStream(mxlFile);
            log.Success($"Successfully migrated => {mxlFile}");

            return true;
        }
        catch (Exception ex)
        {
            log.Error($"{ex.Message}");
            log.Error(ex.StackTrace!);
            return false;
        }
    }

    #region Logging helper

    private sealed class AprxLogger(ICommandLogger? inner, bool silent)
    {
        private readonly List<(string File, string Message)> _warnings = [];
        private readonly List<(string File, string Message)> _errors = [];
        private string _currentFile = string.Empty;

        /// <summary>Sets the APRX file currently being processed (used to tag warnings/errors).</summary>
        public void SetCurrentFile(string filePath) => _currentFile = filePath;

        /// <summary>Verbose output – suppressed in silent mode.</summary>
        public void Info(string message, bool alwaysPrint = false)
        {
            if (silent && !alwaysPrint) return;
            inner?.LogLine(message);
        }

        /// <summary>Always printed, yellow on console.</summary>
        public void Warning(string message)
        {
            _warnings.Add((_currentFile, message));
            WriteColored($"[WARN] {message}", ConsoleColor.Yellow);
            //inner?.LogLine($"[WARN] {message}");
        }

        /// <summary>Always printed, red on console.</summary>
        public void Error(string message)
        {
            _errors.Add((_currentFile, message));
            WriteColored($"[ERROR] {message}", ConsoleColor.Red);
            //inner?.LogLine($"[ERROR] {message}");
        }

        /// <summary>Always printed, green on console.</summary>
        public void Success(string message)
        {
            WriteColored(message, ConsoleColor.Green);
            //inner?.LogLine(message);
        }

        /// <summary>Prints a grouped summary of all collected warnings and errors, grouped by source file.</summary>
        public void PrintSummary()
        {
            if (_warnings.Count == 0 && _errors.Count == 0)
            {
                return;
            }

            WriteColored("\n── Summary ──────────────────────────────────────", ConsoleColor.White);
            //inner?.LogLine("── Summary ──────────────────────────────────────");

            PrintGroup("warning", _warnings, ConsoleColor.Yellow, "[WARN]");
            PrintGroup("error",   _errors,   ConsoleColor.Red,    "[ERROR]");

            WriteColored("─────────────────────────────────────────────────", ConsoleColor.White);
            //inner?.LogLine("─────────────────────────────────────────────────");
        }

        private void PrintGroup(
            string label,
            List<(string File, string Message)> entries,
            ConsoleColor color,
            string prefix)
        {
            if (entries.Count == 0) return;

            WriteColored($"  {entries.Count} {label}(s):", color);
            //inner?.LogLine($"  {entries.Count} {label}(s):");

            foreach (var fileGroup in entries.GroupBy(e => e.File))
            {
                var fileLabel = string.IsNullOrEmpty(fileGroup.Key)
                    ? "(unknown file)"
                    : Path.GetFileName(fileGroup.Key);

                WriteColored($"    {fileLabel}", color);
                //inner?.LogLine($"    {fileLabel}");

                foreach (var (_, msg) in fileGroup)
                {
                    WriteColored($"      {prefix} {msg}", color);
                    //inner?.LogLine($"      {prefix} {msg}");
                }
            }
        }

        private static void WriteColored(string message, ConsoleColor color)
        {
            var prev = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = prev;
        }
    }

    #endregion
}
