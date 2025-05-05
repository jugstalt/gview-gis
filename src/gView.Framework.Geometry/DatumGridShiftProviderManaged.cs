using gView.Framework.Core.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gView.Framework.Geometry;

public class DatumGridShiftProviderManaged : IDatumGridShiftProvider
{
    #region IDatumGridShiftProvider Member

    public (string shortName, string name)[] GridShiftNames()
    {
        string projLibPath = Proj4Net.Core.IO.Paths.PROJ_LIB;

        if (String.IsNullOrEmpty(projLibPath) ||
            !Directory.Exists(projLibPath))
        {
            return [];
        }

        List<(string, string)> result = [("@null", "@null")];

        foreach (var file in Directory.GetFiles(projLibPath))
        {
            switch (System.IO.Path.GetExtension(file).ToLower())
            {
                case ".gsb":
                    result.Add((System.IO.Path.GetFileName(file), System.IO.Path.GetFileName(file)));
                    break;
            }
        }

        return result.ToArray();
    }

    public (string shortName, string name)[] EllipsoidNames()
        => Proj4Net.Core.Datum.Ellipsoid
            .Ellipsoids
            .Select(e => (e.ShortName, e.Name))
            .ToArray();

    public string GridParameter(string shiftName, string ellipsoidShortName)
        => (shiftName ?? "", ellipsoidShortName ?? "") switch
        {
            ("", _) => "",
            (_, "") => $"+nadgrids={shiftName}",
            (_, _) => $"+nadgrids={shiftName},ellps:{ellipsoidShortName}"
        };

    #endregion
}