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


    class JLex
    {
        public readonly static JKey[] Keys = new JKey[]
            {
                new JKey("ADD",SYMBOL.ADD),
                new JKey("ALL",SYMBOL.ALL),
                new JKey("ALTER",SYMBOL.ALTER),
                new JKey("AND",SYMBOL.AND),
                new JKey("ANY",SYMBOL.ANY),
                new JKey("AS",SYMBOL.AS),
                new JKey("ASC",SYMBOL.ASC),
                new JKey("AUTHORIZATION",SYMBOL.AUTHORIZATION),
                new JKey("BACKUP",SYMBOL.BACKUP),
                new JKey("BEGIN",SYMBOL.BEGIN),
                new JKey("BETWEEN",SYMBOL.BETWEEN),
                new JKey("BREAK",SYMBOL.BREAK),
                new JKey("BROWSE",SYMBOL.BROWSE),
                new JKey("BULK",SYMBOL.BULK),
                new JKey("BY",SYMBOL.BY),
                new JKey("CASCADE",SYMBOL.CASCADE),
                new JKey("CASE",SYMBOL.CASE),
                new JKey("CHECK",SYMBOL.CHECK),
                new JKey("CHECKPOINT",SYMBOL.CHECKPOINT),
                new JKey("CLOSE",SYMBOL.CLOSE),
                new JKey("CLUSTERED",SYMBOL.CLUSTERED),
                new JKey("COALESCE",SYMBOL.COALESCE),
                new JKey("COLLATE",SYMBOL.COLLATE),
                new JKey("COLUMN",SYMBOL.COLUMN),
                new JKey("COMMIT",SYMBOL.COMMIT),
                new JKey("COMPUTE",SYMBOL.COMPUTE),
                new JKey("CONSTRAINT",SYMBOL.CONSTRAINT),
                new JKey("CONTAINS",SYMBOL.CONTAINS),
                new JKey("CONTAINSTABLE",SYMBOL.CONTAINSTABLE),
                new JKey("CONTINUE",SYMBOL.CONTINUE),
                new JKey("CONVERT",SYMBOL.CONVERT),
                new JKey("CREATE",SYMBOL.CREATE),
                new JKey("CROSS",SYMBOL.CROSS),
                new JKey("CURRENT",SYMBOL.CURRENT),
                new JKey("CURRENT_DATE",SYMBOL.CURRENT_DATE),
                new JKey("CURRENT_TIME",SYMBOL.CURRENT_TIME),
                new JKey("CURRENT_TIMESTAMP",SYMBOL.CURRENT_TIMESTAMP),
                new JKey("CURRENT_USER",SYMBOL.CURRENT_USER),
                new JKey("CURSOR",SYMBOL.CURSOR),
                new JKey("DATABASE",SYMBOL.DATABASE),
                new JKey("DBCC",SYMBOL.DBCC),
                new JKey("DEALLOCATE",SYMBOL.DEALLOCATE),
                new JKey("DECLARE",SYMBOL.DECLARE),
                new JKey("DEFAULT",SYMBOL.DEFAULT),
                new JKey("DELETE",SYMBOL.DELETE),
                new JKey("DENY",SYMBOL.DENY),
                new JKey("DESC",SYMBOL.DESC),
                new JKey("DISK",SYMBOL.DISK),
                new JKey("DISTINCT",SYMBOL.DISTINCT),
                new JKey("DISTRIBUTED",SYMBOL.DISTRIBUTED),
                new JKey("DOUBLE",SYMBOL.DOUBLE),
                new JKey("DROP",SYMBOL.DROP),
                new JKey("DUMP",SYMBOL.DUMP),
                new JKey("ELSE",SYMBOL.ELSE),
                new JKey("END",SYMBOL.END),
                new JKey("ERRLVL",SYMBOL.ERRLVL),
                new JKey("ESCAPE",SYMBOL.ESCAPE),
                new JKey("EXCEPT",SYMBOL.EXCEPT),
                new JKey("EXEC",SYMBOL.EXEC),
                new JKey("EXECUTE",SYMBOL.EXECUTE),
                new JKey("EXISTS",SYMBOL.EXISTS),
                new JKey("EXIT",SYMBOL.EXIT),
                new JKey("EXTERNAL",SYMBOL.EXTERNAL),
                new JKey("FETCH",SYMBOL.FETCH),
                new JKey("FILE",SYMBOL.FILE),
                new JKey("FILLFACTOR",SYMBOL.FILLFACTOR),
                new JKey("FOR",SYMBOL.FOR),
                new JKey("FOREIGN",SYMBOL.FOREIGN),
                new JKey("FREETEXT",SYMBOL.FREETEXT),
                new JKey("FREETEXTTABLE",SYMBOL.FREETEXTTABLE),
                new JKey("FROM",SYMBOL.FROM),
                new JKey("FULL",SYMBOL.FULL),
                new JKey("FUNCTION",SYMBOL.FUNCTION),
                new JKey("GOTO",SYMBOL.GOTO),
                new JKey("GRANT",SYMBOL.GRANT),
                new JKey("GROUP",SYMBOL.GROUP),
                new JKey("HAVING",SYMBOL.HAVING),
                new JKey("HOLDLOCK",SYMBOL.HOLDLOCK),
                new JKey("IDENTITY",SYMBOL.IDENTITY),
                new JKey("IDENTITY_INSERT",SYMBOL.IDENTITY_INSERT),
                new JKey("IDENTITYCOL",SYMBOL.IDENTITYCOL),
                new JKey("IF",SYMBOL.IF),
                new JKey("IN",SYMBOL.IN),
                new JKey("INDEX",SYMBOL.INDEX),
                new JKey("INNER",SYMBOL.INNER),
                new JKey("INSERT",SYMBOL.INSERT),
                new JKey("INTERSECT",SYMBOL.INTERSECT),
                new JKey("INTO",SYMBOL.INTO),
                new JKey("IS",SYMBOL.IS),
                new JKey("JOIN",SYMBOL.JOIN),
                new JKey("KEY",SYMBOL.KEY),
                new JKey("KILL",SYMBOL.KILL),
                new JKey("LEFT",SYMBOL.LEFT),
                new JKey("LIKE",SYMBOL.LIKE),
                new JKey("LINENO",SYMBOL.LINENO),
                new JKey("LOAD",SYMBOL.LOAD),
                new JKey("MERGE",SYMBOL.MERGE),
                new JKey("NATIONAL",SYMBOL.NATIONAL),
                new JKey("NOCHECK",SYMBOL.NOCHECK),
                new JKey("NONCLUSTERED",SYMBOL.NONCLUSTERED),
                new JKey("NOT",SYMBOL.NOT),
                new JKey("NULL",SYMBOL.NULL),
                new JKey("NULLIF",SYMBOL.NULLIF),
                new JKey("OF",SYMBOL.OF),
                new JKey("OFF",SYMBOL.OFF),
                new JKey("OFFSETS",SYMBOL.OFFSETS),
                new JKey("ON",SYMBOL.ON),
                new JKey("OPEN",SYMBOL.OPEN),
                new JKey("OPENDATASOURCE",SYMBOL.OPENDATASOURCE),
                new JKey("OPENQUERY",SYMBOL.OPENQUERY),
                new JKey("OPENROWSET",SYMBOL.OPENROWSET),
                new JKey("OPENXML",SYMBOL.OPENXML),
                new JKey("OPTION",SYMBOL.OPTION),
                new JKey("OR",SYMBOL.OR),
                new JKey("ORDER",SYMBOL.ORDER),
                new JKey("OUTER",SYMBOL.OUTER),
                new JKey("OVER",SYMBOL.OVER),
                new JKey("PERCENT",SYMBOL.PERCENT),
                new JKey("PIVOT",SYMBOL.PIVOT),
                new JKey("PLAN",SYMBOL.PLAN),
                new JKey("PRECISION",SYMBOL.PRECISION),
                new JKey("PRIMARY",SYMBOL.PRIMARY),
                new JKey("PRINT",SYMBOL.PRINT),
                new JKey("PROC",SYMBOL.PROC),
                new JKey("PROCEDURE",SYMBOL.PROCEDURE),
                new JKey("PUBLIC",SYMBOL.PUBLIC),
                new JKey("RAISERROR",SYMBOL.RAISERROR),
                new JKey("READ",SYMBOL.READ),
                new JKey("READTEXT",SYMBOL.READTEXT),
                new JKey("RECONFIGURE",SYMBOL.RECONFIGURE),
                new JKey("REFERENCES",SYMBOL.REFERENCES),
                new JKey("REPLICATION",SYMBOL.REPLICATION),
                new JKey("RESTORE",SYMBOL.RESTORE),
                new JKey("RESTRICT",SYMBOL.RESTRICT),
                new JKey("RETURN",SYMBOL.RETURN),
                new JKey("REVERT",SYMBOL.REVERT),
                new JKey("REVOKE",SYMBOL.REVOKE),
                new JKey("RIGHT",SYMBOL.RIGHT),
                new JKey("ROLLBACK",SYMBOL.ROLLBACK),
                new JKey("ROWCOUNT",SYMBOL.ROWCOUNT),
                new JKey("ROWGUIDCOL",SYMBOL.ROWGUIDCOL),
                new JKey("RULE",SYMBOL.RULE),
                new JKey("SAVE",SYMBOL.SAVE),
                new JKey("SCHEMA",SYMBOL.SCHEMA),
                new JKey("SECURITYAUDIT",SYMBOL.SECURITYAUDIT),
                new JKey("SELECT",SYMBOL.SELECT),
                new JKey("SEMANTICKEYPHRASETABLE",SYMBOL.SEMANTICKEYPHRASETABLE),
                new JKey("SEMANTICSIMILARITYDETAILSTABLE",SYMBOL.SEMANTICSIMILARITYDETAILSTABLE),
                new JKey("SEMANTICSIMILARITYTABLE",SYMBOL.SEMANTICSIMILARITYTABLE),
                new JKey("SESSION_USER",SYMBOL.SESSION_USER),
                new JKey("SET",SYMBOL.SET),
                new JKey("SETUSER",SYMBOL.SETUSER),
                new JKey("SHUTDOWN",SYMBOL.SHUTDOWN),
                new JKey("SOME",SYMBOL.SOME),
                new JKey("STATISTICS",SYMBOL.STATISTICS),
                new JKey("SYSTEM_USER",SYMBOL.SYSTEM_USER),
                new JKey("TABLE",SYMBOL.TABLE),
                new JKey("TABLESAMPLE",SYMBOL.TABLESAMPLE),
                new JKey("TEXTSIZE",SYMBOL.TEXTSIZE),
                new JKey("THEN",SYMBOL.THEN),
                new JKey("TO",SYMBOL.TO),
                new JKey("TOP",SYMBOL.TOP),
                new JKey("TRAN",SYMBOL.TRAN),
                new JKey("TRANSACTION",SYMBOL.TRANSACTION),
                new JKey("TRIGGER",SYMBOL.TRIGGER),
                new JKey("TRUNCATE",SYMBOL.TRUNCATE),
                new JKey("TRY_CONVERT",SYMBOL.TRY_CONVERT),
                new JKey("TSEQUAL",SYMBOL.TSEQUAL),
                new JKey("UNION",SYMBOL.UNION),
                new JKey("UNIQUE",SYMBOL.UNIQUE),
                new JKey("UNPIVOT",SYMBOL.UNPIVOT),
                new JKey("UPDATE",SYMBOL.UPDATE),
                new JKey("UPDATETEXT",SYMBOL.UPDATETEXT),
                new JKey("USE",SYMBOL.USE),
                new JKey("USER",SYMBOL.USER),
                new JKey("VALUES",SYMBOL.VALUES),
                new JKey("VARYING",SYMBOL.VARYING),
                new JKey("VIEW",SYMBOL.VIEW),
                new JKey("WAITFOR",SYMBOL.WAITFOR),
                new JKey("WHEN",SYMBOL.WHEN),
                new JKey("WHERE",SYMBOL.WHERE),
                new JKey("WHILE",SYMBOL.WHILE),
                new JKey("WITH",SYMBOL.WITH),
                new JKey("WITHIN",SYMBOL.WITHIN),
                new JKey("WRITETEXT",SYMBOL.WRITETEXT)
            };

        protected char ch;

        private JToken tok;
        private JError error;			//the positon of cursor in file

        public JLex(JError err)
        {
            this.error = err;

            tok = new JToken();


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
                        || ch == '`'  //为了支持Generic类型, typeof(Dictionary<,>).Name == "Dictionary`2"
                    );

                tok.sym.len = k;
                tok.sym.id = new String(ident, 0, k);

                i = 0; j = Constant.NKW - 1;  //binary search
                do
                {
                    k = (i + j) / 2;
                    if (strcmp(ident, tok.sym.len, Keys[k].key) <= 0) j = k - 1;
                    if (strcmp(ident, tok.sym.len, Keys[k].key) >= 0) i = k + 1;
                } while (i <= j);

                if (i - 1 > j)
                {
                    tok.sy = Keys[k].ksy;
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
                tok.sym.inum = 0;
                tok.sy = SYMBOL.intcon;

                do
                {
                    tok.sym.inum = tok.sym.inum * 10 + ch - '0';
                    k++;
                    NextCh();
                } while (ch >= '0' && ch <= '9');

                if (k > Constant.KMAX || tok.sym.inum > Constant.NMAX)
                {
                    error.OnError(21);
                    tok.sym.inum = 0;
                    k = 0;
                }

                // float     
                if (ch == '.')
                {
                    NextCh();
                    tok.sy = SYMBOL.floatcon;
                    tok.sym.fnum = tok.sym.inum;
                    e = 0;


                    while (ch >= '0' && ch <= '9')			// decimal .023410
                    {
                        e--;
                        tok.sym.fnum = 10.0 * tok.sym.fnum + ch - '0';
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
                    tok.sym.fnum = tok.sym.inum;
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
            tok.sym.len = k;

            tok.sym.stab = new String(stab, 0, k);
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
            tok.sym.len = k;

            tok.sym.stab = new String(stab, 0, k);
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
                tok.sym.fnum = 0;
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
                    tok.sym.fnum = tok.sym.fnum * t;
                else tok.sym.fnum = tok.sym.fnum / t;
            }

        }

        #endregion

        public override string ToString()
        {
            return tok.ToString();
        }


        public SYMBOL sy
        {
            get
            {
                return tok.sy;
            }
        }

        public Sym sym
        {

            get
            {
                return tok.sym;
            }
        }

        public SYMBOL2 opr
        {

            get
            {
                return tok.opr;
            }
        }

        public JToken token
        {
            get { return this.tok; }
        }

        protected virtual void set_index(int index)
        {

        }

        public virtual int Index()
        {
            return -1;
        }

        public void Traceback(int index, JToken token)
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
