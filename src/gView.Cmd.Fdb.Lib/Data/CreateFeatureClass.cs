using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Core.Data;
using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using System;
using System.Threading.Tasks;

namespace gView.Cmd.Fdb.Lib.Data;
internal class CreateFeatureClass
{
    public CreateFeatureClass()
    {

    }

    async public Task<bool> Create(IFeatureDataset featureDataset,
                             string fcName,
                             IGeometryDef gDef,
                             IFieldCollection fields,
                             BinaryTreeDef binaryTreeDef)
    {
        var fdb = featureDataset.Database as AccessFDB;

        if (fdb == null)
        {
            throw new Exception("Database is not a gView Feature Database");
        }

        int fcId = await fdb.CreateFeatureClass(
            featureDataset.DatasetName,
            fcName,
            gDef,
            fields);

        if (fcId < 0)
        {
            throw new Exception(fdb.LastErrorMessage);
        }

        var storage = (await fdb.SpatialIndexDef(await fdb.DatasetID(featureDataset.DatasetName)))?.StorageType
                      ?? gView.Framework.Core.Data.GeometryStorageType.Classic;

        bool indexOk = storage switch
        {
            gView.Framework.Core.Data.GeometryStorageType.PostGis when fdb is gView.DataSources.Fdb.PostgreSql.pgFDB pg
                => pg.SetPostGisSpatialIndex(fcName, binaryTreeDef.Bounds),
            gView.Framework.Core.Data.GeometryStorageType.SqlServerGeometry when fdb is gView.DataSources.Fdb.MSSql.SqlFDB sql1
                => await SetMsIndex(sql1, fcName, gView.Framework.Core.Data.GeometryFieldType.MsGeometry, binaryTreeDef),
            gView.Framework.Core.Data.GeometryStorageType.SqlServerGeography when fdb is gView.DataSources.Fdb.MSSql.SqlFDB sql2
                => await SetMsIndex(sql2, fcName, gView.Framework.Core.Data.GeometryFieldType.MsGeography, binaryTreeDef),
            _ => await fdb.SetSpatialIndexBounds(fcName, "BinaryTree2", binaryTreeDef.Bounds, 0.55, binaryTreeDef.MaxPerNode, binaryTreeDef.MaxLevel),
        };

        if (!indexOk)
        {
            await fdb.DeleteFeatureClass(fcName);
            throw new Exception(fdb.LastErrorMessage);
        }

        return true;
    }

    private static Task<bool> SetMsIndex(gView.DataSources.Fdb.MSSql.SqlFDB fdb, string fcName,
        gView.Framework.Core.Data.GeometryFieldType type, BinaryTreeDef bounds)
    {
        var msIndex = new MSSpatialIndex
        {
            GeometryType = type,
            SpatialIndexBounds = bounds.Bounds,
        };
        return Task.FromResult(fdb.SetMSSpatialIndex(msIndex, fcName));
    }
}
