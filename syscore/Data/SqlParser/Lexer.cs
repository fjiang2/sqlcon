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


    class Lexer
    {
        protected char ch;

        private Token tok;
        private Error error;			//the positon of cursor in file

        public Lexer(Error error)
        {
            this.error = error;
            this.tok = new Token();
        }

        public virtual void Close()
        {
        }

        protected virtual char NextCh()
        {
            if (Index() > error.Position.cur)  //ignore trackback characters
                error.Position.Move(ch);

            return ch;
        }

        #region GetKeyAndIdent(), strcmp()

        private bool GetKeyAndIdent()
        {
            int i, j, k;
            char[] ident = new char[Constant.ALNG];

            // IDENT   
            if (ch == '_' || ch == '$' || (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z'))
            {
                k = 0;
                for (i = 0; i < Constant.ALNG; i++) ident[i] = (char)0;  //ALNG=10
                if (ch == '$')                              //a variable begun with $ is system varible
                {
                    ident[k++] = ch;
                    NextCh();
                }

                do
                {
                    if (k < Constant.ALNG)
                    {
                        ident[k] = ch;
                        k++;
                    }
                    NextCh();
                } while (ch == '_'
                        || ((ch >= 'A') && (ch <= 'Z'))
                        || ((ch >= 'a') && (ch <= 'z'))
                        || ((ch >= '0') && (ch <= '9'))
                        || ch == '`'
                    );

                tok.SetIdent(new string(ident, 0, k));

                i = 0; j = Keywords.Keys.Length - 1;  //binary search
                do
                {
                    k = (i + j) / 2;
                    if (strcmp(ident, tok.id.Length, Keywords.Keys[k].key) <= 0) j = k - 1;
                    if (strcmp(ident, tok.id.Length, Keywords.Keys[k].key) >= 0) i = k + 1;
                } while (i <= j);

                if (i - 1 > j)
                {
                    tok.sy = Keywords.Keys[k].ksy;
                }
                else
                {
                    tok.sy = SYMBOL.identsy;
                }

                return true;
            }

            return false;
        }



        private int strcmp(char[] src, int len, string dest)
        {

            int i = 0;
            while (i < len && i < dest.Length)
            {
                char c = src[i];
                if (c >= 'a' && c <= 'z')
                    c = char.ToUpper(c);

                if (c < dest[i])
                    return -1;

                if (c > dest[i])
                    return 1;

                i++;
            }

            if (len > dest.Length)
                return 1;
            else if (len < dest.Length)
                return -1;

            //if (len != dest.Length)
            //    return (src[i] < dest[i]) ? -1 : 1;

            return 0;

        }

        #endregion


        #region GetNum(), GetString(), GetStringESC()

        private bool GetNum()
        {
            int k;
            if (ch >= '0' && ch <= '9')
            {  // integer
                int e;
                k = 0;
                tok.SetInt32(inum: 0);
                tok.sy = SYMBOL.intcon;

                do
                {
                    tok.SetInt32(inum: tok.inum * 10 + ch - '0');
                    k++;
                    NextCh();
                } while (ch >= '0' && ch <= '9');

                if (k > Constant.KMAX || tok.inum > Constant.NMAX)
                {
                    error.OnError(21);
                    tok.SetInt32(inum: 0);
                    k = 0;
                }

                // float     
                if (ch == '.')
                {
                    NextCh();
                    tok.sy = SYMBOL.floatcon;
                    tok.SetDouble(fnum: tok.inum);
                    e = 0;


                    while (ch >= '0' && ch <= '9')			// decimal .023410
                    {
                        e--;
                        tok.SetDouble(fnum: 10.0 * tok.fnum + ch - '0');
                        NextCh();
                    }
                    if (e == 0) error.OnError(40);
                    if (ch == 'e' || ch == 'E') ReadScale(ref e);
                    if (e != 0) AdjustScale(e, k);

                }
                else
                    if (ch == 'e' || ch == 'E') //expoent
                {
                    tok.sy = SYMBOL.floatcon;
                    tok.SetDouble(fnum: tok.inum);
                    e = 0;
                    ReadScale(ref e);
                    if (e != 0) AdjustScale(e, k);
                }

                return true;
            }
            return false;
        }

        private void GetString(char sep)
        {
            int k = 0;
            char[] stab = new char[Constant.MAX_STRING_SIZE + 1];

            NextCh();
            while (ch != sep)
            {
                if (k > Constant.MAX_STRING_SIZE)
                {
                    error.OnError(59);
                    while (ch != sep) NextCh();
                    break;
                }
                else
                {
                    if (ch == '\\')
                    {
                        NextCh();
                        if (ch == sep)
                            stab[k++] = ch;
                        else
                        {
                            switch (ch)
                            {
                                case '\\':
                                    stab[k++] = ch;
                                    break;

                                case 'n':
                                    stab[k++] = '\n';
                                    break;

                                case 'r':
                                    stab[k++] = '\r';
                                    break;

                                case 't':
                                    stab[k++] = '\t';
                                    break;

                                default:
                                    stab[k++] = '\\';
                                    stab[k++] = ch;
                                    break;
                            }
                        }
                    }
                    else
                        stab[k++] = ch;
                }
                NextCh();
            }

            NextCh();
            tok.sy = SYMBOL.stringcon;
            stab[k] = (char)0;
            tok.SetString(new string(stab, 0, k));
        }

        private void GetStringESC(char sep)
        {
            int k = 0;
            char[] stab = new char[Constant.MAX_STRING_SIZE + 1];

            NextCh();
            while (ch != sep)
            {
                if (k > Constant.MAX_STRING_SIZE)
                {
                    error.OnError(59);
                    while (ch != sep) NextCh();
                    break;
                }
                else
                    stab[k++] = ch;

                NextCh();
            }

            NextCh();
            tok.sy = SYMBOL.stringcon;
            stab[k] = (char)0;
            tok.SetString(new string(stab, 0, k));
        }

        #endregion

        public bool EOF()
        {
            return ch == 0;
        }

        public bool InSymbol()
        {

            //	if(!IsEmpty) tok=pending;

            L1:
            if (ch == 0)
            {
                tok.sy = SYMBOL.NOP;
                return false;
            }

            while (ch == ' ' || ch == '\t' || ch == '\n' || ch == (char)13)
            {
                NextCh();  //space & h-tab
                if (ch == 0)
                {
                    tok.sy = SYMBOL.NOP;
                    return false;
                }
            }


            // IDENT   
            if (GetKeyAndIdent()) return true;

            //Number
            if (GetNum()) return true;


            switch (ch)
            {
                //comment 
                case '/':
                    NextCh();
                    if (ch == '*')
                    {					// comment type II (/* */) suppor Nest comment
                        NextCh();
                        SkipComment();
                        goto L1;
                    }
                    else
                        tok.sy = SYMBOL.DIV;
                    break;

                //string	
                case '"':
                case '\'':
                    GetString(ch);
                    break;

                case '@':
                    NextCh();
                    if (ch == '"' || ch == '\'')
                        GetStringESC(ch);
                    else
                    {
                        error.OnError(24); NextCh();
                    }
                    break;

                //------------------------------------------------------------------------------------
                case '<':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.RELOP; tok.opr = SYMBOL2.LEQ; NextCh(); break;
                        case '<': tok.sy = SYMBOL.SHIFTOP; tok.opr = SYMBOL2.SHL; NextCh(); break;
                        default: tok.sy = SYMBOL.RELOP; tok.opr = SYMBOL2.LSS; break;
                    }
                    break;
                case '>':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.RELOP; tok.opr = SYMBOL2.GEQ; NextCh(); break;
                        case '>': tok.sy = SYMBOL.SHIFTOP; tok.opr = SYMBOL2.SHR; NextCh(); break;
                        default: tok.sy = SYMBOL.RELOP; tok.opr = SYMBOL2.GTR; break;
                    }
                    break;
                case '!':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.EQUOP; tok.opr = SYMBOL2.NEQ; NextCh(); break;
                        default: tok.sy = SYMBOL.UNOP; tok.opr = SYMBOL2.NOT; break;
                    }
                    break;

                case '|':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.ASSIGNOP; tok.opr = SYMBOL2.eOR; NextCh(); break;
                        default: tok.sy = SYMBOL.OR; break;
                    }
                    break;

                case '&':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.ASSIGNOP; tok.opr = SYMBOL2.eAND; NextCh(); break;
                        default: tok.sy = SYMBOL.AND; tok.opr = SYMBOL2.ADR; break;
                    }
                    break;
                case '=':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.EQUOP; tok.opr = SYMBOL2.EQL; NextCh(); break;
                        default: tok.sy = SYMBOL.EQUAL; break;
                    }
                    break;

                //------------------------------------------------------------------------------------
                case '+':
                    NextCh();
                    switch (ch)
                    {
                        case '+': tok.sy = SYMBOL.INCOP; tok.opr = SYMBOL2.PPLUS; NextCh(); break;
                        case '=': tok.sy = SYMBOL.ASSIGNOP; tok.opr = SYMBOL2.ePLUS; NextCh(); break;
                        default: tok.sy = SYMBOL.PLUS; tok.opr = SYMBOL2.NEG; break;
                    }
                    break;
                case '-':
                    NextCh();
                    switch (ch)
                    {
                        case '-':
                            while (ch != '\n' && ch != '\0')
                                NextCh();
                            goto L1;
                        case '=': tok.sy = SYMBOL.ASSIGNOP; tok.opr = SYMBOL2.eMINUS; NextCh(); break;
                        case '>': tok.sy = SYMBOL.STRUCTOP; tok.opr = SYMBOL2.ARROW; NextCh(); break;
                        default: tok.sy = SYMBOL.MINUS; tok.opr = SYMBOL2.NEG; break;
                    }
                    break;
                case '*':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.ASSIGNOP; tok.opr = SYMBOL2.eSTAR; NextCh(); break;
                        default: tok.sy = SYMBOL.STAR; tok.opr = SYMBOL2.VLU; break;
                    }
                    break;
                case '%':
                    NextCh();
                    switch (ch)
                    {
                        case '=': tok.sy = SYMBOL.ASSIGNOP; tok.opr = SYMBOL2.eMOD; NextCh(); break;
                        default: tok.sy = SYMBOL.MOD; break;
                    }
                    break;
                case ':':
                    NextCh();
                    switch (ch)
                    {
                        default: tok.sy = SYMBOL.COLON; break;
                    }
                    break;


                //------------------------------------------------------------------------------------

                case '(': tok.sy = SYMBOL.LP; NextCh(); break;
                case ')': tok.sy = SYMBOL.RP; NextCh(); break;
                case '[': tok.sy = SYMBOL.LB; NextCh(); break;
                case ']': tok.sy = SYMBOL.RB; NextCh(); break;
                case '{': tok.sy = SYMBOL.LC; NextCh(); break;
                case '}': tok.sy = SYMBOL.RC; NextCh(); break;

                case '?': tok.sy = SYMBOL.QUEST; NextCh(); break;
                case ',': tok.sy = SYMBOL.COMMA; NextCh(); break;
                case ';': tok.sy = SYMBOL.SEMI; NextCh(); break;
                case '.': tok.sy = SYMBOL.STRUCTOP; tok.opr = SYMBOL2.DOT; NextCh(); break;

                case '~': tok.sy = SYMBOL.UNOP; tok.opr = SYMBOL2.BNOT; NextCh(); break;


                default:
                    //cerr<<"error letter:"<<ch<<" has already skip";
                    error.OnError(24); NextCh();
                    goto L1;
            } // switch
            return true;
        }

        private void SkipComment()
        {

            int Nest = 0;

            L1:
            switch (ch)
            {
                case '*':
                    NextCh();
                    if (ch == '/')
                    {
                        NextCh();
                        if (Nest == 0) return;
                        else
                            Nest--;
                    }
                    break;

                case '/':
                    NextCh();
                    if (ch == '*')
                    { NextCh(); Nest++; }
                    break;

                default:
                    NextCh();
                    if (ch == 0) { error.OnError(60); return; }
                    break;
            }
            goto L1;
        }

        #region ReadScale(), AdjustScale()

        private void ReadScale(ref int e)
        {
            int s, sign;

            NextCh();
            sign = 1;
            s = 0;
            if (ch == '+')
                NextCh();
            else if (ch == '-')
            {
                NextCh();
                sign = -1;
            }

            if (!(ch >= '0' && ch <= '9'))
                error.OnError(40);
            else
                do
                {
                    s = 10 * s + ch - '0';
                    NextCh();
                } while (ch >= '0' && ch <= '9');

            e = s * sign + e;
            return;
        }

        private void AdjustScale(int e, int k)
        {
            int s;
            double d, t;

            if (k + e > Constant.EMAX)
                error.OnError(21);
            else if (k + e < Constant.EMIN)
                tok.SetDouble(fnum: 0);
            else
            {
                s = Math.Abs(e);
                t = 1.0;
                d = 10.0;

                do
                {
                    while (s % 2 == 0) { s /= 2; d = d * d; }
                    s--;
                    t = d * t;
                } while (s != 0);

                if (e >= 0)
                    tok.SetDouble(fnum: tok.fnum * t);
                else
                    tok.SetDouble(fnum: tok.fnum / t);
            }

        }

        #endregion

        public override string ToString()
        {
            return tok.ToString();
        }


        public SYMBOL sy => tok.sy;

        public SYMBOL2 opr => tok.opr;

        public Token token => this.tok;

        protected virtual void set_index(int index)
        {

        }

        public virtual int Index()
        {
            return -1;
        }

        public void Traceback(int index, Token token)
        {
            set_index(index);
            this.tok = token;
        }


        public bool InSymbol(int index)
        {
            set_index(index);
            return InSymbol();
        }
    }

}
