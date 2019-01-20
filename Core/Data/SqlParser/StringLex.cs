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

using System.Text;

namespace Sys.Data.SqlParser
{
    class StringLex : Lexer
    {
        private StringBuilder buffer;
        private int index;

        public StringLex(string sourceCode, Error error)
            : base(error)
        {
            buffer = new StringBuilder(sourceCode);
            index = 0;
            NextCh();
        }

        public override void Close()
        {
        }

        protected override char NextCh()
        {
            if (!(index < buffer.Length))
                return ch = (char)0;

            ch = buffer[index++];
            base.NextCh();
            return ch;
        }

        protected override void set_index(int index)
        {
            this.index = index;
            ch = buffer[index - 1];
        }


        public override int Index()
        {
            return this.index;
        }

    }
}
