//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//


using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sys.Data.SqlParser
{


    class JToken
    {

        public SYMBOL sy;
        public Sym sym;
        public SYMBOL2 opr;


        public JToken()
        {
            sym = new Sym();

        }

        public JToken(SYMBOL sy, SYMBOL2 opr)
            : this()
        {
            this.sy = sy;
            this.opr = opr;
        }


    }




}
