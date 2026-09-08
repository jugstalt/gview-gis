using gView.DataSources.Fdb.SQLite;
using gView.Framework.Core.Data;

namespace gView.DataSources.Fdb.SQLite.Tests;

public class SqliteFdbFileTests
{
    [Theory]
    [InlineData("db.fdb", GeometryStorageType.Classic)]
    [InlineData("db.FDB", GeometryStorageType.Classic)]
    [InlineData("db.fdb.gpkg", GeometryStorageType.GeoPackage)]
    [InlineData("C:/x/db.FDB.GPKG", GeometryStorageType.GeoPackage)]
    [InlineData("db.fdb.sqlite", GeometryStorageType.SpatiaLite)]
    [InlineData("db.gpkg", GeometryStorageType.Classic)]   // plain gpkg is not an FDB flavor
    [InlineData("db.sqlite", GeometryStorageType.Classic)]
    public void StorageFromFileName(string path, GeometryStorageType expected)
        => Assert.Equal(expected, SqliteFdbFile.StorageFromFileName(path));

    [Theory]
    [InlineData("db.fdb", true)]
    [InlineData("db.fdb.gpkg", true)]
    [InlineData("db.fdb.sqlite", true)]
    [InlineData("db.gpkg", false)]
    [InlineData("db.sqlite", false)]
    [InlineData("db.txt", false)]
    public void IsFdbFileName(string path, bool expected)
        => Assert.Equal(expected, SqliteFdbFile.IsFdbFileName(path));

    [Theory]
    [InlineData("db", GeometryStorageType.Classic, "db.fdb")]
    [InlineData("db", GeometryStorageType.GeoPackage, "db.fdb.gpkg")]
    [InlineData("db", GeometryStorageType.SpatiaLite, "db.fdb.sqlite")]
    [InlineData("db.fdb.gpkg", GeometryStorageType.GeoPackage, "db.fdb.gpkg")]     // already carries one
    [InlineData("db.fdb", GeometryStorageType.GeoPackage, "db.fdb")]              // keeps an existing recognised ext
    public void EnsureExtension(string name, GeometryStorageType storage, string expected)
        => Assert.Equal(expected, SqliteFdbFile.EnsureExtension(name, storage));
}
