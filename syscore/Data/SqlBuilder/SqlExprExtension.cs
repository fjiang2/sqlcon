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
    public static class SqlExprExtension 
    {
        #region SqlExpr/SqlClause: ColumName/ParameterName/AddParameter

    
        public static Expresssion Assign(this string name, object value)
        {
          return Expresssion.Assign(name, value);
        }
        public static Expresssion Equal(this string name, object value)
        {
            return Expresssion.Equal(name, value);
        }

        /// <summary>
        /// "name" -> "[name]"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Expresssion ColumnName(this string name)
        {
            return Expresssion.ColumnName(name, null);
        }

        public static Expresssion ColumnName(this string name, string alias)
        {
            return Expresssion.ColumnName(name, alias);
        }

        public static Expresssion ColumnName(this string[] names)
        {
            var L = names.Select(column => column.ColumnName()).ToArray();
            return Expresssion.Join(L);
        }

        public static Expresssion Func(this string name, params Expresssion[] args)
        {
            return Expresssion.Func(name, args);
        }


        /// <summary>
        /// write directly into SQL clause
        /// </summary>
        /// <param name="any"></param>
        /// <returns></returns>
        public static Expresssion Inject(this string any)
        {
            return Expresssion.Write(any);
        }
        /// <summary>
        /// "name" -> "@name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Expresssion ParameterName(this string name)
        {
            return Expresssion.ParameterName(name);
        }


        /// <summary>
        /// "name" -> "[name]=@name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Expresssion AddParameter(this string columnName)
        {
            return Expresssion.AddParameter(columnName, columnName);
        }

        public static Expresssion AddParameter(this string columnName, Expresssion value)
        {
            return Expresssion.AddParameter(columnName, columnName);
        }

        /// <summary>
        /// Add SQL parameter
        /// e.g. NodeDpo._ID.AddParameter(TaskDpo._ParentID) -> "[ID]=@ParentID"
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Expresssion AddParameter(this string columnName, string parameterName)
        {
            return Expresssion.AddParameter(columnName, parameterName);
        }

        #endregion

        public static Expresssion AND(this Expresssion exp1, Expresssion exp2)
        {
            return Expresssion.OPR(exp1, "AND", exp2);
        }
        public static Expresssion AND(this IEnumerable<Expresssion> expl)
        {
            if(expl.Count() >1)
                return Expresssion.OPR(expl.First(), "AND", expl.Skip(1).ToArray());
            else
                return expl.First();
        }


        public static Expresssion OR(this Expresssion exp1, Expresssion exp2)
        {
            return Expresssion.OPR(exp1, "OR", exp2);
        }

        public static Expresssion OR(this IEnumerable<Expresssion> expl)
        {
            if (expl.Count() > 1)
                return Expresssion.OPR(expl.First(), "OR", expl.Skip(1).ToArray());
            else
                return expl.First();
        }


        public static Expresssion NOT(this Expresssion exp)
        {
            return Expresssion.OPR("NOT", exp);
        }

        public static Expresssion LEN(this Expresssion expr)
        {
            return Expresssion.Func("LEN", expr);
        }

        public static Expresssion SUBSTRING(this Expresssion expr, Expresssion start, Expresssion length)
        {
            return Expresssion.Func("SUBSTRING", expr, start, length);
        }


        public static Expresssion SUM(this Expresssion expr)
        {
            return Expresssion.Func("SUM", expr);
        }

        public static Expresssion MAX(this Expresssion expr)
        {
            return Expresssion.Func("MAX", expr);
        }

        public static Expresssion MIN(this Expresssion expr)
        {
            return Expresssion.Func("MIN", expr);
        }

        public static Expresssion COUNT(this Expresssion expr)
        {
            return Expresssion.Func("COUNT", expr);
        }


        public static Expresssion ISNULL(this Expresssion expr)
        {
            return Expresssion.Func("ISNULL", expr);
        }

        public static Expresssion GETDATE()
        {
            return Expresssion.Func("GETDATE");
        }
    }
}
