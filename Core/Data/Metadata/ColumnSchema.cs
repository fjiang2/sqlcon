//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Sys.Data
{
    


    public class ColumnSchema : PersistentObject, IColumn
    {

        [Column("ColumnName", CType.NVarChar, Primary = true)]
        public string ColumnName { get; set; }

        [Column("DataType", CType.NVarChar)]
        public string DataType { get; set; }

        [Column("Length", CType.Int)]
        public short Length { get; set; }    //length return from SQL Server

        [Column("Nullable", CType.Bit)]
        public bool Nullable { get; set; }

        [Column("precision", CType.TinyInt)]
        public byte Precision { get; set; }

        [Column("scale", CType.TinyInt)]
        public byte Scale { get; set; }

        [Column("IsPrimary", CType.Bit)]
        public bool IsPrimary { get; set; }

        [Column("IsIdentity", CType.Bit)]
        public bool IsIdentity { get; set; }

        [Column("IsComputed", CType.Bit)]
        public bool IsComputed { get; set; }

        [Column("definition", CType.NVarChar)]
        public string Definition { get; set; }

        [Column("PKContraintName", CType.NVarChar)]
        public string PkContraintName { get; set; }

        [Column("PK_Schema", CType.NVarChar)]
        public string PK_Schema { get; set; }

        [Column("PK_Table", CType.NVarChar)]
        public string PK_Table { get; set; }

        [Column("PK_Column", CType.NVarChar)]
        public string PK_Column { get; set; }

        [Column("FKContraintName", CType.NVarChar)]
        public string FkContraintName { get; set; }

        [Column("ColumnID", CType.Int)]
        public int ColumnID { get; set; }    //column_id is from column dictionary
        
        [Column("label", CType.NVarChar)]
        public string label { get; set; }    //label used as caption to support internationalization


        private CType ctype;
        private IForeignKey foreignKey;

        public ColumnSchema(DataRow dataRow)
            : base(dataRow)
        {
            this.ctype = GetCType(this.DataType);
        }

        internal ColumnSchema(ColumnAttribute attr)
        {

            this.ColumnName = attr.ColumnName;
            this.CType = attr.CType;

            this.Nullable = attr.Nullable;
            this.Precision = attr.Precision;
            this.Scale = attr.Scale;
            this.IsIdentity = attr.Identity;
            this.IsComputed = attr.Computed;
            this.IsPrimary = attr.Primary;

            this.ColumnID = -1; //useless here
            this.label = attr.Caption;
        }

        public string Caption
        {
            get
            {
                if (string.IsNullOrEmpty(this.label))
                    return ColumnName;
                else
                    return this.label;
            }
        }

        public CType CType
        {
            get { return this.ctype; }
            set
            {
                this.ctype = value;
                this.DataType = value.ToString();
            }
        }

       

      

        public IForeignKey ForeignKey
        {
            get { return this.foreignKey; }
            set { this.foreignKey = value; }
        }

        public override int GetHashCode()
        {
            return this.ColumnName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ColumnSchema it = obj as ColumnSchema;
            
            if (it == null)
                return false;

            return this.ColumnName.Equals(it.ColumnName)
            && this.CType.Equals(it.CType)
            && this.Nullable.Equals(it.Nullable)
            && this.Precision.Equals(it.Precision)
            && this.Scale.Equals(it.Scale)
            && this.IsIdentity.Equals(it.IsIdentity)
            && this.IsComputed.Equals(it.IsComputed)
            && this.IsPrimary.Equals(it.IsPrimary);
        }

        public override string ToString()
        {
            return string.Format("Column={0}(Type={1}, Null={2}, Length={3})", ColumnName, DataType, Nullable, Length);
        }

     

       


        #region SqlDataType -> System.SqlDbType / C# field type / SQL_Create_Table Type

        public static CType GetCType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "varchar":
                    return CType.VarChar;

                case "char":
                    return CType.Char;
         
                case "nvarchar":
                    return CType.NVarChar;

                case "nchar":
                    return CType.NChar;

                case "decimal":
                    return CType.Decimal;

                case "numeric":
                    return CType.Decimal;

                case "text":
                    return CType.Text;

                case "ntext":
                    return CType.NText;

                case "datetime":
                    return CType.DateTime;

                case "smalldatetime":
                    return CType.SmallDateTime;

                case "timestamp":
                    return CType.Timestamp;

                case "bit":
                    return CType.Bit;

                case "money":
                    return CType.Money;

                case "smallmoney":
                    return CType.SmallMoney;

                case "real":
                    return CType.Real;

                case "float":
                    return CType.Float;

                case "tinyint":
                    return CType.TinyInt;

                case "smallint":
                    return CType.SmallInt;

                case "int":
                    return CType.Int;

                case "bigint":
                    return CType.BigInt;

                case "varbinary":
                    return CType.VarBinary;

                case "binary":
                    return CType.Binary;


                case "image":
                    return CType.Image;

                case "uniqueidentifier":
                    return CType.UniqueIdentifier;

                case "hierarchyid":
                    return CType.HierarchyId;

                case "geometry":
                    return CType.Geometry;

                case "geography":
                    return CType.Geography;

                case "date":
                    return CType.Date;

                case "time":
                    return CType.Time;

                case "xml":
                    return CType.Xml;

            }

            throw new MessageException("data type [{0}] is not supported", sqlType);
        }



        public static string GetFieldType(string sqlType, bool nullable)
        {
            string ty = "";
            switch (sqlType.ToLower())
            {
                case "varchar":
                case "char":
                case "text":
                case "nvarchar":
                case "nchar":
                case "ntext":
                    ty = "string";
                    break;

                case "date":
                case "datetime":
                case "smalldatetime":
                    ty = "DateTime";
                    if (nullable) ty += "?";
                    break;

                case "time":
                case "timestamp":
                    ty = "TimeSpan";
                    if (nullable) ty += "?";
                    break;

                case "bit":
                    ty = "bool";
                    if (nullable) ty += "?";
                    break;

                case "money":
                case "smallmoney":
                case "decimal":
                case "numeric":
                    ty = "decimal";
                    if (nullable) ty += "?";
                    break;

                case "real":
                    ty = "Single";
                    if (nullable) ty += "?";
                    break;

                case "float":
                    ty = "double";
                    if (nullable) ty += "?";
                    break;

                case "tinyint":
                    ty = "byte";
                    if (nullable) ty += "?";
                    break;

                case "smallint":
                    ty = "short";
                    if (nullable) ty += "?";
                    break;

                case "int":
                    ty = "int";
                    if (nullable) ty += "?";
                    break;

                case "bigint":
                    ty = "long";
                    if (nullable) ty += "?";
                    break;


                case "varbinary":
                case "binary":
                case "image":
                    ty = "byte[]";
                    break;

                case "uniqueidentifier":
                    ty = "Guid";
                    break;

                case "sql_variant":
                    ty = "object";
                    break;

                case "geography":
                    ty = "SqlGeography";
                    break;

                case "geometry":
                    ty = "SqlGeometry";
                    break;

                case "hierarchyid":
                    ty = "SqlHierarchyId";
                    break;

                default:
                    ty = sqlType;
                    break;
            }
            return ty;
        }


       
        

        public static string GetSQLField(IColumn column)
        {
            string ty = GetSQLType(column);

            string line = string.Format("[{0}] {1} {2}", column.ColumnName, ty, column.Nullable ? "NULL" : "NOT NULL");

            if (column.IsIdentity)
            {
                line += " IDENTITY(1,1)";
                return line;
            }

            if (column.IsComputed)
            {
                line = string.Format("[{0}] AS {1}", column.ColumnName, column.Definition);
                //throw new JException("not support computed column: {0}", column.ColumnName);
            }
                
            return line;
        }

        public static string GetSQLType(IColumn column)
        {
            string ty = "";
            string DataType = column.DataType;
            int Length = column.Length;

            switch (column.CType)
            {
                case CType.VarChar:
                case CType.Char:
                case CType.VarBinary:
                case CType.Binary:
                    if (Length >= 0)
                        ty = string.Format("{0}({1})", DataType, Length);
                    else
                        ty = string.Format("{0}(max)", DataType);
                    break;

                case CType.NVarChar:
                case CType.NChar:
                    if (Length >= 0)
                        ty = string.Format("{0}({1})", DataType, Length / 2);
                    else
                        ty = string.Format("{0}(max)", DataType);
                    break;

                //case SqlDbType.Numeric:
                case CType.Decimal:
                    ty = string.Format("{0}({1},{2})", DataType, column.Precision, column.Scale);
                    break;


                default:
                    ty = DataType;
                    break;
            }
            return ty;
        }
    
        #endregion


        public object ToObject(string val)
        {
            if (this.Nullable && val == "")
                return null;

            switch (ctype)
            {
                case CType.VarChar:
                case CType.NVarChar:
                case CType.Char:
                case CType.NChar:
                    if (this.Oversize(val))
                        throw new MessageException("Column Name={0}, length of value \"{1}\" {2} > {3}", ColumnName, val, val.Length, this.AdjuestedLength());
                    else
                        return val;
           
                case CType.VarBinary:
                case CType.Binary:
                    throw new NotImplementedException(string.Format("cannot convert {0} into type of {1}", val, CType.Binary));
                    

                case CType.Date:
                case CType.DateTime:
                    if(val.IndexOf("-") > 0)    //2011-10-30
                    {
                        string[] date = val.Split('-');
                        return new DateTime(Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2]));
                    }
                    else if(val.Length == 8)    //20111030
                    {
                        int month = Convert.ToInt32(val.Substring(4, 2));
                        int day = Convert.ToInt32(val.Substring(6, 2));
                        if(month==0)
                           month =1;
                        
                        if(day ==0)
                            day = 1;

                        return new DateTime(Convert.ToInt32(val.Substring(0, 4)), month, day);
                    }
                    else 
                    {
                        return Convert.ToDateTime(val);
                    }

                case CType.Time:
                    {
                        string[] time = val.Split(':');
                        return new TimeSpan(Convert.ToInt32(time[0]), Convert.ToInt32(time[1]), Convert.ToInt32(time[2]));
                    }

                case CType.Float:
                    return Convert.ToDouble(val);

                case CType.Real:
                    return Convert.ToSingle(val);


                case CType.Bit:
                    if (val == "0")
                        return false;
                    else if (val == "1")
                        return true;
                    else 
                        return Convert.ToBoolean(val);

                case CType.Decimal:
                    return Convert.ToDecimal(val);

                case CType.TinyInt:
                    return Convert.ToByte(val);

                case CType.SmallInt:
                    return Convert.ToInt16(val);

                case CType.Int:
                    return Convert.ToInt32(val);
                
                case CType.BigInt:
                    return Convert.ToInt64(val);

//                case CType.Xml:

                default:
                    throw new NotImplementedException(string.Format("cannot convert {0} into type of {1}", val, ctype));
            }

        }
    }
}
