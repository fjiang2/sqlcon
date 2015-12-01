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

namespace Sys.Data
{

    /// <summary>
    /// A locator is template string used in SQL WHERE clause
    /// e.g. ColumnaName1=@ColumnaName1 AND ColumnaName2 IS NULL OR ColumnaName3 >= @ColumnaName4
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class LocatorAttribute : Attribute
    {
        private Locator locator = null;
        public bool Unique = true;

        public LocatorAttribute(string any)
        {
            this.locator = new Locator(any);
            this.locator.Unique = this.Unique;
        }

        public LocatorAttribute(string[] columns)
        {
            this.locator = new Locator(columns);
            this.locator.Unique = this.Unique;
        }

        public Locator Locator
        {
            get 
            {
                return this.locator;
            }
        }

        public override string ToString()
        {
            return this.locator.ToString();
        }

    }
}
