﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.SqlParser
{
    class Keywords
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
    }
}
