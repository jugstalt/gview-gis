using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Threading.Tasks;

namespace gView.Cmd.Fdb.Lib.Data;

internal class CreateDataset
{
    public CreateDataset()
    {

    }

    async public Task<bool> CreateFeatureDataset(AccessFDB fdb,
                                                 string datasetName,
                                                 ISpatialReference sRef)
    {
        int dsID = await fdb.CreateDataset(datasetName, sRef);
        if (dsID < 0)
        {
            throw new Exception(fdb.LastErrorMessage);
        }

        return true;
    }

    async public Task<bool> CreateImageDataset(AccessFDB fdb,
                                               string datasetName,
                                               ISpatialReference sRef,
                                               string imageSpace,
                                               IFieldCollection additionalFields)
    {
        int dsID = await fdb.CreateImageDataset(datasetName, sRef, null, imageSpace, additionalFields);
        if(dsID < 0)
        {
            throw new Exception(fdb.LastErrorMessage);
        }

        return true;

    }
}
