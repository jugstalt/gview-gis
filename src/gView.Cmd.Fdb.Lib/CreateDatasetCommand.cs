using gView.Cmd.Core;
using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.Cmd.Core.Extensions;
using gView.DataSources.Fdb.MSAccess;
using gView.DataSources.Fdb.MSSql;
using gView.DataSources.Fdb.PostgreSql;
using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace gView.Cmd.Fdb.Lib;
public class CreateDatasetCommand : ICommand
{
    public string Name => "FDB.CreateDataset";

    public string Description => "Creates a new gView Feature Database FeatureClass";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions => new ICommandParameterDescription[]
    {
        new RequiredCommandParameter<string>("fdb")
        {
            Description = "FDB Type [SqlFDB | pgFDB | SQLiteFDB]"
        },
        new RequiredCommandParameter<string>("connection_string")
        {
            Description = "FDB Connectionstring"
        },
        new RequiredCommandParameter<string>("ds_name")
        {
            Description = "Dataset Name"
        },
        new RequiredCommandParameter<string>("sref_espg")
        {
            Description="EPSG Code <int> for datasets spatial reference system"
        },
        new RequiredCommandParameter<string>("ds_type")
        {
            Description="Dataset Type [FeatureDataset | ImageDataset]"
        },
        new CommandParameter<string>("geometry_storage")
        {
            Description="Geometry storage [Classic | PostGis | SqlServerGeometry | SqlServerGeography | SpatiaLite | GeoPackage]. Classic = gView proprietary blob (default). The native formats build no gView spatial index."
        },
        new CommandParameter<IEnvelope>("si_bounds")
        {
            Description = "Spatial Index Bounds (required for Classic storage, optional otherwise)"
        },
        new CommandParameter<int>("si_max_levels")
        {
            Description = "Maximal Spatial Index Levels (Classic storage only)"
        },
        new RequiredCommandParameter<string>("autofields")
        {
            Description = new AutoFieldsParameterBuilder().ParameterDescriptions?.FirstOrDefault()?.Description ?? String.Empty
        }
    };

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        try
        {

            #region FDB / Connectionstring

            AccessFDB? fdb = null;
            string connectionString = parameters.GetRequiredValue<string>("connection_string");

            switch (parameters.GetRequiredValue<string>("fdb").ToLower())
            {
                case "sql":
                case "sqlfdb":
                    fdb = new SqlFDB();
                    break;
                case "pg":
                case "postgres":
                case "pgfdb":
                    fdb = new pgFDB();
                    break;
                case "sqlite":
                case "sqlitefdb":
                    fdb = new SQLiteFDB();
                    break;
                default:
                    throw new ArgumentException($"Unknown FDB Type {parameters.GetRequiredValue<string>("fdb")}");
            }

            #endregion

            #region SpatialReference

            ISpatialReference spatialReference = new SpatialReference($"epsg:{parameters.GetRequiredValue<int>("sref_epsg")}");

            #endregion

            if (!await fdb.Open(connectionString))
            {
                throw new Exception($"Can't open FDB: {fdb.LastErrorMessage}");
            }

            var datasetType = parameters.GetRequiredValue<string>("ds_type");

            #region Spatial Index Def

            var storageString = parameters.GetValueOrDefault<string>("geometry_storage", nameof(GeometryStorageType.Classic));
            if (!Enum.TryParse<GeometryStorageType>(storageString, ignoreCase: true, out var storageType))
            {
                storageType = GeometryStorageType.Classic;
            }

            bool nativeStorage = storageType.IsDatabaseNative();

            // Bounds / levels are only required for the gView BinaryTree (Classic); optional otherwise.
            IEnvelope? siBounds = null;
            try { siBounds = await new EnvelopeParameterBuilder("si_bounds").Build<IEnvelope>(parameters); } catch { }

            int siMaxLevels = parameters.GetValueOrDefault<int>("si_max_levels", 0);

            bool haveSi = siBounds != null && siMaxLevels > 0;

            if (!nativeStorage && !haveSi && datasetType.Equals("ImageDataset", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Spatial index bounds / max levels are required for an image dataset.");
            }

            ISpatialIndexDef? spatialIndexDef = storageType switch
            {
                GeometryStorageType.PostGis => new PostGisSpatialIndexDef(siBounds, siMaxLevels),
                GeometryStorageType.SqlServerGeometry => new MSSpatialIndex { GeometryType = GeometryFieldType.MsGeometry, SpatialIndexBounds = siBounds ?? new Envelope() },
                GeometryStorageType.SqlServerGeography => new MSSpatialIndex { GeometryType = GeometryFieldType.MsGeography, SpatialIndexBounds = new Envelope() },
                GeometryStorageType.SpatiaLite or GeometryStorageType.GeoPackage => new gViewSpatialIndexDef(siBounds ?? new Envelope(), Math.Max(siMaxLevels, 0)) { StorageType = storageType },
                _ => haveSi ? new gViewSpatialIndexDef(siBounds, siMaxLevels) : null,   // Classic: keep legacy "no def without bounds"
            };

            #endregion

            if (datasetType.Equals("FeatureDataset", StringComparison.OrdinalIgnoreCase))
            {
                if (await fdb.CreateDataset(parameters.GetRequiredValue<string>("ds_name"), spatialReference, spatialIndexDef) < 0)
                {
                    throw new Exception($"Unable to create dataset: {fdb.LastErrorMessage}");
                }
            }
            else if (datasetType.Equals("ImageDataset", StringComparison.OrdinalIgnoreCase))
            {
                #region AutoFields

                var autoFieldsBuilder = new AutoFieldsParameterBuilder();
                var autoFields = await autoFieldsBuilder.Build<IFieldCollection>(parameters);

                #endregion

                if (await fdb.CreateImageDataset(
                    parameters.GetRequiredValue<string>("ds_name"),
                    spatialReference, spatialIndexDef, string.Empty, autoFields) < 0)
                {
                    throw new Exception($"Unable to create dataset: {fdb.LastErrorMessage}");
                }
            }
            else
            {
                throw new Exception($"Unknown dataset type: {datasetType}");
            }

            return true;
        }
        catch (Exception ex)
        {
            logger?.LogLine($"ERROR: {ex.Message}");

            return false;
        }
    }
}
