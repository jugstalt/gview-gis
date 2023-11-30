namespace gView.Framework.Core.FDB
{
    public interface IDatabaseNames
    {
        string TableName(string tableName);
        string DbColName(string fieldName);
    }
}
