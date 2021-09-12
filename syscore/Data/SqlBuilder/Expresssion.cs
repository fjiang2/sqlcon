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
    public sealed class Expresssion : SqlBuilderInfo
    {
     
        private StringBuilder script = new StringBuilder();

        private Expresssion()
        {
        }



        private Expresssion NextValue(object value)
        {
            script.Append(new SqlValue(value));
            return this;
        }

        private Expresssion Next(object x)
        {
            script.Append(x);
            return this;
        }

        internal static Expresssion Assign(string name, object value)
        {
            return ColumnName(name, null).Next(" = ").NextValue(value);
        }

        internal static Expresssion Equal(string name, object value)
        {
            if (value == null || value == DBNull.Value)
                return ColumnName(name, null).Next(" IS NULL");
            else
                return ColumnName(name, null).Next(" = ").NextValue(value);
        }

        internal static Expresssion ColumnName(string name, string alias)
        {
            Expresssion exp = new Expresssion();
            if (alias != null)
                exp.Next(alias)
                    .Next(".");

            exp.Next("[" + name + "]");

            return exp;
        }

        internal static Expresssion AllColumnNames(string alias)
        {
            Expresssion exp = new Expresssion();
            if (alias != null)
                exp.Next(alias)
                    .Next(".");

            exp.Next("*");
            return exp;
        }

        internal static Expresssion ParameterName(string name)
        {
            Expresssion exp = new Expresssion().Next(name.SqlParameterName());
            exp.AddParam(name, null);
            return exp;
        }

        internal static Expresssion AddParameter(string columnName, string parameterName)
        {
            Expresssion exp = new Expresssion()
                .Next("[" + columnName + "]")
                .Next("=")
                .Next(parameterName.SqlParameterName());

            exp.AddParam(parameterName, columnName);

            return exp;
        }

        internal static Expresssion Join(Expresssion[] expl)
        {
            Expresssion exp = new Expresssion()
                .Next(string.Join<Expresssion>(",", expl));
            return exp;
        }

        internal static Expresssion Write(string any)
        {
            return new Expresssion().Next(any);
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
        public static implicit operator Expresssion(ident ident)
        {
            return new Expresssion().Next(ident);
        }


        public static implicit operator Expresssion(string value)
        {
            return new Expresssion().NextValue(value);    // s= 'string'
        }

        public static implicit operator Expresssion(bool value)
        {
            return new Expresssion().NextValue(value);    // b=1 or b=0
        }


        public static implicit operator Expresssion(char value)
        {
            return new Expresssion().NextValue(value);    // ch= 'c'
        }

        public static implicit operator Expresssion(byte value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(sbyte value)
        {
            return new Expresssion().NextValue(value);
        }


        public static implicit operator Expresssion(int value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(short value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(ushort value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(uint value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(long value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(ulong value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(float value)
        {
            return new Expresssion().NextValue(value);
        }

        public static implicit operator Expresssion(DateTime value)
        {
            return new Expresssion().NextValue(value);    //dt = '10/20/2012'
        }

        public static implicit operator Expresssion(DBNull value)
        {
            return new Expresssion().NextValue(value);    // NULL
        }

        public static implicit operator Expresssion(Enum value)
        {
            return new Expresssion().NextValue(Convert.ToInt32(value));    // NULL
        }

        #endregion


        /// <summary>
        /// string s = (string)expr;
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static explicit operator string(Expresssion expr)
        {
            return expr.ToString();
        }

        public Expresssion AS(Expresssion alias)
        {
            this.Next(" AS ").Next(alias);
            return this;
        }


        public Expresssion this[Expresssion exp]
        {
            get
            {
                this.Next("[").Next(exp).Next("]");
                return this;
            }
        }


        public Expresssion IN(SqlBuilder select)
        {
            this
                    .Next(" IN (")
                    .Next(select.Clause)
                    .Next(")");

            this.Merge(select);
            return this;
        }

        public Expresssion IN(params Expresssion[] collection)
        {
            return IN(collection);
        }

        public Expresssion NOT
        {
            get
            {
                this.Next(" NOT");
                return this;
            }
        }

        public Expresssion NULL
        {
            get
            {
                this.Next(" NULL");
                return this;
            }
        }

        public Expresssion IS
        {
            get
            {
                this.Next(" IS");
                return this;
            }
        }

        public Expresssion IN<T>(IEnumerable<T> collection)
        {
            this.Next(" IN (")
                .Next(string.Join<T>(",", collection))
                .Next(")");

            return this;
        }


        public Expresssion BETWEEN(Expresssion exp1, Expresssion exp2)
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

        private static string ExpToString(Expresssion exp)
        {
            if (exp.compound)
                return string.Format("({0})", exp);
            else
                return exp.ToString();
        }

        internal static Expresssion OPR(Expresssion exp1, string opr, Expresssion exp2)
        {
            Expresssion exp = new Expresssion()
                .Next(string.Format("{0} {1} {2}", ExpToString(exp1), opr, ExpToString(exp2)));

            exp.Merge(exp1).Merge(exp2);

            exp.compound = true;
            return exp;
        }

        // AND(A==1, B!=3, C>4) => "(A=1 AND B<>3 AND C>4)"
        internal static Expresssion OPR(Expresssion exp1, string opr, Expresssion[] exps)
        {
            Expresssion exp = new Expresssion();
            exp.Next("(")
               .Next(string.Format("{0}", ExpToString(exp1)));

            foreach (Expresssion exp2 in exps)
            {
                exp.Next(string.Format(" {0} {1}", opr, ExpToString(exp2)));
            }

            exp.compound = true;
            return exp.Next(")");
        }

        internal static Expresssion OPR(string opr, Expresssion exp1)
        {
            Expresssion exp = new Expresssion()
                .Next(string.Format("{0} {1}", opr, ExpToString(exp1)));

            exp.Merge(exp1);
            return exp;
        }

        public static Expresssion operator -(Expresssion exp1)
        {
            return OPR("-", exp1);
        }

        public static Expresssion operator +(Expresssion exp1)
        {
            return OPR("+", exp1);
        }

        public static Expresssion operator +(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "+", exp2);
        }

        public static Expresssion operator -(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "-", exp2);
        }

        public static Expresssion operator *(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "*", exp2);
        }

        public static Expresssion operator /(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "/", exp2);
        }

        public static Expresssion operator %(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "%", exp2);
        }


        public static Expresssion operator ==(Expresssion exp1, Expresssion exp2)
        {
            if ((object)exp2 == null || exp2.ToString() == "NULL")
            {
                Expresssion exp = new Expresssion().Next(exp1).Next(" IS NULL");
                exp.Merge(exp1);
                return exp;
            }

            return OPR(exp1, "=", exp2);
        }


        public static Expresssion operator !=(Expresssion exp1, Expresssion exp2)
        {
            if ((object)exp2 == null || exp2.ToString() == "NULL")
            {
                Expresssion exp = new Expresssion().Next(exp1).Next(" IS NOT NULL");
                exp.Merge(exp1);
                return exp;
            }

            return OPR(exp1, "<>", exp2);
        }

        public static Expresssion operator >(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, ">", exp2);
        }

        public static Expresssion operator <(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "<", exp2);
        }

        public static Expresssion operator >=(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, ">=", exp2);
        }

        public static Expresssion operator <=(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "<=", exp2);
        }


        public static Expresssion operator &(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "&", exp2);
        }

        public static Expresssion operator |(Expresssion exp1, Expresssion exp2)
        {
            return OPR(exp1, "|", exp2);
        }

        public static Expresssion operator ~(Expresssion exp)
        {
            return OPR("~", exp);
        }

        #endregion


        #region SQL Function

        internal static Expresssion Func(string func, params Expresssion[] expl)
        {
            Expresssion exp = new Expresssion()
                .Next(func)
                .Next("(")
                .Next(string.Join<Expresssion>(",", expl))
                .Next(")");

            //exp.Merge(exp1);
            return exp;
        }

        #endregion


        public override bool Equals(object obj)
        {
            return script.Equals(((Expresssion)obj).script);
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
