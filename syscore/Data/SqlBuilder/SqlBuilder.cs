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
    /// <summary>
    /// SQL clauses builder
    /// </summary>
    public sealed class SqlBuilder : SqlBuilderInfo, ISqlBuilder
    {
        private ConnectionProvider provider;

        public SqlBuilder()
        {
            this.provider = ConnectionProviderManager.DefaultProvider;
        }

        public SqlBuilder(ConnectionProvider privider)
        {
            this.provider = privider;
        }

        public static explicit operator string(SqlBuilder sql)
        {
            return sql.Clause;
        }

        public ConnectionProvider Provider => provider;

        private List<string> script = new List<string>();
        public string Clause
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (string item in script)
                    builder.Append(item);

                return builder.ToString();
            }
        }

        public SqlBuilder Append(string text)
        {
            script.Add(text);
            return this;
        }

        public SqlBuilder AppendLine(string value)
        {
            return Append(value).AppendLine();
        }

        public SqlBuilder AppendLine()
        {
            return Append(Environment.NewLine);
        }

        #region Table Name
        private SqlBuilder TABLE_NAME(string tableName, string alias)
        {
            Append(tableName);
            if (alias != null)
                Append($" {alias}");

            return this;
        }

        public SqlBuilder TABLE_NAME(TableName tableName, string alias = null)
        {
            return TABLE_NAME(tableName.FullName, alias);
        }


        public SqlBuilder TABLE_NAME(DPObject dpo, string alias = null)
        {
            return TABLE_NAME(dpo.TableName, alias);
        }

        public SqlBuilder TABLE_NAME(Type dpoType, string alias = null)
        {
            return TABLE_NAME(dpoType.TableName(), alias);
        }

        public SqlBuilder TABLE<T>(string alias = null)
        {
            return TABLE_NAME(typeof(T).TableName(), alias);
        }



        #endregion

        public SqlBuilder USE(string database)
        {
            return Append($"USE {database}").AppendLine();
        }

        public SqlBuilder USE(DatabaseName databaseName)
        {
            return Append($"USE {databaseName.Name}").AppendLine();
        }

        public SqlBuilder SET(string key, SqlExpr value)
        {
            return AppendLine($"SET {key} {value}");
        }

        #region SELECT clause

        public SqlBuilder SELECT => Append("SELECT ");

        public SqlBuilder DISTINCT => Append("DISTINCT ");

        public SqlBuilder ALL => Append("ALL ");

        public SqlBuilder TOP(int n)
        {
            if (n > 0)
                return Append($"TOP {n} ");

            return this;
        }

        public SqlBuilder ROWID(bool has)
        {
            if (has)
            {
                return Append($"{SqlExpr.PHYSLOC} AS [{SqlExpr.PHYSLOC}],")
                       .Append($"0 AS [{SqlExpr.ROWID}],");
            }

            return this;
        }


        public SqlBuilder COLUMNS(string columns)
        {
            return Append(columns).Append(" ");
        }

        public SqlBuilder COLUMNS(string[] columns)
        {
            if (columns.Length == 0)
                return Append("* ");
            else
            {
                var L = columns.Select(column => column.ColumnName());
                return Append(string.Join(",", L)).Append(" ");
            }
        }


        public SqlBuilder COLUMNS(params SqlExpr[] columns)
        {
            if (columns.Length == 0)
                return Append("* ");
            else
                return Append(string.Join(",", columns.Select(column => column.ToString()))).Append(" ");
        }


        public SqlBuilder INTO(string tableName)
        {
            return Append($"INTO {tableName}");
        }

        public SqlBuilder INTO(TableName tableName)
        {
            return Append($"INTO {tableName.FullName}");
        }

        #endregion


        #region FROM clause

        public SqlBuilder FROM(DPObject dpo, string alias = null)
        {
            return FROM(dpo.TableName, alias);
        }

        public SqlBuilder FROM(Type dpoType, string alias = null)
        {
            return FROM(dpoType.TableName(), alias);
        }


        public SqlBuilder FROM<T>(string alias = null)
        {
            return FROM(typeof(T).TableName(), alias);
        }


        public SqlBuilder FROM(TableName tableName, string alias = null)
        {
            this.provider = tableName.Provider;
            return FROM(tableName.FullName, alias);
        }

        private SqlBuilder FROM(string from, string alias)
        {
            Append($"FROM {from} ");
            if (alias != null)
                return Append($"{alias} ");

            return this;
        }

        #endregion



        public SqlBuilder UPDATE(DPObject dpo, string alias = null)
        {
            return UPDATE(dpo.TableName, alias);
        }

        public SqlBuilder UPDATE<T>(string alias = null)
        {
            return UPDATE(typeof(T).TableName(), alias);
        }

        public SqlBuilder UPDATE(Type dpoType, string alias = null)
        {
            return UPDATE(dpoType.TableName(), alias);
        }

        public SqlBuilder UPDATE(TableName tableName, string alias = null)
        {
            this.provider = tableName.Provider;
            return UPDATE(tableName.FullName, alias);
        }


        private SqlBuilder UPDATE(string tableName, string alias)
        {
            Append($"UPDATE {tableName} ");
            if (alias != null)
                Append($"{alias} ");

            return this;
        }


        public SqlBuilder SET(params string[] assignments)
        {
            return Append("SET ").Append(string.Join(", ", assignments)).AppendLine();
        }



        public SqlBuilder SET(params SqlExpr[] assignments)
        {
            Append("SET ");
            string s = string.Join<SqlExpr>(", ", assignments);
            return AppendLine(s);
        }


        public SqlBuilder SET(string assignments)
        {
            return Append("SET ")
               .Append(assignments)
               .AppendLine(" ");
        }

        public SqlBuilder INSERT<T>(params string[] columns)
        {
            return INSERT(typeof(T).TableName(), columns);
        }

        public SqlBuilder INSERT(TableName tableName, params string[] columns)
        {
            this.provider = tableName.Provider;
            Append($"INSERT INTO {tableName.FullName}");

            if (columns.Length > 0)
                Append($"({ConcatColumns(columns)}) ");


            return this;
        }


        public SqlBuilder VALUES(params object[] values)
        {
            return Append($"VALUES ({ConcatValues(values)}) ").AppendLine();
        }

        public SqlBuilder DELETE(TableName tableName)
        {
            this.provider = tableName.Provider;

            return Append($"DELETE FROM {tableName.FullName}").AppendLine();
        }

        public SqlBuilder DELETE<T>()
        {
            return DELETE(typeof(T).TableName());
        }

        #region WHERE clause

        public SqlBuilder WHERE(SqlExpr exp)
        {
            Append($"WHERE {exp} ");
            this.Merge(exp);
            return AppendLine();
        }

        public SqlBuilder WHERE(Locator locator)
        {
            return Append($"WHERE {locator.Where} ");
        }

        public SqlBuilder WHERE(string exp)
        {
            return Append($"WHERE {exp} ");
        }

        public SqlBuilder WHERE(byte[] loc)
        {
            return Append($"WHERE {SqlExpr.PHYSLOC} = {new SqlValue(loc)}").AppendLine();
        }

        #endregion


        #region INNER/OUT JOIN clause

        public SqlBuilder LEFT => Append("LEFT ");

        public SqlBuilder RIGHT => Append("RIGHT ");

        public SqlBuilder INNER => Append("INNER ");

        public SqlBuilder OUTTER => Append("OUTTER ");


        public SqlBuilder JOIN(DPObject dpo, string alias = null)
        {
            return JOIN(dpo.TableName, alias);
        }

        public SqlBuilder JOIN<T>(string alias = null)
        {
            return JOIN(typeof(T).TableName(), alias);
        }

        public SqlBuilder JOIN(Type dpoType, string alias = null)
        {
            return JOIN(dpoType.TableName(), alias);
        }

        public SqlBuilder JOIN(TableName tableName, string alias = null)
        {
            return JOIN(tableName.FullName, alias);
        }

        private SqlBuilder JOIN(string tableName, string alias)
        {
            Append($"JOIN {tableName} ");

            if (alias != null)
                Append($"{alias} ");

            return this;
        }

        public SqlBuilder ON(SqlExpr exp)
        {
            Append($"ON {exp} ");
            this.Merge(exp);
            return this;
        }

        #endregion



        #region GROUP BY / HAVING clause
        public SqlBuilder GROUP_BY(params string[] columns)
        {
            return Append($"GROUP BY {ConcatColumns(columns)} ");
        }

        public SqlBuilder HAVING(SqlExpr expr)
        {
            return Append($"HAVING {expr} ");
        }

        #endregion



        public SqlBuilder ORDER_BY(params string[] columns)
        {
            if (columns == null || columns.Length == 0)
                return this;

            return Append($"ORDER BY {ConcatColumns(columns)} ");
        }


        public SqlBuilder UNION => Append("UNION ");


        public SqlBuilder DESC => Append("DESC ");


        private int tab = 0;
        private SqlBuilder TAB(int n)
        {
            tab += n;
            return Append(Enumerable.Repeat("\t", tab).Aggregate((x, y) => x + y));
        }

        private SqlBuilder PAR(string exp)
        {
            return Append($"({exp})");
        }

        private SqlBuilder SPACE => Append(" ");


        #region Concatenate



        private static string ConcatColumns(IEnumerable<string> columns)
        {
            return string.Join(",", columns.Select(x => $"[{x}]"));
        }


        private static string ConcatValues(object[] values)
        {
            return string.Join(",", values.Select(x => new SqlValue(x).ToString()));
        }

        #endregion

        /// <summary>
        /// concatenate 2 clauses in TWO lines
        /// </summary>
        /// <param name="clause1"></param>
        /// <param name="clause2"></param>
        /// <returns></returns>
        public static SqlBuilder operator +(SqlBuilder clause1, SqlBuilder clause2)
        {
            var builder = new SqlBuilder();

            builder.Append(clause1.Clause)
                .AppendLine()
                .Append(clause2.Clause);
            return builder;
        }

        /// <summary>
        /// concatenate 2 clauses in one line
        /// </summary>
        /// <param name="clause1"></param>
        /// <param name="clause2"></param>
        /// <returns></returns>
        public static SqlBuilder operator -(SqlBuilder clause1, SqlBuilder clause2)
        {
            var builder = new SqlBuilder();

            builder.Append(clause1.Clause)
                .Append(" ")
                .Append(clause2.Clause);
            return builder;
        }



        public override string ToString() => Clause;

        public bool Invalid()
        {
            bool result = false;

            SqlCmd.Error += (sender, e) =>
            {
                result = true;
            };

            try
            {
                SqlCmd.ExecuteScalar();

                return result;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public SqlCmd SqlCmd => new SqlCmd(this);

    }

}
