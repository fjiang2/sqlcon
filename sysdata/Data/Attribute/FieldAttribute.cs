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
    /// describe a field in class or enum
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class FieldAttribute : Attribute
    {
        /// <summary>
        /// description of field
        /// </summary>
        public string Caption;

        /// <summary>
        /// construct by description
        /// </summary>
        /// <param name="caption"></param>
        public FieldAttribute(string caption)
        {
            this.Caption = caption;
        }


        /// <summary>
        /// return attribute description
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string attribute = typeof(FieldAttribute).Name.Replace("Attribute", "");
            
            //handling escape letter in the Caption
            string field = new VAL(Caption).ToString();

            return string.Format("[{0}({1})]", attribute, field);
        }
    }

   
}
