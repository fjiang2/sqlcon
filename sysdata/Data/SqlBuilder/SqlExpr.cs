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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Data
{
    public sealed class SqlExpr : SqlBuilderInfo
    {
        public static readonly SqlExpr COUNT = new SqlExpr().Append("COUNT(*)");

        internal const string PHYSLOC = "%%physloc%%";
        internal const string ROWID = "%%RowId%%";

        private StringBuilder script = new StringBuilder();

        private SqlExpr()
        {
        }

        private SqlExpr AppendValue(object value)
        {
            script.Append(new SqlValue(value));
            return this;
        }

        private SqlExpr Append(object x)
        {
            script.Append(x);
            return this;
        }

        private SqlExpr Append(string x)
        {
            script.Append(x);
            return this;
        }

        private SqlExpr AppendSpace(string x) => Append(x).AppendSpace();
        private SqlExpr WrapSpace(string x) => AppendSpace().Append(x).AppendSpace();
        private SqlExpr AppendSpace() => Append(" ");


        internal static SqlExpr Assign(string name, object value)
        {
            return ColumnName(name, null).WrapSpace("=").AppendValue(value);
        }

        internal static SqlExpr Equal(string name, object value)
        {
            if (value == null || value == DBNull.Value)
                return ColumnName(name, null).WrapSpace("IS NULL");
            else
                return ColumnName(name, null).WrapSpace("=").AppendValue(value);
        }

        internal static SqlExpr ColumnName(string name, string alias)
        {
            SqlExpr exp = new SqlExpr();
            if (alias != null)
                exp.Append(alias)
                    .Append(".");

            exp.Append("[" + name + "]");

            return exp;
        }

        internal static SqlExpr AllColumnNames(string dbo)
        {
            SqlExpr exp = new SqlExpr();
            if (dbo != null)
                exp.Append(dbo).Append(".");

            exp.Append("*");
            return exp;
        }

        internal static SqlExpr ParameterName(string name)
        {
            SqlExpr exp = new SqlExpr().Append(name.SqlParameterName());
            exp.AddParam(name, null);
            return exp;
        }

        internal static SqlExpr AddParameter(string columnName, string parameterName)
        {
            SqlExpr exp = new SqlExpr()
                .Append("[" + columnName + "]")
                .Append("=")
                .Append(parameterName.SqlParameterName());

            exp.AddParam(parameterName, columnName);

            return exp;
        }

        internal static SqlExpr Join(SqlExpr[] expl)
        {
            SqlExpr exp = new SqlExpr()
                .Append(string.Join<SqlExpr>(",", expl));
            return exp;
        }

        internal static SqlExpr Write(string any)
        {
            return new SqlExpr().Append(any);
        }

#if USE
        public static explicit operator string(SqlExpr x)
        {
            return x.ToString();
        }

        public static explicit operator bool(SqlExpr x)
        {
            return x.expr == "1";
        }

        public static explicit operator char(SqlExpr x)
        {
            return x.ToString()[0];
        }

        public static explicit operator byte(SqlExpr x)
        {
            return Convert.ToByte(x.expr);
        }

        public static explicit operator sbyte(SqlExpr x)
        {
            return Convert.ToSByte(x.expr);
        }

        public static explicit operator short(SqlExpr x)
        {
            return Convert.ToInt16(x.expr);
        }

        public static explicit operator ushort(SqlExpr x)
        {
            return Convert.ToUInt16(x.expr);
        }

        public static explicit operator uint(SqlExpr x)
        {
            return Convert.ToUInt32(x.expr);
        }
        public static explicit operator long(SqlExpr x)
        {
            return Convert.ToInt64(x.expr);
        }

        public static explicit operator ulong(SqlExpr x)
        {
            return Convert.ToUInt64(x.expr);
        }

        public static explicit operator float(SqlExpr x)
        {
            return Convert.ToSingle(x.expr);
        }

        public static explicit operator DateTime(SqlExpr x)
        {
            return Convert.ToDateTime(x.expr);
        }

        public static explicit operator DBNull(SqlExpr x)
        {
            if (script.ToString() == "NULL")
                return System.DBNull.Value;
            else
                throw new SysException("cannot cast value {0} to System.DBNull", x);
        }

#endif

        #region implicit section
        public static implicit operator SqlExpr(ident ident)
        {
            return new SqlExpr().Append(ident);
        }


        public static implicit operator SqlExpr(string value)
        {
            return new SqlExpr().AppendValue(value);    // s= 'string'
        }

        public static implicit operator SqlExpr(bool value)
        {
            return new SqlExpr().AppendValue(value);    // b=1 or b=0
        }


        public static implicit operator SqlExpr(char value)
        {
            return new SqlExpr().AppendValue(value);    // ch= 'c'
        }

        public static implicit operator SqlExpr(byte value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(sbyte value)
        {
            return new SqlExpr().AppendValue(value);
        }


        public static implicit operator SqlExpr(int value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(short value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(ushort value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(uint value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(long value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(ulong value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(float value)
        {
            return new SqlExpr().AppendValue(value);
        }

        public static implicit operator SqlExpr(DateTime value)
        {
            return new SqlExpr().AppendValue(value);    //dt = '10/20/2012'
        }

        public static implicit operator SqlExpr(DBNull value)
        {
            return new SqlExpr().AppendValue(value);    // NULL
        }

        public static implicit operator SqlExpr(Enum value)
        {
            return new SqlExpr().AppendValue(Convert.ToInt32(value));    // NULL
        }

        #endregion


        /// <summary>
        /// string s = (string)expr;
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static explicit operator string(SqlExpr expr)
        {
            return expr.ToString();
        }

        public SqlExpr AS(SqlExpr alias)
        {
            this.WrapSpace("AS").Append(alias);
            return this;
        }


        public SqlExpr this[SqlExpr exp] => this.Append("[").Append(exp).Append("]");

        public SqlExpr IN(SqlBuilder select)
        {
            this.WrapSpace($"IN ({select.Query})");
            this.Merge(select);
            return this;
        }

        public SqlExpr IN(params SqlExpr[] collection)
        {
            string values = string.Join(",", collection.Select(x => x.ToString()));
            return this.WrapSpace($"IN ({values})");
        }

        public SqlExpr IN<T>(IEnumerable<T> collection) => this.WrapSpace($"IN ({string.Join<T>(",", collection)})");

        public SqlExpr BETWEEN(SqlExpr exp1, SqlExpr exp2) => this.WrapSpace($"BETWEEN {exp1} AND {exp2}");

        public SqlExpr IS() => this.WrapSpace("IS");
        public SqlExpr IS_NULL() => this.WrapSpace("IS NULL");
        public SqlExpr IS_NOT_NULL() => this.WrapSpace("IS NOT NULL");
        public SqlExpr NOT() => this.WrapSpace("NOT");
        public SqlExpr NULL() => this.WrapSpace("NULL");


        #region +-*/, compare, logical operation

        /// <summary>
        /// Compound expression
        /// </summary>
        private bool compound = false;

        private static string ExpToString(SqlExpr exp)
        {
            if (exp.compound)
                return string.Format("({0})", exp);
            else
                return exp.ToString();
        }

        internal static SqlExpr OPR(SqlExpr exp1, string opr, SqlExpr exp2)
        {
            SqlExpr exp = new SqlExpr()
                .Append(string.Format("{0} {1} {2}", ExpToString(exp1), opr, ExpToString(exp2)));

            exp.Merge(exp1).Merge(exp2);

            exp.compound = true;
            return exp;
        }

        // AND(A==1, B!=3, C>4) => "(A=1 AND B<>3 AND C>4)"
        internal static SqlExpr OPR(SqlExpr exp1, string opr, SqlExpr[] exps)
        {
            SqlExpr exp = new SqlExpr();
            exp.Append("(")
               .Append(string.Format("{0}", ExpToString(exp1)));

            foreach (SqlExpr exp2 in exps)
            {
                exp.Append(string.Format(" {0} {1}", opr, ExpToString(exp2)));
            }

            exp.compound = true;
            return exp.Append(")");
        }

        private static SqlExpr OPR(string opr, SqlExpr exp1)
        {
            SqlExpr exp = new SqlExpr()
                .Append(string.Format("{0} {1}", opr, ExpToString(exp1)));

            exp.Merge(exp1);
            return exp;
        }

        public static SqlExpr operator -(SqlExpr exp1)
        {
            return OPR("-", exp1);
        }

        public static SqlExpr operator +(SqlExpr exp1)
        {
            return OPR("+", exp1);
        }

        public static SqlExpr operator +(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "+", exp2);
        }

        public static SqlExpr operator -(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "-", exp2);
        }

        public static SqlExpr operator *(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "*", exp2);
        }

        public static SqlExpr operator /(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "/", exp2);
        }

        public static SqlExpr operator %(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "%", exp2);
        }


        public static SqlExpr operator ==(SqlExpr exp1, SqlExpr exp2)
        {
            if ((object)exp2 == null || exp2.ToString() == "NULL")
            {
                SqlExpr exp = new SqlExpr().Append(exp1).Append(" IS NULL");
                exp.Merge(exp1);
                return exp;
            }

            return OPR(exp1, "=", exp2);
        }


        public static SqlExpr operator !=(SqlExpr exp1, SqlExpr exp2)
        {
            if ((object)exp2 == null || exp2.ToString() == "NULL")
            {
                SqlExpr exp = new SqlExpr().Append(exp1).Append(" IS NOT NULL");
                exp.Merge(exp1);
                return exp;
            }

            return OPR(exp1, "<>", exp2);
        }

        public static SqlExpr operator >(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, ">", exp2);
        }

        public static SqlExpr operator <(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "<", exp2);
        }

        public static SqlExpr operator >=(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, ">=", exp2);
        }

        public static SqlExpr operator <=(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "<=", exp2);
        }


        public static SqlExpr operator &(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "AND", exp2);
        }

        public static SqlExpr operator |(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "OR", exp2);
        }

        public static SqlExpr operator ~(SqlExpr exp)
        {
            return OPR("NOT", exp);
        }

        #endregion


        #region SQL Function

        internal static SqlExpr Func(string func, params SqlExpr[] expl)
        {
            SqlExpr exp = new SqlExpr()
                .Append(func)
                .Append("(")
                .Append(string.Join<SqlExpr>(",", expl))
                .Append(")");

            //exp.Merge(exp1);
            return exp;
        }


        #endregion


        public override bool Equals(object obj)
        {
            return script.Equals(((SqlExpr)obj).script);
        }

        public override int GetHashCode()
        {
            return script.GetHashCode();
        }

        public override string ToString()
        {
            return script.ToString();
        }
    }
}
