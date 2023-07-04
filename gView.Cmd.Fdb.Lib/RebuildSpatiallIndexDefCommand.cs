using gView.Cmd.Core.Abstraction;
using gView.Cmd.Core.Builders;
using gView.Cmd.Core;
using gView.Cmd.Fdb.Lib.SpatialIndex;
using gView.DataSources.Fdb.MSAccess;
using gView.Framework.Data;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Cmd.Fdb.Lib;

internal class RebuildSpatiallIndexDefCommand : ICommand
{
    public string Name => "FDB.RebuildSpatiallIndexDef";

    public string Description => "Rebuild spatial index of a featureclasses";

    public string ExecutableName => "";

    public IEnumerable<ICommandParameterDescription> ParameterDescriptions => new ICommandParameterDescription[]
    {
    new RequiredCommandParameter<IFeatureClass>("dataset")
    {
        Description="FDB Featureclass"
    }
    };

    async public Task<bool> Run(IDictionary<string, object> parameters, ICancelTracker? cancelTracker = null, ICommandLogger? logger = null)
    {
        var featureClassBuilder = new FeatureClassParameterBuilder("dataset");
        var featureClass = await featureClassBuilder.Build<IFeatureClass>(parameters);

        if (featureClass is null)
        {
            throw new Exception("Can't build featureclass");
        }

        if (!(featureClass.Dataset?.Database is AccessFDB))
        {
            throw new Exception("Dataset is not a gView FDB Dataset");
        }

        var rebuilder = new Rebuilder(cancelTracker);
        rebuilder.ReportAction += (sender, message) =>
        {
            logger?.LogLine(message);
        };
        await rebuilder.RebuildIndicesAsync(featureClass);

        return true;
    }
}
