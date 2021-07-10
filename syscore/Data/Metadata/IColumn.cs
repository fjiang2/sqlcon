namespace Sys.Data
{
    public interface IColumn
    {
        string ColumnName { get; }
        string DataType { get; }
        short Length { get; }
        bool Nullable { get; }
        byte Precision { get; }
        byte Scale { get; }
        bool IsPrimary { get; }
        bool IsIdentity { get; }
        bool IsComputed { get; }
        string Definition { get; }
        int ColumnID { get; }

        IForeignKey ForeignKey { get; }
        CType CType { get; }

        void SetForeignKey(IForeignKey value);
    }
}
