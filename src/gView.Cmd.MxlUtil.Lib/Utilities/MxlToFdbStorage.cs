using gView.Framework.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gView.Cmd.MxlUtil.Lib.Utilities;

/// <summary>
/// Which <see cref="GeometryStorageType"/> values are valid for a given MxlToFdb target database
/// (the <c>-target-guid</c> / "Target database" token <c>sqlite</c> | <c>postgres</c> | <c>sqlserver</c>).
/// Shared by the CLI (<see cref="MxlToFdb"/>) and the DataExplorer MxlUtil dialog.
/// </summary>
public static class MxlToFdbStorage
{
    public static IReadOnlyList<GeometryStorageType> CompatibleStorages(string? targetDb)
        => (targetDb ?? "").Trim().ToLowerInvariant() switch
        {
            "sqlite" => new[]
            {
                GeometryStorageType.Classic,
                GeometryStorageType.GeoPackage,
                GeometryStorageType.SpatiaLite,
            },
            "postgres" => new[]
            {
                GeometryStorageType.Classic,
                GeometryStorageType.PostGis,
            },
            "sqlserver" => new[]
            {
                GeometryStorageType.Classic,
                GeometryStorageType.SqlServerGeometry,
                GeometryStorageType.SqlServerGeography,
            },
            // an explicit provider GUID or unknown token - can't validate, allow anything
            _ => Enum.GetValues<GeometryStorageType>(),
        };

    /// <summary>True if <paramref name="storage"/> can be used with the given target database token.</summary>
    public static bool IsCompatible(string? targetDb, GeometryStorageType storage)
        => CompatibleStorages(targetDb).Contains(storage);

    /// <summary>Whether <paramref name="targetDb"/> is one of the known engine tokens we can validate.</summary>
    public static bool IsKnownTargetDb(string? targetDb)
        => (targetDb ?? "").Trim().ToLowerInvariant() is "sqlite" or "postgres" or "sqlserver";
}
