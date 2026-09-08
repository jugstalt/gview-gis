using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Cmd.Fdb.Lib;

/// <summary>
/// Recomputes the extent of a feature class stored in a database-native geometry column
/// (PostGIS / SQL Server geometry|geography / SQLite SpatiaLite|GeoPackage) and rebuilds its
/// database-native spatial index (GiST / GEOMETRY_GRID / R-Tree). The gView BinaryTree is not
/// touched - use <see cref="RepairSpatialIndexCommand"/> for gView-managed (classic) storage.
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

        var storage = (featureClass.Dataset as IFDBDataset)?.SpatialIndexDef?.StorageType ?? GeometryStorageType.Classic;
        if (!storage.IsDatabaseNative())
        {
            throw new Exception($"Featureclass '{featureClass.Name}' uses gView-managed storage - use FDB.RepairSpatialIndex instead.");
        }

        logger?.LogLine($"Rebuild native spatial index: {featureClass.Name} ({storage})");

        if (!await fdb.RebuildNativeSpatialIndexAsync(featureClass.Name))
        {
            logger?.LogLine($"FDB.ERROR: {fdb.LastErrorMessage}");
            return false;
        }

        logger?.LogLine("done.");
        return true;
    }
}
