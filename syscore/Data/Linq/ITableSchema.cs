namespace Sys.Data.Linq
{
    interface ITableSchema
    {
        string[] IdentityKeys { get; }
        string[] PrimaryKeys { get; }
        string SchemaName { get; }
        string TableName { get; }
    }
}