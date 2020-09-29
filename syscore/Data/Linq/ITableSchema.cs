namespace Sys.Data.Linq
{
    interface ITableSchema
    {
        string SchemaName { get; }
        string TableName { get; }
        string[] PrimaryKeys { get; }
        string[] IdentityKeys { get; }
    }
}