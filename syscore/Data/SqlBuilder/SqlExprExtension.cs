﻿//--------------------------------------------------------------------------------------------------//
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

    
        public static SqlExpr Assign(this string name, object value)
        {
          return SqlExpr.Assign(name, value);
        }
        public static SqlExpr Equal(this string name, object value)
        {
            return SqlExpr.Equal(name, value);
        }

        /// <summary>
        /// "name" -> "[name]"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SqlExpr ColumnName(this string name)
        {
            return SqlExpr.ColumnName(name, null);
        }

        public static SqlExpr ColumnName(this string name, string alias)
        {
            return SqlExpr.ColumnName(name, alias);
        }

        public static SqlExpr ColumnName(this string[] names)
        {
            var L = names.Select(column => column.ColumnName()).ToArray();
            return SqlExpr.Join(L);
        }

        public static SqlExpr Func(this string name, params SqlExpr[] args)
        {
            return SqlExpr.Func(name, args);
        }


        /// <summary>
        /// write directly into SQL clause
        /// </summary>
        /// <param name="any"></param>
        /// <returns></returns>
        public static SqlExpr Inject(this string any)
        {
            return SqlExpr.Write(any);
        }
        /// <summary>
        /// "name" -> "@name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SqlExpr ParameterName(this string name)
        {
            return SqlExpr.ParameterName(name);
        }


        /// <summary>
        /// "name" -> "[name]=@name"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SqlExpr AddParameter(this string columnName)
        {
            return SqlExpr.AddParameter(columnName, columnName);
        }

        public static SqlExpr AddParameter(this string columnName, SqlExpr value)
        {
            return SqlExpr.AddParameter(columnName, columnName);
        }

        /// <summary>
        /// Add SQL parameter
        /// e.g. NodeDpo._ID.AddParameter(TaskDpo._ParentID) -> "[ID]=@ParentID"
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static SqlExpr AddParameter(this string columnName, string parameterName)
        {
            return SqlExpr.AddParameter(columnName, parameterName);
        }

        #endregion

        public static SqlExpr AND(this SqlExpr exp1, SqlExpr exp2)
        {
            return SqlExpr.OPR(exp1, "AND", exp2);
        }
        public static SqlExpr AND(this IEnumerable<SqlExpr> expl)
        {
            if(expl.Count() >1)
                return SqlExpr.OPR(expl.First(), "AND", expl.Skip(1).ToArray());
            else
                return expl.First();
        }


        public static SqlExpr OR(this SqlExpr exp1, SqlExpr exp2)
        {
            return SqlExpr.OPR(exp1, "OR", exp2);
        }

        public static SqlExpr OR(this IEnumerable<SqlExpr> expl)
        {
            if (expl.Count() > 1)
                return SqlExpr.OPR(expl.First(), "OR", expl.Skip(1).ToArray());
            else
                return expl.First();
        }


        public static SqlExpr NOT(this SqlExpr exp)
        {
            return SqlExpr.OPR("NOT", exp);
        }

        public static SqlExpr LEN(this SqlExpr expr)
        {
            return SqlExpr.Func("LEN", expr);
        }

        public static SqlExpr SUBSTRING(this SqlExpr expr, SqlExpr start, SqlExpr length)
        {
            return SqlExpr.Func("SUBSTRING", expr, start, length);
        }


        public static SqlExpr SUM(this SqlExpr expr)
        {
            return SqlExpr.Func("SUM", expr);
        }

        public static SqlExpr MAX(this SqlExpr expr)
        {
            return SqlExpr.Func("MAX", expr);
        }

        public static SqlExpr MIN(this SqlExpr expr)
        {
            return SqlExpr.Func("MIN", expr);
        }

        public static SqlExpr COUNT(this SqlExpr expr)
        {
            return SqlExpr.Func("COUNT", expr);
        }


        public static SqlExpr ISNULL(this SqlExpr expr)
        {
            return SqlExpr.Func("ISNULL", expr);
        }

        public static SqlExpr GETDATE()
        {
            return SqlExpr.Func("GETDATE");
        }
    }
}
