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
        internal const string PHYSLOC = "%%physloc%%";
        internal const string ROWID = "%%RowId%%";

        private StringBuilder script = new StringBuilder();

        private SqlExpr()
        {
        }

      

        private SqlExpr NextValue(object value)
        {
            script.Append(new SqlValue(value).Text);
            return this;
        }

        private SqlExpr Next(object x)
        {
            script.Append(x);
            return this;
        }

        internal static SqlExpr Assign(string name, object value)
        {
            return ColumnName(name, null).Next(" = ").NextValue(value);
        }

        internal static SqlExpr ColumnName(string name, string alias)
        {
            SqlExpr exp = new SqlExpr();
            if (alias != null)
                exp.Next(alias)
                    .Next(".");

            exp.Next("[" + name + "]");
            
            return exp;
        }

        internal static SqlExpr AllColumnNames(string alias)
        {
            SqlExpr exp = new SqlExpr();
            if (alias != null)
                exp.Next(alias)
                    .Next(".");

            exp.Next("*");
            return exp;
        }

        internal static SqlExpr ParameterName(string name)
        {
            SqlExpr exp = new SqlExpr().Next(name.SqlParameterName());
            exp.AddParam(name, null);
            return exp;
        }

        internal static SqlExpr AddParameter(string columnName, string parameterName)
        {
            SqlExpr exp = new SqlExpr()
                .Next("[" + columnName + "]")
                .Next("=")
                .Next(parameterName.SqlParameterName());

            exp.AddParam(parameterName, columnName);
            
            return exp;
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
            return new SqlExpr().Next(ident);
        }


        public static implicit operator SqlExpr(string value)
        {
            return new SqlExpr().NextValue(value);    // s= 'string'
        }

        public static implicit operator SqlExpr(bool value)
        {
            return new SqlExpr().NextValue(value);    // b=1 or b=0
        }


        public static implicit operator SqlExpr(char value)
        {
            return new SqlExpr().NextValue(value);    // ch= 'c'
        }

        public static implicit operator SqlExpr(byte value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(sbyte value)
        {
            return new SqlExpr().NextValue(value);
        }


        public static implicit operator SqlExpr(int value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(short value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(ushort value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(uint value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(long value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(ulong value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(float value)
        {
            return new SqlExpr().NextValue(value);
        }

        public static implicit operator SqlExpr(DateTime value)
        {
            return new SqlExpr().NextValue(value);    //dt = '10/20/2012'
        }

        public static implicit operator SqlExpr(DBNull value)
        {
            return new SqlExpr().NextValue(value);    // NULL
        }

        public static implicit operator SqlExpr(Enum value)
        {
            return new SqlExpr().NextValue(Convert.ToInt32(value));    // NULL
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
            this.Next(" AS ").Next(alias);
            return this;
        }
        
        
        public SqlExpr this[SqlExpr exp]
        {
            get
            {
                this.Next("[").Next(exp).Next("]");
                return this;
            }
        }


        public SqlExpr IN(SqlBuilder select)
        {
            this
                    .Next(" IN (")
                    .Next(select.Clause)
                    .Next(")");

            this.Merge(select);
            return this;
        }

        public SqlExpr IN(params SqlExpr[] collection)
        {
            return IN(collection);
        }

        public SqlExpr NOT
        {
            get
            {
                this.Next(" NOT");
                return this;
            }
        }

        public SqlExpr NULL
        {
            get
            {
                this.Next(" NULL");
                return this;
            }
        }

        public SqlExpr IS
        {
            get
            {
                this.Next(" IS");
                return this;
            }
        }

        public SqlExpr IN<T>(IEnumerable<T> collection)
        {
            this.Next(" IN (")
                .Next(string.Join<T>(",", collection))
                .Next(")");

            return this;
        }

      
        public SqlExpr BETWEEN(SqlExpr exp1, SqlExpr exp2)
        {
            this.Next(" BETWEEN ")
                .Next(exp1)
                .Next(" AND ")
                .Next(exp2);

            return this;
        }

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
                .Next(string.Format("{0} {1} {2}", ExpToString(exp1), opr, ExpToString(exp2)));

            exp.Merge(exp1).Merge(exp2);
            
            exp.compound = true;
            return exp;
        }

        // AND(A==1, B!=3, C>4) => "(A=1 AND B<>3 AND C>4)"
        internal static SqlExpr OPR(SqlExpr exp1, string opr, SqlExpr[] exps)
        {
            SqlExpr exp = new SqlExpr();
            exp.Next("(")
               .Next(string.Format("{0}", ExpToString(exp1)));
            
            foreach(SqlExpr exp2 in exps)
            {
                exp.Next(string.Format(" {0} {1}", opr, ExpToString(exp2)));
            }

            exp.compound = true;
            return exp.Next(")");
        }

        internal static SqlExpr OPR(string opr, SqlExpr exp1)
        {
            SqlExpr exp = new SqlExpr()
                .Next(string.Format("{0} {1}", opr, ExpToString(exp1)));

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
                SqlExpr exp = new SqlExpr().Next(exp1).Next(" IS NULL");
                exp.Merge(exp1);
                return exp;
            }

            return OPR(exp1, "=", exp2);
        }


        public static SqlExpr operator !=(SqlExpr exp1, SqlExpr exp2)
        {
            if ((object)exp2 == null || exp2.ToString() == "NULL")
            {
                SqlExpr exp = new SqlExpr().Next(exp1).Next(" IS NOT NULL");
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
            return OPR(exp1, "&", exp2);
        }

        public static SqlExpr operator |(SqlExpr exp1, SqlExpr exp2)
        {
            return OPR(exp1, "|", exp2);
        }

        public static SqlExpr operator ~(SqlExpr exp)
        {
            return OPR("~", exp);
        }
        
        #endregion


        #region SQL Function

        internal static SqlExpr Func(string func, params SqlExpr[] expl)
        {
            SqlExpr exp = new SqlExpr()
                .Next(func)
                .Next("(")
                .Next(string.Join<SqlExpr>(",", expl))
                .Next(")");

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
