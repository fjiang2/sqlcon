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
        public Symbol sy;
        public OperatorSymbol opr;
        private object sym;

        public Token()
        {
        }

        public Token(Symbol sy, OperatorSymbol opr)
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
                if (sy == Symbol.stringcon)
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
                case Symbol.intcon:
                    o.Write(inum);
                    break;

                case Symbol.floatcon:
                    o.Write(fnum);
                    if (Math.Ceiling(fnum) == fnum)
                        o.Write(".0");
                    break;

                case Symbol.stringcon:
                    o.Write("\"{0}\"", stab);
                    break;

                case Symbol.identsy:
                    o.Write("{0}", id);
                    break;

                //---------------------------------------------------------------------
                case Symbol.PLUS: o.Write('+'); break;
                case Symbol.MINUS: o.Write('-'); break;
                case Symbol.STAR: o.Write('*'); break;
                case Symbol.DIV: o.Write('/'); break;
                case Symbol.MOD: o.Write('%'); break;

                case Symbol.INCOP:
                    switch (opr)
                    {
                        case OperatorSymbol.PPLUS: o.Write("++"); break;
                        case OperatorSymbol.MMINUS: o.Write("--"); break;
                    }
                    break;

                case Symbol.ASSIGNOP:
                    switch (opr)
                    {
                        case OperatorSymbol.ePLUS: o.Write("+="); break;
                        case OperatorSymbol.eMINUS: o.Write("-="); break;
                        case OperatorSymbol.eSTAR: o.Write("*="); break;
                        case OperatorSymbol.eDIV: o.Write("/="); break;
                        case OperatorSymbol.eMOD: o.Write("%="); break;
                        case OperatorSymbol.eAND: o.Write("&="); break;
                        case OperatorSymbol.eOR: o.Write("|="); break;
                        case OperatorSymbol.eXOR: o.Write("^="); break;
                        case OperatorSymbol.eSHL: o.Write("<<="); break;
                        case OperatorSymbol.eSHR: o.Write(">>="); break;
                    }
                    break;
                case Symbol.EQUOP:
                    switch (opr)
                    {
                        case OperatorSymbol.EQL: o.Write("=="); break;
                        case OperatorSymbol.NEQ: o.Write("!="); break;
                    }
                    break;
                case Symbol.RELOP:
                    switch (opr)
                    {
                        case OperatorSymbol.GTR: o.Write(">"); break;
                        case OperatorSymbol.GEQ: o.Write(">="); break;
                        case OperatorSymbol.LSS: o.Write("<"); break;
                        case OperatorSymbol.LEQ: o.Write("<="); break;
                    }
                    break;

                case Symbol.SHIFTOP:
                    switch (opr)
                    {
                        case OperatorSymbol.SHL: o.Write("<<"); break;
                        case OperatorSymbol.SHR: o.Write(">>"); break;
                    }
                    break;

                case Symbol.EQUAL: o.Write('='); break;

                case Symbol.LP: o.Write('('); break;
                case Symbol.RP: o.Write(')'); break;
                case Symbol.LB: o.Write('['); break;
                case Symbol.RB: o.Write(']'); break;
                case Symbol.LC: o.Write('{'); break;
                case Symbol.RC: o.Write('}'); break;

                case Symbol.AND: o.Write("AND"); break;
                case Symbol.OR: o.Write("OR"); break;

                case Symbol.UNOP:
                    switch (opr)
                    {
                        case OperatorSymbol.BNOT: o.Write('~'); break;
                        case OperatorSymbol.NOT: o.Write("!"); break;
                        case OperatorSymbol.NEG: o.Write("-"); break;
                    }
                    break;

                case Symbol.STRUCTOP:
                    switch (opr)
                    {
                        case OperatorSymbol.DOT: o.Write('.'); break;
                        case OperatorSymbol.ARROW: o.Write("->"); break;
                    }
                    break;

                case Symbol.QUEST: o.Write('?'); break;
                case Symbol.COLON: o.Write(':'); break;
                case Symbol.COMMA: o.Write(','); break;
                case Symbol.SEMI: o.WriteLine(';'); break;


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
