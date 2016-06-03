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
        private StringBuilder script = new StringBuilder();
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

        public ConnectionProvider Provider
        {
            get
            {
                return provider;
            }
            set
            {
                this.provider = value;
            }
        }


        public SqlBuilder Append(string text)
        {
            script.Append(text);
            return this;
        }
        public SqlBuilder AppendFormat(string format, params object[] args)
        {
            script.AppendFormat(format, args);
            return this;
        }

        #region Table Name
        private SqlBuilder TABLE_NAME(string tableName, string alias)
        {
            script.Append(tableName);
            if (alias != null)
                script.Append(" ").Append(alias);

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
            script.Append("USE ").AppendLine(database);
            return this;
        }

        public SqlBuilder USE(DatabaseName databaseName)
        {
            script.Append("USE ").AppendLine(databaseName.Name);
            return this;
        }

        public SqlBuilder SET(string key, SqlExpr value)
        {

            script.AppendFormat("SET {0} {1}", key, value)
                .AppendLine();
            return this;
        }

        #region SELECT clause

        public SqlBuilder SELECT
        {
            get
            {
                script.Append("SELECT ");
                return this;
            }
        }

        public SqlBuilder DISTINCT
        {
            get
            {
                script.Append("DISTINCT ");
                return this;
            }
        }

        public SqlBuilder ALL
        {
            get
            {
                script.Append("ALL ");
                return this;
            }
        }


        public SqlBuilder TOP(int n)
        {
            if (n > 0)
                script.Append("TOP ").Append(n).Append(" ");

            return this;
        }

        public SqlBuilder ROWID(bool has)
        {
            if (has)
                script
                    .Append(string.Format("{0} AS [{0}],", SqlExpr.PHYSLOC))
                    .Append(string.Format("0 AS [{0}],", SqlExpr.ROWID));

            return this;
        }


        public SqlBuilder COLUMNS(string columns)
        {
            script.Append(columns).Append(" ");
            return this;
        }

        public SqlBuilder COLUMNS(string[] columns)
        {
            if (columns.Length == 0)
                script.Append("* ");
            else
            {
                var L = columns.Select(column => column.ColumnName());

                script.Append(string.Join(",", L)).Append(" ");
            }

            return this;
        }


        public SqlBuilder COLUMNS(params SqlExpr[] columns)
        {
            if (columns.Length == 0)
                script.Append("* ");
            else
                script.Append(string.Join(",", columns.Select(column => column.ToString()))).Append(" ");

            return this;
        }


        public SqlBuilder INTO(string tableName)
        {
            script
                .Append("INTO ")
                .Append(tableName);

            return this;
        }

        public SqlBuilder INTO(TableName tableName)
        {
            script
                .Append("INTO ")
                .Append(tableName.FullName);

            return this;
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
            script.Append("FROM ").Append(from).Append(" ");
            if (alias != null)
                script.Append(alias).Append(" ");

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
            script.Append("UPDATE ").Append(tableName).Append(" ");
            if (alias != null)
                script.Append(alias).Append(" ");

            return this;
        }


        public SqlBuilder SET(params string[] assignments)
        {
            script.Append("SET ").Append(string.Join(", ", assignments));

            return this.CRLF;
        }



        public SqlBuilder SET(params SqlExpr[] assignments)
        {
            script.Append("SET ");
            string s = string.Join<SqlExpr>(", ", assignments);
            script.Append(s);

            return this.CRLF;
        }


        public SqlBuilder SET(string assignments)
        {
            script.Append("SET ")
               .Append(assignments)
               .Append(" ");

            return this.CRLF;
        }

        public SqlBuilder INSERT<T>(params string[] columns)
        {
            return INSERT(typeof(T).TableName(), columns);
        }

        public SqlBuilder INSERT(TableName tableName, params string[] columns)
        {
            this.provider = tableName.Provider;
            script
                .Append("INSERT INTO ")
                .Append(tableName);

            if (columns.Length > 0)
                script.Append("(").Append(ConcatColumns(columns)).Append(") ");


            return this;
        }


        public SqlBuilder VALUES(params object[] values)
        {
            script
                .Append("VALUES ")
                .Append("(").Append(ConcatValues(values)).Append(") ");

            return this.CRLF;
        }

        public SqlBuilder DELETE(TableName tableName)
        {
            this.provider = tableName.Provider;

            script.Append("DELETE FROM ").Append(tableName).Append(" ");

            return this;
        }

        public SqlBuilder DELETE<T>()
        {
            return DELETE(typeof(T).TableName());
        }

        #region WHERE clause

        public SqlBuilder WHERE(SqlExpr exp)
        {
            script.Append("WHERE ").Append(exp).Append(" ");
            this.Merge(exp);
            return this.CRLF;
        }

        public SqlBuilder WHERE(Locator locator)
        {
            script.Append("WHERE ").Append(locator).Append(" ");
            return this.CRLF;
        }

        public SqlBuilder WHERE(string exp)
        {
            script.Append("WHERE ").Append(exp).Append(" ");
            return this.CRLF;
        }

        public SqlBuilder WHERE(byte[] loc)
        {
            script.Append("WHERE ").Append(SqlExpr.PHYSLOC).Append(" = ").Append(new SqlValue(loc));
            return this.CRLF;
        }

        #endregion


        #region INNER/OUT JOIN clause

        public SqlBuilder LEFT
        {
            get
            {
                script.Append("LEFT ");
                return this;
            }
        }

        public SqlBuilder RIGHT
        {
            get
            {
                script.Append("RIGHT ");
                return this;
            }
        }

        public SqlBuilder INNER
        {
            get
            {
                script.Append("INNER ");
                return this;
            }
        }

        public SqlBuilder OUTTER
        {
            get
            {
                script.Append("OUTTER ");
                return this;
            }
        }




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
            script
                .Append("JOIN ")
                .Append(tableName)
                .Append(" ");

            if (alias != null)
                script.Append(alias).Append(" ");

            return this;
        }

        public SqlBuilder ON(SqlExpr exp)
        {
            script
                .Append("ON ")
                .Append(exp)
                .Append(" ");

            this.Merge(exp);
            return this;
        }

        #endregion



        #region GROUP BY / HAVING clause
        public SqlBuilder GROUP_BY(params string[] columns)
        {
            script.Append("GROUP BY ").Append(ConcatColumns(columns)).Append(" ");
            return this;
        }

        public SqlBuilder HAVING(SqlExpr expr)
        {
            script.Append("HAVING ").Append(expr).Append(" ");
            return this;
        }

        #endregion



        public SqlBuilder ORDER_BY(params string[] columns)
        {
            if (columns == null)
                return this;

            script.Append("ORDER BY ").Append(ConcatColumns(columns)).Append(" ");
            return this;
        }


        public SqlBuilder UNION
        {
            get
            {
                script.Append("UNION ");
                return this;
            }
        }


        public SqlBuilder DESC
        {
            get
            {
                script.Append("DESC ");
                return this;
            }
        }




        private int tab = 0;
        private SqlBuilder TAB(int n)
        {
            tab += n;
            for (int i = 0; i < tab; i++)
                script.Append("\t");

            return this;
        }


        private SqlBuilder CRLF
        {
            get
            {
                script.AppendLine();
                return this;
            }
        }

        private SqlBuilder PAR(string exp)
        {
            script.Append("(").Append(exp).Append(")");
            return this;
        }

        private SqlBuilder SPACE
        {
            get
            {
                script.Append(" ");
                return this;
            }
        }


        #region Concatenate



        private static string ConcatColumns(string[] columns)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                if (i != 0)
                    sb.Append(",");

                sb.Append("[").Append(columns[i]).Append("]");
            }

            return sb.ToString();
        }


        private static string ConcatValues(object[] S)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < S.Length; i++)
            {
                if (i != 0)
                    sb.Append(",");

                sb.Append(new SqlValue(S[i]));

            }

            return sb.ToString();
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
            var clause = new SqlBuilder();

            clause.script
                .Append(clause1)
                .AppendLine()
                .Append(clause2);
            return clause;
        }

        /// <summary>
        /// concatenate 2 clauses in one line
        /// </summary>
        /// <param name="clause1"></param>
        /// <param name="clause2"></param>
        /// <returns></returns>
        public static SqlBuilder operator -(SqlBuilder clause1, SqlBuilder clause2)
        {
            var clause = new SqlBuilder();

            clause.script
                .Append(clause1)
                .Append(" ")
                .Append(clause2);
            return clause;
        }


        public string Clause
        {
            get
            {
                return script.ToString();
            }
        }


        public override string ToString()
        {
            return script.ToString();
        }

        public bool Invalid()
        {
            bool result = false;

            SqlCmd.Error += (sender, e) =>
            {
                result = true;
            };

            SqlCmd.ExecuteScalar();

            return result;
        }

        public SqlCmd SqlCmd
        {
            get
            {
                SqlCmd cmd = new SqlCmd(this);
                return cmd;
            }
        }

    }
  
}
