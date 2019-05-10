using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


    public interface IPrimaryKeys
    {
        string[] Keys { get; }
        int Length { get; }
        string ConstraintName { get; }
    }

    public interface IForeignKeys
    {
        IForeignKey[] Keys { get; }
        int Length { get; }
    }


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

    public interface IIdentityKeys
    {
        string[] ColumnNames { get; }
        int Length { get; }
    }
}
