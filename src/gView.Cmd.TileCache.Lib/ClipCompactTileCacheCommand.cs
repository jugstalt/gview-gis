using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.Cmd.Core.Extensions;
using gView.Cmd.TileCache.Lib.Tools;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.TileCache.Lib;

public class ClipCompactTileCacheCommand : ICommand
{
    public string Name => "TileCache.ClipCompact";

    public string Description => "Clips a tile compact cache by polygon(s)";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions => new ICommandParameterDescription[]
    {
        new RequiredCommandParameter<string>("clip-type")
        {
            Description="[copy,cut,list] 'copy' the cache, 'cut'/'delelte' affected tiles or 'list' affected tiles",
        },
        new RequiredCommandParameter<string>("source-config")
        {
            Description = "Source tilecache path"
        },
        new CommandParameter<int>("max-level")
        {
            Description="optional: Max tilecacle level"
        },
        new RequiredCommandParameter<IFeatureClass>("clipper")
        {
            Description = "Featureclass with clipper polygons"
        },
        new CommandParameter<string>("clipper-query")
        {
            Description = "Definition query (Where Clause) for quering clipper polygons"
        },
        new CommandParameter<string>("target-folder")
        {
            Description = "or target tilecache path"
        },
        new CommandParameter<int>("jpeg-qual")
        {
            Description="optional: jpeg quality of output tiles"
        }
    };

    async public Task<bool> Run(
            IDictionary<string, object> parameters,
            ICancelTracker? cancelTracker = null,
            ICommandLogger? logger = null
        )
    {
        try
        {
            var clipperBuilder = new FeatureClassParameterBuilder("clipper");

            string sourceCache = parameters.GetRequiredValue<string>("source-config");
            IFeatureClass clipper = await clipperBuilder.Build<IFeatureClass>(parameters);
            string? clipperQuery = parameters.GetValueOrDefault<string>("clipper-query", null);
            TileCacheClipType clipType = parameters.GetValueOrDefault<string>("clip-type", "copy")!.ToLower() switch
            {
                "copy" => TileCacheClipType.Copy,
                "cut" => TileCacheClipType.Cut,
                "delete" => TileCacheClipType.Cut,
                "list" => TileCacheClipType.List,
                _ => throw new ArgumentException("Unknown clip type")
            };
            string targetCache = parameters.GetValueOrDefault<string>("target-folder", null) ?? "";
            int jpegQual = parameters.GetValueOrDefault<int>("jpeg-qual", -1);
            int maxLevel = parameters.GetValueOrDefault<int>("max-level", -1);

            var clip = new ClipCompact(cancelTracker);

            return await clip.Run(
                    sourceCache,
                    targetCache,
                    clipper,
                    clipperQuery,
                    jpegQuality: jpegQual,
                    maxlevel: maxLevel,
                    clipType: clipType,
                    logger: logger
                );
        }
        catch (Exception ex)
        {
            logger?.LogLine($"ERROR: {ex.Message}");

            return false;
        }
    }
}
