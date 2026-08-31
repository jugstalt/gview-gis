using gView.Cmd.Core.Abstraction;
using gView.Cmd.MxlUtil.Lib.Abstraction;
using gView.Cmd.MxlUtil.Lib.Exceptions;
using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;
using gView.Framework.Common;
using gView.Framework.Core.Carto;
using gView.Framework.Core.Common;
using gView.Framework.IO;
using gView.GraphicsEngine;

namespace gView.Cmd.MxlUtil.Lib.Utilities;

internal class ConvertAprx : IMxlUtility
{
    public string Name => "ConvertAprx";

    public string Description()
    {
        return """
              ConvertAprx:
              ------------
              Converts ESRI Aprx files to MXL (experimental).
              """;
    }

    public string HelpText()
    {
        return
            """
            Required arguments:
            -input <an aprx file or a folder with aprx files>

            Optional arguments:
            -output <Output path for the MXL file, or output directory when input is a directory (default: same location as input)>
            -silent <If true, only the input file, errors, and the final result are printed (default: false)>
            
            -dataset <Plugin GUID of the dataset to use for all imported feature classes (optional). When omitted, an UnknownFeatureDataset is used.>
            -dataset-connectionstring <Connection string for the dataset plugin specified by 'dataset' (optional).
                May contain "{key}" placeholders (case-insensitive) resolved per layer from that
                layer's own workspace connection string in the aprx - e.g. "{server}", "{instance}",
                "{database}", "{dbclient}", "{user}", "{version}", "{authentication_mode}" for a
                typical SDE connection. "{dbname}" is a legacy alias for the database name (from
                the qualified feature class name "dbname.schema.table" when present, otherwise the
                connection string's own DATABASE value). Useful since not every layer in an aprx
                necessarily comes from the same database/server - each gets its own resolved
                connection string.
                Example: "SERVER={server};DATABASE={database};USER=svc;PASSWORD=secret">

            -allow-overlapping-labels-priority <gView label priority a label class with ArcGIS
                Pro's "Allow overlapping labels" checked (Standard engine) is converted to - one
                of Always, High, Normal, Low (case-insensitive; default: Always). Always most
                closely matches the checkbox's own name, but skips gView's overlap check entirely
                and always places at the first candidate position - unlike ArcGIS Pro, which still
                tries a normal placement first and only allows overlap as a fallback, so a busy
                layer can come out visibly noisier than in ArcGIS Pro. Pass e.g. "High" for a
                gentler equivalent that's still checked, just preferred over Normal/Low labels.>

            -composition-mode-copy-layers <Comma-separated list of layer-name wildcard patterns
                ("*"/"?", case-insensitive - e.g. "*streifen*,*Kabeltrasse*") opting a transparent
                layer (aprx "transparency" > 0) into gView's CompositionMode.Copy instead of the
                default of baking that transparency into every symbol color. Baking it into colors
                is wrong whenever the layer's own features can overlap themselves - e.g. many
                crossing semi-transparent line/polygon symbols at the same transparency, common for
                corridor/buffer-strip style layers: each overlap gets drawn/blended twice, producing
                a visibly darker seam that doesn't exist in ArcGIS Pro (which always composites a
                layer once, then applies its transparency to the whole result). Not applied to any
                layer by default, since rendering to an extra full-size bitmap first costs real
                memory/CPU per matched layer at render time - only opt in the specific layers that
                actually show the artifact.
                Example: "*streifen*">

            -glyph-centering-correction <Manual overrides for gView's automatic glyph-ink-centering
                correction, as one or more "FontFamilyName:CharacterIndex(ReferenceSize,X,Y)"
                entries (font name case-insensitive, character index 0-255), comma-separated for
                more than one - e.g. "STROM SSG:148(36,2.9616666,-6.7749996)" means "at font size
                36, this glyph is exactly centered at offset X=2.9616666, Y=-6.7749996", read off
                directly as gView's own HorizontalOffset/VerticalOffset by nudging the glyph to
                visually centered in gView.Carto's symbol editor at that size. X/Y are scaled by
                ReferenceSize for whatever size a marker actually uses, and *replace* that marker's
                own anchorPoint/offsetX/offsetY entirely - not add on top of them - since the
                observed "centered" state at calibration time already reflects whatever
                anchor/offset that instance needed; re-adding a different marker's own separate
                anchor/offset on top would double-count it. A marker that itself has a deliberate,
                unrelated anchorPoint (e.g. offsetting from the feature point for cartographic
                reasons, not to work around font metrics) loses that wherever the same override
                applies - there's no way to tell the two apart from the override value alone, so
                only reuse one entry across markers that are fine sharing it, and calibrate against
                an anchor-free test symbol (no anchorPoint/offsetX/offsetY set) if you want an
                entry that's safe to reuse everywhere this font+character appears. The automatic
                correction exists because some ArcGIS Pro dingbat/symbol fonts ship bogus
                ascent/descent metadata, so gView measures the glyph's actually-rendered ink and
                re-centers on it - but for a character that bakes in a dominant shape *plus* a
                separate, deliberately off-center attached label (e.g. a circle with a short
                abbreviation next to it, both part of the same glyph), that correction centers on
                the combined ink and drags the shape away from where it should sit. There's no
                reliable way to detect this automatically without also mis-centering ordinary
                glyphs that legitimately need their whole ink included (e.g. "?"/"i"/"j" and their
                dot), so override only the specific font+character combos you have actually seen
                mis-centered, with an exact measurement rather than a heuristic - not every symbol
                necessarily needs one.
                Single entry:    "STROM SSG:148(36,2.9616666,-6.7749996)"
                Multiple entries: "STROM SSG:148(36,2.9616666,-6.7749996),STROM SSG:66(20,1,2)">
            """;
    }

    async public Task<bool> Run(string[] args, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {

        string input = "", output = "", datasetGuidStr = "", datasetCs = "", allowOverlappingLabelsPriorityStr = "", compositionModeCopyLayersStr = "", glyphCenteringCorrectionStr = "";
        bool silent = false;

        for (int i = 0; i < args.Length - 1; i++)
        {
            switch (args[i].ToLower())
            {
                case "-input":
                    input = args[++i];
                    break;
                case "-output":
                    output = args[++i];
                    break;
                case "-silent":
                    silent = "true".Equals(args[++i], StringComparison.OrdinalIgnoreCase);
                    break;
                case "-dataset":
                case "-ds":
                    datasetGuidStr = args[++i];
                    break;
                case "-dataset-connectionstring":
                case "-ds-cs":
                    datasetCs = args[++i];
                    break;
                case "-allow-overlapping-labels-priority":
                case "-aolp":
                    allowOverlappingLabelsPriorityStr = args[++i];
                    break;
                case "-composition-mode-copy-layers":
                case "-cmcl":
                    compositionModeCopyLayersStr = args[++i];
                    break;
                case "-glyph-centering-correction":
                case "-gcc":
                    glyphCenteringCorrectionStr = args[++i];
                    break;
            }
        }

        if (String.IsNullOrEmpty(input))
        {
            throw new IncompleteArgumentsException();
        }

        try
        {
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

            var allowOverlappingLabelsPriority = RenderLabelPriority.Always;
            if (!string.IsNullOrWhiteSpace(allowOverlappingLabelsPriorityStr))
            {
                if (!Enum.TryParse(allowOverlappingLabelsPriorityStr, ignoreCase: true, out allowOverlappingLabelsPriority))
                {
                    logger?.LogLine($"[ERROR] 'allow-overlapping-labels-priority' is not a valid value: {allowOverlappingLabelsPriorityStr} (expected Always, High, Normal, or Low)");
                    return false;
                }
            }

            var compositionModeCopyLayerPatterns = compositionModeCopyLayersStr
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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

                    var success = await ConvertAprxAsync(aprxFile, mxlFile, log, datasetOptions, allowOverlappingLabelsPriority, compositionModeCopyLayerPatterns, glyphCenteringCorrectionStr, cancelTracker);
                    if (!success) allSucceeded = false;
                }

                log.PrintSummary();
                return allSucceeded;
            }
            else
            {
                var mxlFile = output ?? Path.ChangeExtension(input, ".mxl");
                var result = await ConvertAprxAsync(input, mxlFile, log, datasetOptions, allowOverlappingLabelsPriority, compositionModeCopyLayerPatterns, glyphCenteringCorrectionStr, cancelTracker);
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
            RenderLabelPriority allowOverlappingLabelsPriority,
            IEnumerable<string> compositionModeCopyLayerPatterns,
            string glyphCenteringCorrection,
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

            var converter = new AprxMapConverter(
                warn: log.Warning,
                info: msg => log.Info(msg, alwaysPrint: true),
                datasetPlugin: datasetOptions,
                allowOverlappingLabelsPriority: allowOverlappingLabelsPriority,
                compositionModeCopyLayerPatterns: compositionModeCopyLayerPatterns,
                glyphCenteringCorrection: glyphCenteringCorrection);

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
            //doc.Readonly = true;
            doc.AddMap(map);
            doc.FocusMap = map;

            var xmlStream = new XmlStream("");
            xmlStream.Save("MapDocument", doc);

            var xmlFileInfo = new FileInfo(mxlFile);
            if (xmlFileInfo.Directory?.Exists == false)
            {
                xmlFileInfo.Directory.Create();
            }

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
            PrintGroup("error", _errors, ConsoleColor.Red, "[ERROR]");

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
