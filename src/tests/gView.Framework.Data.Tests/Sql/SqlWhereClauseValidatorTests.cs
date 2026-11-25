using gView.Framework.Data.Extensions;
using gView.Framework.Data.Sql;

namespace gView.Framework.Data.Tests.Sql;

[Collection(SqlWhereClauseValidatorCollection.Name)]
public class SqlWhereClauseValidatorTests
{
    [Theory]
    [InlineData("1=1")]
    [InlineData("1 = 1")]
    [InlineData("NAME = 'Graz'")]
    [InlineData("NAME = 'O''Brien'")]
    [InlineData("NAME = N'Graz'")]
    [InlineData("POPULATION > 1000 AND AREA_KM2 <= 12.5")]
    [InlineData("POPULATION <> 3 OR POPULATION != -2")]
    [InlineData("POPULATION >= 1.5e3")]
    [InlineData("OBJECTID IN (1, 2, 3)")]
    [InlineData("OBJECTID NOT IN (1,2)")]
    [InlineData("NAME LIKE 'G%' AND NAME NOT LIKE '%x%'")]
    [InlineData("NAME LIKE 'a!%' ESCAPE '!'")]
    [InlineData("NAME IS NULL OR NAME IS NOT NULL")]
    [InlineData("POPULATION BETWEEN 1 AND 10")]
    [InlineData("NOT (POPULATION = 1)")]
    [InlineData("(NAME = 'a' OR NAME = 'b') AND POPULATION > 0")]
    [InlineData("POPULATION + AREA_KM2 * 2 / 4 - 1 > 0")]
    [InlineData("name = 'graz'")]                               // case insensitive fields
    [InlineData("\"Feld A\" = 'x' AND [Feld A] = 'y'")]        // quoted identifiers
    [InlineData("Straße = 'x'")]
    [InlineData("UPPER(NAME) = 'GRAZ' AND LOWER(NAME) = 'graz'")]
    [InlineData("CHAR_LENGTH(TRIM(NAME)) > 2")]
    [InlineData("CAST(POPULATION AS VARCHAR(10)) = '5'")]
    [InlineData("CAST(AREA_KM2 AS DOUBLE PRECISION) > 1")]
    [InlineData("CAST(AREA_KM2 AS DECIMAL(10,2)) > 1")]
    [InlineData("EXTRACT(YEAR FROM CREATED) = 2020")]
    [InlineData("CREATED > date '2020-01-01'")]
    [InlineData("CREATED > timestamp '2020-01-01 00:00:00'")]
    [InlineData("CREATED < CURRENT_TIMESTAMP")]
    [InlineData("CREATED < CURRENT_DATE")]
    [InlineData("ACTIVE = TRUE")]
    [InlineData("NAME = 'a\\b'")]                             // backslash not before a quote
    [InlineData("POPULATION=1\r\nAND\tAREA_KM2=2")]
    public void Validate_AllowedWhereClause_DoesNotThrow(string where)
    {
        SqlWhereClauseValidator.Validate(where, FakeFeatureClass.Default());
        SqlWhereClauseValidator.Validate(where);
    }

    [Theory]
    [InlineData("NAME = 'x'; DROP TABLE t")]
    [InlineData("NAME = 'x' -- comment")]
    [InlineData("NAME = 'x' /* comment */")]
    [InlineData("OBJECTID IN (SELECT OBJECTID FROM t)")]
    [InlineData("NAME = 'x' UNION SELECT password FROM users")]
    [InlineData("1=1 AND pg_sleep(10) IS NULL")]
    [InlineData("1=1 WAITFOR DELAY '0:0:5'")]
    [InlineData("@x = 1")]
    [InlineData("NAME = 0x41")]
    [InlineData("NAME = $1")]
    [InlineData("NAME::text = 'x'")]
    [InlineData("NAME = 'x' ORDER BY NAME")]
    [InlineData("NAME = CONVERT(int, NAME)")]
    [InlineData("NAME = 'x' COLLATE Latin1_General_CI_AS")]
    [InlineData("POPULATION % 2 = 0")]
    [InlineData("NAME || 'x' = 'y'")]
    [InlineData("NAME = E'x'")]                                 // PostgreSQL escape string
    [InlineData("NAME = 'x\\'' OR 1=1 OR ''='")]                // backslash escaped quote (other databases)
    [InlineData("NAME = 'unterminated")]
    [InlineData("\"NAME = 1")]
    [InlineData("NAME = {fn NOW()}")]
    [InlineData("NAME = `x`")]
    [InlineData("dbo.users.password = 'x'")]
    [InlineData("NAME = sys.fn_x()")]
    [InlineData("NAME AS x")]
    [InlineData("NAME FROM t")]
    [InlineData("EXTRACT(NAME FROM CREATED) = 1")]
    [InlineData("(NAME = 'x'")]
    [InlineData("NAME = 'x')")]
    [InlineData("NAME = 'x' OR 1=1")]                      // non-breaking space
    [InlineData("NAME = 'x' OR ILIKE('x')")]
    public void Validate_DangerousWhereClause_Throws(string where)
    {
        Assert.Throws<SqlDangerousStatementExceptions>(() => SqlWhereClauseValidator.Validate(where, FakeFeatureClass.Default()));
        Assert.Throws<SqlDangerousStatementExceptions>(() => SqlWhereClauseValidator.Validate(where));
    }

    [Theory]
    [InlineData("PASSWORD = 'x'")]
    [InlineData("NAME = OTHER")]
    [InlineData("\"NAM\"\"E\" = 'x'")]
    public void Validate_UnknownField_ThrowsOnlyWithTableClass(string where)
    {
        SqlWhereClauseValidator.Validate(where);

        var ex = Assert.Throws<SqlDangerousStatementExceptions>(() => SqlWhereClauseValidator.Validate(where, FakeFeatureClass.Default()));
        Assert.Contains("Unknown field", ex.Message);
    }

    [Fact]
    public void Validate_DottedFunctionCall_ThrowsEvenIfFieldExists()
    {
        // the fake table has a field "SHAPE.STArea()", but method calls are never allowed in where clauses
        Assert.Throws<SqlDangerousStatementExceptions>(
            () => SqlWhereClauseValidator.Validate("SHAPE.STArea() > 5", FakeFeatureClass.Default()));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_Empty_DoesNotThrow(string? where)
    {
        SqlWhereClauseValidator.Validate(where, FakeFeatureClass.Default());
    }

    [Fact]
    public void Validate_RejectedMessage_DoesNotContainWholeWhereClause()
    {
        var ex = Assert.Throws<SqlDangerousStatementExceptions>(
            () => SqlWhereClauseValidator.Validate("NAME = 'secret' UNION SELECT password FROM users"));

        Assert.Contains("UNION", ex.Message);
        Assert.DoesNotContain("secret", ex.Message);
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void Validate_AdditionalAllowedFunction_IsAccepted()
    {
        try
        {
            Assert.Throws<SqlDangerousStatementExceptions>(() => SqlWhereClauseValidator.Validate("ROUND(AREA_KM2) = 1"));

            SqlWhereClauseValidator.AdditionalAllowedFunctions = ["round"];

            SqlWhereClauseValidator.Validate("ROUND(AREA_KM2) = 1", FakeFeatureClass.Default());
        }
        finally
        {
            SqlWhereClauseValidator.AdditionalAllowedFunctions = [];
        }
    }

    [Theory]
    [InlineData("pg_sleep(1)")]
    [InlineData("dbo.fn")]
    [InlineData("x;y")]
    public void AdditionalAllowedFunctions_InvalidName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => SqlWhereClauseValidator.AdditionalAllowedFunctions = [name]);
    }

    [Fact]
    public void CheckWhereClauseForSqlInjection_ReturnsUnchangedWhereClause()
    {
        const string where = "name = 'Graz' AND 1=1";

        Assert.Equal(where, where.CheckWhereClauseForSqlInjection(FakeFeatureClass.Default()));
    }
}

/// <summary>
/// AdditionalAllowedFunctions is static: tests that change it must not run in parallel with the others.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public class SqlWhereClauseValidatorCollection
{
    public const string Name = "SqlWhereClauseValidator";
}
