using gView.Cmd.MxlUtil.Lib.Utilities.Aprx;

namespace gView.Cmd.MxlUtil.Lib.Tests.Utilities.Aprx;

/// <summary>
/// Tests for <see cref="AprxMapConverter"/>'s <c>--connection-string</c> placeholder support:
/// parsing an aprx layer's own ArcGIS workspace connection string (SERVER=...;DATABASE=...;...)
/// and substituting "{key}" placeholders from it, case-insensitively, per layer (since not
/// every layer in an aprx necessarily comes from the same database/server).
/// </summary>
public class ConnectionStringPlaceholderTests
{
    private const string SampleWorkspaceConnectionString =
        "ENCRYPTED_PASSWORD_UTF8=00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff00;" +
        "ENCRYPTED_PASSWORD=00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff00;" +
        "SERVER=sqlhost01.example.com;" +
        @"INSTANCE=sde:sqlserver:sqlhost01.example.com\GISPROD;" +
        "DBCLIENT=sqlserver;" +
        @"DB_CONNECTION_PROPERTIES=sqlhost01.example.com\GISPROD;" +
        "DATABASE=gisdb;" +
        "USER=gis_reader;" +
        "VERSION=sde.DEFAULT;" +
        "AUTHENTICATION_MODE=DBMS";

    // -----------------------------------------------------------------------
    // ParseWorkspaceConnectionProperties
    // -----------------------------------------------------------------------

    [Fact]
    public void ParseWorkspaceConnectionProperties_SplitsEveryKeyValuePair()
    {
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(SampleWorkspaceConnectionString);

        Assert.Equal("sqlhost01.example.com", properties["SERVER"]);
        Assert.Equal("gisdb", properties["DATABASE"]);
        Assert.Equal("sqlserver", properties["DBCLIENT"]);
        Assert.Equal("gis_reader", properties["USER"]);
        Assert.Equal("sde.DEFAULT", properties["VERSION"]);
        Assert.Equal("DBMS", properties["AUTHENTICATION_MODE"]);
        Assert.Equal(@"sde:sqlserver:sqlhost01.example.com\GISPROD", properties["INSTANCE"]);
    }

    [Fact]
    public void ParseWorkspaceConnectionProperties_LookupIsCaseInsensitive()
    {
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(SampleWorkspaceConnectionString);

        Assert.Equal("gisdb", properties["database"]);
        Assert.Equal("gisdb", properties["Database"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParseWorkspaceConnectionProperties_NullOrEmptyInput_ReturnsEmptyDictionary(string? input)
    {
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(input);

        Assert.Empty(properties);
    }

    [Fact]
    public void ParseWorkspaceConnectionProperties_IgnoresEntriesWithoutEqualsSign()
    {
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties("SERVER=host;garbage;DATABASE=db");

        Assert.Equal(2, properties.Count);
        Assert.Equal("host", properties["SERVER"]);
        Assert.Equal("db", properties["DATABASE"]);
    }

    // -----------------------------------------------------------------------
    // ApplyConnectionStringPlaceholders
    // -----------------------------------------------------------------------

    [Fact]
    public void ApplyConnectionStringPlaceholders_ReplacesEveryPlaceholder()
    {
        var converter = new AprxMapConverter();
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(SampleWorkspaceConnectionString);

        var result = converter.ApplyConnectionStringPlaceholders(
            "Server={server};Database={database};User={user};Version={version}", properties);

        Assert.Equal("Server=sqlhost01.example.com;Database=gisdb;User=gis_reader;Version=sde.DEFAULT", result);
    }

    [Fact]
    public void ApplyConnectionStringPlaceholders_MatchesPlaceholderNameCaseInsensitively()
    {
        var converter = new AprxMapConverter();
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(SampleWorkspaceConnectionString);

        // Template uses lowercase placeholders; the aprx's own keys are uppercase (SERVER, DATABASE, ...).
        var result = converter.ApplyConnectionStringPlaceholders("{SERVER}/{Server}/{server}", properties);

        Assert.Equal("sqlhost01.example.com/sqlhost01.example.com/sqlhost01.example.com", result);
    }

    [Fact]
    public void ApplyConnectionStringPlaceholders_LeavesLiteralTextUntouched()
    {
        var converter = new AprxMapConverter();
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(SampleWorkspaceConnectionString);

        var result = converter.ApplyConnectionStringPlaceholders("Server={server};Password=SecurePassword;Pooling=true", properties);

        Assert.Equal("Server=sqlhost01.example.com;Password=SecurePassword;Pooling=true", result);
    }

    [Fact]
    public void ApplyConnectionStringPlaceholders_UnresolvedPlaceholder_LeftAsLiteralAndWarnedOnce()
    {
        var warnings = new List<string>();
        var converter = new AprxMapConverter(warn: warnings.Add);
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(SampleWorkspaceConnectionString);

        var result1 = converter.ApplyConnectionStringPlaceholders("Password={passwrd}", properties);
        var result2 = converter.ApplyConnectionStringPlaceholders("Password={passwrd}", properties);

        Assert.Equal("Password={passwrd}", result1);
        Assert.Equal("Password={passwrd}", result2);
        Assert.Single(warnings); // only warned once, not once per occurrence/layer
        Assert.Contains("passwrd", warnings[0]);
    }

    [Fact]
    public void ApplyConnectionStringPlaceholders_NoPlaceholders_ReturnsTemplateUnchanged()
    {
        var converter = new AprxMapConverter();

        var result = converter.ApplyConnectionStringPlaceholders("Server=fixed-host;Database=fixed-db", []);

        Assert.Equal("Server=fixed-host;Database=fixed-db", result);
    }

    [Fact]
    public void ApplyConnectionStringPlaceholders_DbnameAlias_ResolvesFromExplicitlyAddedKey()
    {
        // Mirrors how CreateFeatureClassFromPlugin seeds "dbname" into the properties dictionary
        // before calling this - from the qualified feature class name when present, otherwise
        // from the connection string's own DATABASE property.
        var converter = new AprxMapConverter();
        var properties = AprxMapConverter.ParseWorkspaceConnectionProperties(SampleWorkspaceConnectionString);
        properties["dbname"] = "gisdb";

        var result = converter.ApplyConnectionStringPlaceholders("Initial Catalog={dbname}", properties);

        Assert.Equal("Initial Catalog=gisdb", result);
    }
}
