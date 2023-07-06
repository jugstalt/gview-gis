using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using gView.Framework.FDB;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
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
                             IFieldCollection fields)
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
            throw new Exception("ERROR: " + fdb.LastErrorMessage);
        }
    }
}
