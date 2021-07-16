namespace Sys.Data
{
    public interface ITableSchema
    {
        TableName TableName { get; }
        int TableID { get; }

        IIdentityKeys Identity { get; }
        IPrimaryKeys PrimaryKeys { get; }
        IForeignKeys ForeignKeys { get; }

        ColumnCollection Columns { get; }
    }
}
