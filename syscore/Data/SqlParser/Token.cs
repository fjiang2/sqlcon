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

    class Token
    {
        public SYMBOL sy;
        public SYMBOL2 opr;
        private object sym;

        public Token()
        {
        }

        public Token(SYMBOL sy, SYMBOL2 opr)
            : this()
        {
            this.sy = sy;
            this.opr = opr;
        }

        public void SetInt32(int inum)
        {
            this.sym = inum;
        }

        public void SetDouble(double fnum)
        {
            this.sym = fnum;
        }

        public void SetIdent(string ident)
        {
            this.sym = ident;
        }

        public void SetString(string stab)
        {
            this.sym = stab;
        }

        public double fnum => (double)sym;              // real number from insymbol 

        public int inum => (int)sym;				// integer from insymbol 

        public string id => (string)sym;


        public string stab          // string table
        {
            get
            {
                if (sy == SYMBOL.stringcon)
                    return (string)sym;

                return null;
            }
        }

        private string encode()
        {
            //search keyword
            for (int i = 0; i < Keywords.Keys.Length; i++)
            {
                if (sy == Keywords.Keys[i].ksy)
                {
                    return Keywords.Keys[i].key;
                }
            }

            StringWriter o = new StringWriter();
            switch (sy)
            {
                case SYMBOL.intcon:
                    o.Write(inum);
                    break;

                case SYMBOL.floatcon:
                    o.Write(fnum);
                    if (Math.Ceiling(fnum) == fnum)
                        o.Write(".0");
                    break;

                case SYMBOL.stringcon:
                    o.Write("\"{0}\"", stab);
                    break;

                case SYMBOL.identsy:
                    o.Write("{0}", id);
                    break;

                //---------------------------------------------------------------------
                case SYMBOL.PLUS: o.Write('+'); break;
                case SYMBOL.MINUS: o.Write('-'); break;
                case SYMBOL.STAR: o.Write('*'); break;
                case SYMBOL.DIV: o.Write('/'); break;
                case SYMBOL.MOD: o.Write('%'); break;

                case SYMBOL.INCOP:
                    switch (opr)
                    {
                        case SYMBOL2.PPLUS: o.Write("++"); break;
                        case SYMBOL2.MMINUS: o.Write("--"); break;
                    }
                    break;

                case SYMBOL.ASSIGNOP:
                    switch (opr)
                    {
                        case SYMBOL2.ePLUS: o.Write("+="); break;
                        case SYMBOL2.eMINUS: o.Write("-="); break;
                        case SYMBOL2.eSTAR: o.Write("*="); break;
                        case SYMBOL2.eDIV: o.Write("/="); break;
                        case SYMBOL2.eMOD: o.Write("%="); break;
                        case SYMBOL2.eAND: o.Write("&="); break;
                        case SYMBOL2.eOR: o.Write("|="); break;
                        case SYMBOL2.eXOR: o.Write("^="); break;
                        case SYMBOL2.eSHL: o.Write("<<="); break;
                        case SYMBOL2.eSHR: o.Write(">>="); break;
                    }
                    break;
                case SYMBOL.EQUOP:
                    switch (opr)
                    {
                        case SYMBOL2.EQL: o.Write("=="); break;
                        case SYMBOL2.NEQ: o.Write("!="); break;
                    }
                    break;
                case SYMBOL.RELOP:
                    switch (opr)
                    {
                        case SYMBOL2.GTR: o.Write(">"); break;
                        case SYMBOL2.GEQ: o.Write(">="); break;
                        case SYMBOL2.LSS: o.Write("<"); break;
                        case SYMBOL2.LEQ: o.Write("<="); break;
                    }
                    break;

                case SYMBOL.SHIFTOP:
                    switch (opr)
                    {
                        case SYMBOL2.SHL: o.Write("<<"); break;
                        case SYMBOL2.SHR: o.Write(">>"); break;
                    }
                    break;

                case SYMBOL.EQUAL: o.Write('='); break;

                case SYMBOL.LP: o.Write('('); break;
                case SYMBOL.RP: o.Write(')'); break;
                case SYMBOL.LB: o.Write('['); break;
                case SYMBOL.RB: o.Write(']'); break;
                case SYMBOL.LC: o.Write('{'); break;
                case SYMBOL.RC: o.Write('}'); break;

                case SYMBOL.AND: o.Write("AND"); break;
                case SYMBOL.OR: o.Write("OR"); break;

                case SYMBOL.UNOP:
                    switch (opr)
                    {
                        case SYMBOL2.BNOT: o.Write('~'); break;
                        case SYMBOL2.NOT: o.Write("!"); break;
                        case SYMBOL2.NEG: o.Write("-"); break;
                    }
                    break;

                case SYMBOL.STRUCTOP:
                    switch (opr)
                    {
                        case SYMBOL2.DOT: o.Write('.'); break;
                        case SYMBOL2.ARROW: o.Write("->"); break;
                    }
                    break;

                case SYMBOL.QUEST: o.Write('?'); break;
                case SYMBOL.COLON: o.Write(':'); break;
                case SYMBOL.COMMA: o.Write(','); break;
                case SYMBOL.SEMI: o.WriteLine(';'); break;


                default: o.Write("undefined symbol:{0} {1}", sy, id); break;
            }

            return o.ToString();
        }


        public override String ToString()
        {
            return encode();
        }
    }




}
