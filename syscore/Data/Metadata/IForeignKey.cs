namespace Sys.Data
{
    public interface IForeignKey
    {
        TableName TableName { get; }

        string FK_Schema { get; }
        string FK_Table { get; }
        string FK_Column { get; }
        string PK_Schema { get; }
        string PK_Table { get; }
        string PK_Column { get; }
        string Constraint_Name { get; }
    }
}
