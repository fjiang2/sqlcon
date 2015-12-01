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
using System.Text;
using Tie;

namespace Sys.Data
{
    /// <summary>
    /// used for user defined data enum type from database
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class DataEnumAttribute : Attribute
    {
        /// <summary>
        /// description of DataEnum
        /// </summary>
        public readonly string Caption;
        
        /// <summary>
        /// no description
        /// </summary>
        public DataEnumAttribute()
        {
        }

        /// <summary>
        /// constuct by description
        /// </summary>
        /// <param name="caption"></param>
        public DataEnumAttribute(string caption)
        {
            this.Caption = caption;
        }

        /// <summary>
        /// return name of DataEnum Attribute
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string attribute = typeof(DataEnumAttribute).Name.Replace("Attribute", "");
            return string.Format("[{0}]", attribute);
        }
    }

   
}
