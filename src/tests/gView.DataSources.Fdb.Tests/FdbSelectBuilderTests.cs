using gView.DataSources.Fdb;

namespace gView.DataSources.Fdb.Tests;

public class FdbSelectBuilderTests
{
    private const string Head = "SELECT * FROM FC_x";

    // ===== shared clauses (exercised through SqliteSelectBuilder) ============

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Where_EmptyClause_AppendsNothing(string? clause)
        => Assert.Equal(Head, new SqliteSelectBuilder(Head).Where(clause!).Build());

    [Fact]
    public void Where_Clause_IsAppended()
        => Assert.Equal($"{Head} WHERE a = 1", new SqliteSelectBuilder(Head).Where("a = 1").Build());

    [Fact]
    public void WhereAnd_BothParts_AreAndCombinedWithSecondParenthesized()
        => Assert.Equal(
            $"{Head} WHERE (FDB_NID IN (1,2)) AND (name = 'x')",
            new SqliteSelectBuilder(Head).WhereAnd("(FDB_NID IN (1,2))", "name = 'x'").Build());

    [Fact]
    public void WhereAnd_OnlyFirst_UsesFirstVerbatim()
        => Assert.Equal(
            $"{Head} WHERE (FDB_NID IN (1,2))",
            new SqliteSelectBuilder(Head).WhereAnd("(FDB_NID IN (1,2))", "").Build());

    [Fact]
    public void WhereAnd_OnlySecond_UsesSecondVerbatim()
        => Assert.Equal(
            $"{Head} WHERE name = 'x'",
            new SqliteSelectBuilder(Head).WhereAnd(null!, "name = 'x'").Build());

    [Fact]
    public void WhereAnd_NeitherPart_AppendsNothing()
        => Assert.Equal(Head, new SqliteSelectBuilder(Head).WhereAnd(null!, "").Build());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OrderBy_Empty_AppendsNothing(string? clause)
        => Assert.Equal(Head, new SqliteSelectBuilder(Head).OrderBy(clause!).Build());

    [Fact]
    public void OrderBy_Clause_IsAppended()
        => Assert.Equal($"{Head} ORDER BY a, FDB_OID", new SqliteSelectBuilder(Head).OrderBy("a, FDB_OID").Build());

    [Fact]
    public void AppendRawIf_True_Appends()
        => Assert.Equal($"{Head} WITH (NOLOCK)", new SqliteSelectBuilder(Head).AppendRawIf(true, " WITH (NOLOCK)").Build());

    [Fact]
    public void AppendRawIf_False_AppendsNothing()
        => Assert.Equal(Head, new SqliteSelectBuilder(Head).AppendRawIf(false, " WITH (NOLOCK)").Build());

    // ===== SQLite paging: LIMIT n [OFFSET m], OFFSET needs a LIMIT ===========

    [Fact]
    public void Sqlite_Page_NoLimitNoOffset_AppendsNothing()
        => Assert.Equal(Head, new SqliteSelectBuilder(Head).Page(0, 1).Build());

    [Fact]
    public void Sqlite_Page_LimitOnly()
        => Assert.Equal($"{Head} LIMIT 10", new SqliteSelectBuilder(Head).Page(10, 1).Build());

    [Fact]
    public void Sqlite_Page_LimitAndOffset()
        => Assert.Equal($"{Head} LIMIT 10 OFFSET 4", new SqliteSelectBuilder(Head).Page(10, 5).Build());

    [Fact]
    public void Sqlite_Page_OffsetOnly_UsesLimitMinusOneSentinel()
        => Assert.Equal($"{Head} LIMIT -1 OFFSET 4", new SqliteSelectBuilder(Head).Page(0, 5).Build());

    // ===== PostgreSQL paging: LIMIT and OFFSET are independent ==============

    [Fact]
    public void Postgres_Page_NoLimitNoOffset_AppendsNothing()
        => Assert.Equal(Head, new PostgreSqlSelectBuilder(Head).Page(0, 1).Build());

    [Fact]
    public void Postgres_Page_LimitOnly()
        => Assert.Equal($"{Head} LIMIT 10", new PostgreSqlSelectBuilder(Head).Page(10, 1).Build());

    [Fact]
    public void Postgres_Page_LimitAndOffset()
        => Assert.Equal($"{Head} LIMIT 10 OFFSET 4", new PostgreSqlSelectBuilder(Head).Page(10, 5).Build());

    [Fact]
    public void Postgres_Page_OffsetOnly_EmitsBareOffset()
        => Assert.Equal($"{Head} OFFSET 4", new PostgreSqlSelectBuilder(Head).Page(0, 5).Build());

    // ===== SQL Server paging: OFFSET .. ROWS [FETCH ..], needs ORDER BY =====

    [Fact]
    public void SqlServer_Page_NoLimitNoOffset_AppendsNothing()
        => Assert.Equal(Head, new SqlServerSelectBuilder(Head).Page(0, 1).Build());

    [Fact]
    public void SqlServer_Page_WithExistingOrderBy_EmitsOffsetFetch()
        => Assert.Equal(
            $"{Head} ORDER BY name OFFSET 4 ROWS FETCH NEXT 10 ROWS ONLY",
            new SqlServerSelectBuilder(Head).OrderBy("name").Page(10, 5).Build());

    [Fact]
    public void SqlServer_Page_WithoutOrderBy_InjectsFdbOid()
        => Assert.Equal(
            $"{Head} ORDER BY FDB_OID OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY",
            new SqlServerSelectBuilder(Head).Page(10, 1).Build());

    [Fact]
    public void SqlServer_Page_OffsetOnly_OmitsFetch()
        => Assert.Equal(
            $"{Head} ORDER BY FDB_OID OFFSET 4 ROWS",
            new SqlServerSelectBuilder(Head).Page(0, 5).Build());

    // ===== full chains ====================================================

    [Fact]
    public void FullChain_SqliteSpatialStyle()
        => Assert.Equal(
            $"{Head} WHERE (FDB_NID BETWEEN 1 AND 9) AND (state = 1) ORDER BY name, FDB_OID LIMIT 25 OFFSET 50",
            new SqliteSelectBuilder(Head)
                .WhereAnd("(FDB_NID BETWEEN 1 AND 9)", "state = 1")
                .OrderBy("name, FDB_OID")
                .Page(25, 51)
                .Build());

    [Fact]
    public void FullChain_SqlServerSpatialStyle()
        => Assert.Equal(
            $"{Head} WHERE (FDB_NID BETWEEN 1 AND 9) AND (state = 1) ORDER BY name, FDB_OID OFFSET 50 ROWS FETCH NEXT 25 ROWS ONLY WITH (NOLOCK)",
            new SqlServerSelectBuilder(Head)
                .WhereAnd("(FDB_NID BETWEEN 1 AND 9)", "state = 1")
                .OrderBy("name, FDB_OID")
                .Page(25, 51)
                .AppendRawIf(true, " WITH (NOLOCK)")
                .Build());
}
