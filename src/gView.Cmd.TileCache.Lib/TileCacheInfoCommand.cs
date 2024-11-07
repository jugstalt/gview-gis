using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Extensions;
using gView.Cmd.TileCache.Lib.Extensions;
using gView.Framework.Core.Common;
using gView.Framework.Metadata;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.TileCache.Lib;
internal class TileCacheInfoCommand : ICommand
{
    public string Name => "TileCache.Info";

    public string Description => "Shows information about a tilecache service";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions => new ICommandParameterDescription[]
    {
        new RequiredCommandParameter<string>("server")
        {
            Description = "gView Server Instance, eg. https://my-server/gview-server"
        },
        new RequiredCommandParameter<string>("service")
        {
            Description = "The service to pre-render, eg. folder@servicename"
        }

    };

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        try
        {
            string server = parameters.GetRequiredValue<string>("server");
            string service = parameters.GetRequiredValue<string>("service");

            #region Read Metadata

            var metadata = await new TileServiceMetadata().FromService(server, service);
            if (metadata == null)
            {
                throw new Exception("Can't read metadata from server. Are you sure that ervice is a gView WMTS service?");
            }

            #endregion

            #region TileSize

            logger?.LogLine($"TileSize [Pixel]: {metadata.TileWidth} x {metadata.TileHeight}");

            #endregion

            #region ImageFormat

            logger?.Log("ImageFormats:");
            logger?.Log(metadata.FormatJpg ? " jpg" : "");
            logger?.Log(metadata.FormatPng ? " png" : "");
            logger?.LogLine("");

            #endregion

            #region Scales

            logger?.LogLine("Scales:");
            if (metadata.Scales != null)
            {
                foreach (var scale in metadata.Scales.ToArray())
                {
                    logger?.LogLine($"  1 : {scale}");
                }
            }

            #endregion

            #region Origin

            logger?.Log("Origin:");
            logger?.Log(metadata.UpperLeft ? " upperleft" : "");
            logger?.Log(metadata.LowerLeft ? " lowerleft" : "");
            logger?.LogLine("");

            if (metadata.EPSGCodes != null)
            {
                foreach (var epsgCode in metadata.EPSGCodes)
                {
                    if (metadata.UpperLeft)
                    {
                        var ul = metadata.GetOriginUpperLeft(epsgCode);
                        logger?.LogLine($"  EPSG:{epsgCode} upperleft: {ul.X}, {ul.Y}");
                    }
                    if (metadata.LowerLeft)
                    {
                        var ll = metadata.GetOriginUpperLeft(epsgCode);
                        logger?.LogLine($"  EPSG:{epsgCode} lowerleft: {ll.X}, {ll.Y}");
                    }
                }
            }

            #endregion

            #region Extents

            logger?.LogLine("BBox:");
            if (metadata.EPSGCodes != null)
            {
                foreach (var epsgCode in metadata.EPSGCodes)
                {
                    var envelope = metadata.GetEPSGEnvelope(epsgCode);
                    if (envelope != null)
                    {
                        logger?.LogLine($"  EPSG:{epsgCode}: {envelope.MinX}, {envelope.MinY}, {envelope.MaxX}, {envelope.MaxY}");
                    }
                }
            }

            #endregion

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogLine($"ERROR: {ex.Message}");

            return false;
        }
    }
}

