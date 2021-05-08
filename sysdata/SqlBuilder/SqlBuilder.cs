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
            return sql.Query;
        }

        public ConnectionProvider Provider => provider;

        private List<string> script = new List<string>();
        public string Query
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

        public SqlBuilder AppendSpace(string text)
        {
            script.Add(text + " ");
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
        private SqlBuilder TABLE_NAME(SqlTableName tableName, string alias)
        {
            AppendSpace(tableName.ToString());
            if (!string.IsNullOrEmpty(alias))
                AppendSpace(alias);

            if (tableName.Provider != null)
                this.provider = tableName.Provider;

            return this;
        }


        #endregion

        public SqlBuilder USE(string database)
        {
            return AppendLine($"USE {database}");
        }

        public SqlBuilder USE(DatabaseName databaseName)
        {
            return AppendLine($"USE {databaseName.Name}");
        }

        public SqlBuilder SET(string key, SqlExpr value)
        {
            return AppendLine($"SET {key} {value}");
        }

        #region SELECT clause

        public SqlBuilder SELECT() => AppendSpace("SELECT");

        public SqlBuilder DISTINCT() => AppendSpace("DISTINCT");

        public SqlBuilder ALL() => AppendSpace("ALL");

        public SqlBuilder TOP(int n)
        {
            if (n > 0)
                return AppendSpace($"TOP {n}");

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
            return AppendSpace(columns);
        }

        public SqlBuilder COLUMNS(IEnumerable<string> columns)
        {
            if (columns.Count() == 0)
                return COLUMNS("*");
            else
            {
                var L = columns.Select(column => column.ColumnName());
                return COLUMNS(string.Join(",", L));
            }
        }

        public SqlBuilder COLUMNS(params SqlExpr[] columns)
        {
            if (columns.Count() == 0)
                return COLUMNS("*");
            else
            {
                var L = columns.Select(column => column.ToString());
                return COLUMNS(string.Join(", ", L));
            }
        }

        #endregion

        public SqlBuilder INTO(SqlTableName tableName)
        {
            return Append($"INTO {tableName}");
        }

        public SqlBuilder FROM<T>(string alias = null)
        {
            return FROM(typeof(T).TableName(), alias);
        }

        public SqlBuilder FROM(SqlTableName from, string alias = null) => AppendSpace($"FROM").TABLE_NAME(from, alias);


        public SqlBuilder UPDATE<T>(string alias = null)
        {
            return UPDATE(typeof(T).TableName(), alias);
        }

        public SqlBuilder UPDATE(SqlTableName tableName, string alias = null)
        {
            return AppendSpace($"UPDATE").TABLE_NAME(tableName, alias);
        }

        public SqlBuilder SET(params SqlExpr[] assignments) => SET(string.Join<SqlExpr>(", ", assignments));

        public SqlBuilder SET(string assignments) => AppendSpace("SET").AppendSpace(assignments);

        public SqlBuilder INSERT_INTO<T>(params string[] columns)
        {
            return INSERT_INTO(typeof(T).TableName(), columns);
        }

        public SqlBuilder INSERT_INTO(SqlTableName tableName, params string[] columns)
        {
            Append($"INSERT INTO {tableName}");

            if (columns.Length > 0)
                AppendSpace($"({JoinColumns(columns)})");

            return this;
        }


        public SqlBuilder VALUES(params object[] values)
        {
            return AppendLine($"VALUES ({JoinValues(values)})");
        }

        private static string JoinValues(object[] values)
        {
            return string.Join(",", values.Select(x => new SqlValue(x).ToString()));
        }

        public SqlBuilder DELETE<T>()
        {
            return DELETE(typeof(T).TableName());
        }

        public SqlBuilder DELETE(SqlTableName tableName)
        {
            return AppendLine($"DELETE FROM {tableName}");
        }


        #region WHERE clause

        public SqlBuilder WHERE(SqlExpr exp)
        {
            AppendSpace($"WHERE {exp}");
            this.Merge(exp);
            return this;
        }

        public SqlBuilder WHERE(Locator locator)
        {
            if (locator == null || locator.IsEmpty)
                return this;

            return WHERE(locator.Where);
        }

        public SqlBuilder WHERE(string exp)
        {
            return AppendSpace($"WHERE {exp}");
        }

        public SqlBuilder WHERE(byte[] loc)
        {
            return AppendLine($"WHERE {SqlExpr.PHYSLOC} = {new SqlValue(loc)}");
        }

        #endregion


        #region INNER/OUT JOIN clause

        public SqlBuilder LEFT() => AppendSpace("LEFT");

        public SqlBuilder RIGHT() => AppendSpace("RIGHT");

        public SqlBuilder INNER() => AppendSpace("INNER");

        public SqlBuilder OUTTER() => AppendSpace("OUTTER");

        public SqlBuilder JOIN<T>(string alias = null) => JOIN(typeof(T).TableName(), alias);

        public SqlBuilder JOIN(SqlTableName tableName, string alias = null) => AppendSpace("JOIN").TABLE_NAME(tableName, alias);

        public SqlBuilder ON(SqlExpr exp)
        {
            AppendSpace($"ON {exp}");
            this.Merge(exp);
            return this;
        }

        #endregion



        #region GROUP BY / HAVING clause
        public SqlBuilder GROUP_BY(params string[] columns)
        {
            return AppendSpace($"GROUP BY {JoinColumns(columns)}");
        }

        public SqlBuilder HAVING(SqlExpr expr)
        {
            return AppendSpace($"HAVING {expr}");
        }

        #endregion



        public SqlBuilder ORDER_BY(params string[] columns)
        {
            if (columns == null || columns.Length == 0)
                return this;

            return AppendLine($"ORDER BY {JoinColumns(columns)} ");
        }


        public SqlBuilder UNION() => AppendSpace("UNION");
        public SqlBuilder DESC() => AppendSpace("DESC");

        private static string JoinColumns(IEnumerable<string> columns)
        {
            return string.Join(",", columns.Select(x => $"[{x}]"));
        }

        /// <summary>
        /// concatenate 2 clauses in TWO lines
        /// </summary>
        /// <param name="clause1"></param>
        /// <param name="clause2"></param>
        /// <returns></returns>
        public static SqlBuilder operator +(SqlBuilder clause1, SqlBuilder clause2)
        {
            var builder = new SqlBuilder();

            builder.Append(clause1.Query)
                .AppendLine()
                .Append(clause2.Query);
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

            builder.Append(clause1.Query)
                .Append(" ")
                .Append(clause2.Query);
            return builder;
        }



        public override string ToString() => Query;

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
