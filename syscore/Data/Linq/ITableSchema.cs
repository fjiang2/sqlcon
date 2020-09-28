namespace Sys.Data.Linq
{
    interface ITableSchema
    {
        string[] IdentityKeys { get; set; }
        string[] PrimaryKeys { get; set; }
        string TableName { get; set; }
    }
}