using gView.DataSources.Fdb;

namespace gView.DataSources.Fdb.Tests;

public class FdbNidWhereClauseTests
{
    // ---- Build ------------------------------------------------------------

    [Fact]
    public void Build_EmptyList_ReturnsFalsePredicate()
    {
        Assert.Equal("(1=0)", FdbNidWhereClause.Build(new List<long>(), "FDB_NID"));
    }

    [Fact]
    public void Build_SingleNode_ReturnsInWithOneValue()
    {
        Assert.Equal("(FDB_NID IN (7))", FdbNidWhereClause.Build(new List<long> { 7 }, "FDB_NID"));
    }

    [Fact]
    public void Build_ScatteredSingletons_ReturnsSingleInList()
    {
        Assert.Equal(
            "(FDB_NID IN (1,4,9))",
            FdbNidWhereClause.Build(new List<long> { 1, 4, 9 }, "FDB_NID"));
    }

    [Fact]
    public void Build_ConsecutiveSingletons_CoalesceIntoBetween()
    {
        // 1,2,3,4 -> one BETWEEN run
        Assert.Equal(
            "(FDB_NID BETWEEN 1 AND 4)",
            FdbNidWhereClause.Build(new List<long> { 1, 2, 3, 4 }, "FDB_NID"));
    }

    [Fact]
    public void Build_RangePair_ReturnsBetween()
    {
        // -20 followed by its upper bound 40 -> BETWEEN 20 AND 40
        Assert.Equal(
            "(FDB_NID BETWEEN 20 AND 40)",
            FdbNidWhereClause.Build(new List<long> { -20, 40 }, "FDB_NID"));
    }

    [Fact]
    public void Build_MixedSingletonsAndRanges_CombinesWithOr()
    {
        // 1 ; [-20,40] ; 55 ; [-70,90]
        Assert.Equal(
            "(FDB_NID IN (1,55) OR FDB_NID BETWEEN 20 AND 40 OR FDB_NID BETWEEN 70 AND 90)",
            FdbNidWhereClause.Build(new List<long> { 1, -20, 40, 55, -70, 90 }, "FDB_NID"));
    }

    [Fact]
    public void Build_OverlappingAndAdjacentRanges_AreMerged()
    {
        // [10,20], [21,30] adjacent -> [10,30]; [25,40] overlaps -> [10,40]
        Assert.Equal(
            "(FDB_NID BETWEEN 10 AND 40)",
            FdbNidWhereClause.Build(new List<long> { -10, 20, -21, 30, -25, 40 }, "FDB_NID"));
    }

    [Fact]
    public void Build_UnsortedInput_IsSortedBeforeBuilding()
    {
        Assert.Equal(
            "(FDB_NID IN (2,5,8))",
            FdbNidWhereClause.Build(new List<long> { 8, 2, 5 }, "FDB_NID"));
    }

    [Fact]
    public void Build_TwoValueRun_StaysInListNotBetween()
    {
        // span of 2 values -> IN, not BETWEEN (BETWEEN reserved for runs of >= 3)
        Assert.Equal(
            "(FDB_NID IN (3,4))",
            FdbNidWhereClause.Build(new List<long> { 3, 4 }, "FDB_NID"));
    }

    [Fact]
    public void Build_QuotedColumn_IsUsedVerbatim()
    {
        Assert.Equal(
            "(\"FDB_NID\" IN (1,2))",
            FdbNidWhereClause.Build(new List<long> { 1, 2 }, "\"FDB_NID\""));
    }

    [Fact]
    public void Build_NullColumn_Throws()
    {
        Assert.Throws<ArgumentException>(() => FdbNidWhereClause.Build(new List<long> { 1 }, ""));
    }

    // ---- OrderByWithOidTiebreaker ---------------------------------------------

    [Theory]
    [InlineData("", "FDB_OID")]
    [InlineData("   ", "FDB_OID")]
    [InlineData("NAME", "NAME, FDB_OID")]
    [InlineData("NAME DESC", "NAME DESC, FDB_OID")]
    [InlineData("A, B DESC", "A, B DESC, FDB_OID")]
    public void OrderByWithOidTiebreaker_AppendsOidWhenAbsent(string input, string expected)
    {
        Assert.Equal(expected, FdbNidWhereClause.OrderByWithOidTiebreaker(input, "FDB_OID"));
    }

    [Theory]
    [InlineData("FDB_OID")]
    [InlineData("FDB_OID DESC")]
    [InlineData("NAME, FDB_OID")]
    [InlineData("oid")]
    [InlineData("t.OID ASC")]
    public void OrderByWithOidTiebreaker_LeavesUnchangedWhenOidAlreadyReferenced(string input)
    {
        Assert.Equal(input, FdbNidWhereClause.OrderByWithOidTiebreaker(input, "FDB_OID"));
    }
}
