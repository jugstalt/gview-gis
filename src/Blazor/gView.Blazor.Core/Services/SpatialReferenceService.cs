using gView.Framework.Geometry;
using gView.Framework.Geometry.Proj;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Blazor.Core.Services;

public class SpatialReferenceService
{
    private readonly ProjDB _projectsDb;
    private readonly ProjDB _datumsDb;

    public SpatialReferenceService()
    {
        _projectsDb = new ProjDB(ProjDBTables.projs);
        _datumsDb = new ProjDB(ProjDBTables.datums);
    }

    public Task<IDictionary<string, string>> GetProjections()
    {
        var result = new Dictionary<string, string>();
        var dataTable = _projectsDb.GetTable();

        foreach (DataRow row in dataTable.Rows)
        {
            result.Add(row[0]?.ToString() ?? "", row[1]?.ToString() ?? "");
        }

        return Task.FromResult<IDictionary<string, string>>(result);
    }

    public Task<IDictionary<string, string>> GetDatums(bool addGridShifts = false, bool excludeGeoCentric = false)
    {
        var result = new Dictionary<string, string>();

        if (addGridShifts)
        {
            GeometricTransformerFactory.SupportedGridShifts().ToList().ForEach(gridShift =>
            {
                result.Add(gridShift.shortName, gridShift.name);
            });
        }

        var dataTable = _datumsDb.GetDatumTable(columns: "*");

        foreach (DataRow row in dataTable.Rows)
        {
            if(excludeGeoCentric && _datumsDb.IsGeoCentricDatum(row))
            {
                continue;
            }

            result.Add(row["DATUM_NAME"].ToString() ?? "", row["DATUM_NAME"].ToString() ?? "");
        }

        return Task.FromResult<IDictionary<string, string>>(result);
    }

    public Task<IDictionary<string, string>> GetEllipsoids()
    {
        var result = new Dictionary<string, string>();

        GeometricTransformerFactory.SupportedEllipsoids().ToList().ForEach(ellipsoid =>
        {
            result.Add(ellipsoid.shortName, ellipsoid.name);
        });

        return Task.FromResult<IDictionary<string, string>>(result);
    }

    public Task<SpatialReference> GetSpatialReference(string id)
    {
        return Task.FromResult(new SpatialReference(id));
    }

    public Task<GeodeticDatum> GetGeodeticDatum(string id)
    {
        return Task.FromResult(new GeodeticDatum(id));
    }
}
