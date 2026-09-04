using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.Fdb.Lib;

/// <summary>
/// Recomputes the extent of a feature class stored in a database-native geometry column
/// (PostGIS / SQL Server geometry|geography) and rebuilds its database-native spatial index
/// (GiST / GEOMETRY_GRID / GEOGRAPHY_GRID). The gView BinaryTree is not touched - use
/// <see cref="RepairSpatialIndexCommand"/> for gView-managed (classic / WKB) storage.
/// </summary>
public class RepairNativeSpatialIndexCommand : ICommand
{
    public string Name => "FDB.RepairNativeSpatialIndex";

    public string Description => "Recalculate the extent and rebuild the database-native spatial index of a native FDB featureclass";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions => new ICommandParameterDescription[]
    {
        new RequiredCommandParameter<IFeatureClass>("dataset")
        {
            Description = "FDB Featureclass"
        }
    };

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        var featureClass = await new FeatureClassParameterBuilder("dataset").Build<IFeatureClass>(parameters);

        if (featureClass is null)
        {
            throw new Exception("Can't build featureclass");
        }

        if (featureClass is IRasterClass)
        {
            // e.g. the <ds> raster-catalog element of an image dataset - only <ds>_IMAGE_POLYGONS
            // has a spatial index.
            throw new Exception($"'{featureClass.Name}' is a raster catalog, not a plain feature class.");
        }

        if (featureClass.Dataset?.Database is not AccessFDB fdb)
        {
            throw new Exception("Dataset is not a gView FDB Dataset");
        }

        var sIndexDef = (featureClass.Dataset as IFDBDataset)?.SpatialIndexDef;
        var storage = sIndexDef?.StorageType ?? GeometryStorageType.Classic;

        if (storage is GeometryStorageType.Classic or GeometryStorageType.Wkb)
        {
            throw new Exception($"Featureclass '{featureClass.Name}' uses gView-managed storage - use FDB.RepairSpatialIndex instead.");
        }

        logger?.LogLine($"Calculate extent: {featureClass.Name}");
        await fdb.CalculateExtent(featureClass);
        IEnvelope extent = await fdb.QueryExtent(featureClass.Name) ?? new Envelope();

        bool ok;
        switch (storage)
        {
            case GeometryStorageType.PostGis when fdb is pgFDB pg:
                logger?.LogLine("Rebuild PostGIS GiST index...");
                ok = pg.SetPostGisSpatialIndex(featureClass.Name, extent);
                break;

            case GeometryStorageType.SqlServerGeometry when fdb is SqlFDB sqlGeom:
                ok = RebuildMsIndex(sqlGeom, sIndexDef, featureClass.Name, GeometryFieldType.MsGeometry, extent, logger);
                break;

            case GeometryStorageType.SqlServerGeography when fdb is SqlFDB sqlGeog:
                ok = RebuildMsIndex(sqlGeog, sIndexDef, featureClass.Name, GeometryFieldType.MsGeography, extent, logger);
                break;

            default:
                throw new Exception($"Native storage '{storage}' is not supported by this FDB provider.");
        }

        if (ok && !Envelope.IsNull(extent))
        {
            await fdb.SetFeatureclassExtent(featureClass.Name, extent);
        }

        if (!ok)
        {
            logger?.LogLine($"FDB.ERROR: {fdb.LastErrorMessage}");
            return false;
        }

        logger?.LogLine($"Rebuilt native spatial index: {featureClass.Name}");
        return true;
    }

    private static bool RebuildMsIndex(
        SqlFDB fdb, ISpatialIndexDef? sIndexDef, string fcName, GeometryFieldType type, IEnvelope extent, ICommandLogger? logger)
    {
        var msIndex = sIndexDef as MSSpatialIndex ?? new MSSpatialIndex();
        msIndex.GeometryType = type;

        // GEOMETRY_GRID needs a valid bounding box; geography ignores it.
        if (!Envelope.IsNull(extent))
        {
            msIndex.SpatialIndexBounds = extent;
        }

        logger?.LogLine("Rebuild SQL Server spatial index...");
        return fdb.SetMSSpatialIndex(msIndex, fcName);
    }
}
