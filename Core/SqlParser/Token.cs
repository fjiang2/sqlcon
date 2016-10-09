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

namespace Sys.SqlParser
{
    /// <summary>
    /// Token Type used on Tokenizer
    /// </summary>
    public enum tokty
    {
        /// <summary>
        /// number is int, double, float,...
        /// </summary>
        number,

        /// <summary>
        /// like variable name
        /// </summary>
        identsy,

        /// <summary>
        /// string constant
        /// </summary>
        stringcon,

        /// <summary>
        /// symbol like: +,-,++,>=
        /// </summary>
        symbol,

        /// <summary>
        /// reserved keywords in c/c++
        /// </summary>
        keyword
    }

    /// <summary>
    /// define a token 
    /// </summary>
    public struct token
    {
        /// <summary>
        /// token type
        /// </summary>
        public readonly tokty ty;

        /// <summary>
        /// token itself
        /// </summary>
        public readonly string tok;

        internal token(string tok, tokty ty)
        {
            this.ty = ty;
            this.tok = tok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", ty, tok);
        }
    }



    // a Token
    enum SYMBOL
    {
        intcon, floatcon, boolcon, stringcon, identsy,      // constance number 31,3.14,'c',"STRING"
        nullsy, truesy, falsesy,

        PLUS, MINUS, STAR, DIV, MOD,                        // + - * / %
        EQUAL,
        RELOP, EQUOP, ASSIGNOP, INCOP, SHIFTOP, STRUCTOP, UNOP,

        LP, RP, LB, RB, LC, RC,                     // (	)	[	]	{	}
        COMMA, SEMI, QUEST, COLON,

        ADD,
        ALL,
        ALTER,
        AND,
        ANY,
        AS,
        ASC,
        AUTHORIZATION,
        BACKUP,
        BEGIN,
        BETWEEN,
        BREAK,
        BROWSE,
        BULK,
        BY,
        CASCADE,
        CASE,
        CHECK,
        CHECKPOINT,
        CLOSE,
        CLUSTERED,
        COALESCE,
        COLLATE,
        COLUMN,
        COMMIT,
        COMPUTE,
        CONSTRAINT,
        CONTAINS,
        CONTAINSTABLE,
        CONTINUE,
        CONVERT,
        CREATE,
        CROSS,
        CURRENT,
        CURRENT_DATE,
        CURRENT_TIME,
        CURRENT_TIMESTAMP,
        CURRENT_USER,
        CURSOR,
        DATABASE,
        DBCC,
        DEALLOCATE,
        DECLARE,
        DEFAULT,
        DELETE,
        DENY,
        DESC,
        DISK,
        DISTINCT,
        DISTRIBUTED,
        DOUBLE,
        DROP,
        DUMP,
        ELSE,
        END,
        ERRLVL,
        ESCAPE,
        EXCEPT,
        EXEC,
        EXECUTE,
        EXISTS,
        EXIT,
        EXTERNAL,
        FETCH,
        FILE,
        FILLFACTOR,
        FOR,
        FOREIGN,
        FREETEXT,
        FREETEXTTABLE,
        FROM,
        FULL,
        FUNCTION,
        GOTO,
        GRANT,
        GROUP,
        HAVING,
        HOLDLOCK,
        IDENTITY,
        IDENTITY_INSERT,
        IDENTITYCOL,
        IF,
        IN,
        INDEX,
        INNER,
        INSERT,
        INTERSECT,
        INTO,
        IS,
        JOIN,
        KEY,
        KILL,
        LEFT,
        LIKE,
        LINENO,
        LOAD,
        MERGE,
        NATIONAL,
        NOCHECK,
        NONCLUSTERED,
        NOT,
        NULL,
        NULLIF,
        OF,
        OFF,
        OFFSETS,
        ON,
        OPEN,
        OPENDATASOURCE,
        OPENQUERY,
        OPENROWSET,
        OPENXML,
        OPTION,
        OR,
        ORDER,
        OUTER,
        OVER,
        PERCENT,
        PIVOT,
        PLAN,
        PRECISION,
        PRIMARY,
        PRINT,
        PROC,
        PROCEDURE,
        PUBLIC,
        RAISERROR,
        READ,
        READTEXT,
        RECONFIGURE,
        REFERENCES,
        REPLICATION,
        RESTORE,
        RESTRICT,
        RETURN,
        REVERT,
        REVOKE,
        RIGHT,
        ROLLBACK,
        ROWCOUNT,
        ROWGUIDCOL,
        RULE,
        SAVE,
        SCHEMA,
        SECURITYAUDIT,
        SELECT,
        SEMANTICKEYPHRASETABLE,
        SEMANTICSIMILARITYDETAILSTABLE,
        SEMANTICSIMILARITYTABLE,
        SESSION_USER,
        SET,
        SETUSER,
        SHUTDOWN,
        SOME,
        STATISTICS,
        SYSTEM_USER,
        TABLE,
        TABLESAMPLE,
        TEXTSIZE,
        THEN,
        TO,
        TOP,
        TRAN,
        TRANSACTION,
        TRIGGER,
        TRUNCATE,
        TRY_CONVERT,
        TSEQUAL,
        UNION,
        UNIQUE,
        UNPIVOT,
        UPDATE,
        UPDATETEXT,
        USE,
        USER,
        VALUES,
        VARYING,
        VIEW,
        WAITFOR,
        WHEN,
        WHERE,
        WHILE,
        WITH,
        WITHIN,
        WRITETEXT,


        NOP     //最后一个token

    }

    enum SYMBOL2
    {
        EQL, NEQ,
        GTR, GEQ, LSS, LEQ,
        ADR, VLU,                    // &var, *adr

        ePLUS, eMINUS, eSTAR, eDIV, eMOD,       // +=
        eSHR, eSHL,
        eAND, eOR, eXOR,

        NOT, BNOT, NEG,

        PPLUS, MMINUS,                                  // ++, --
        SHL, SHR,                                       // <<,	>>
        DOT, ARROW
    }								// .	->

    class Sym
    {
        public double fnum;				// real number from insymbol 
        public int inum;				// integer from insymbol 
        public string id;

        public int len;			        // string length 
        public string stab;		        // string table


        public Sym()
        {

        }
    };


    class Token
    {

        public SYMBOL sy;
        public Sym sym;
        public SYMBOL2 opr;


        public Token()
        {
            sym = new Sym();

        }

        public Token(SYMBOL sy, SYMBOL2 opr)
            : this()
        {
            this.sy = sy;
            this.opr = opr;
        }


    }




}
