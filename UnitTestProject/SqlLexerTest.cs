using System;
using System.Collections.Generic;
using System.Linq;
using Sys.Data;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sys.Data.SqlParser;

namespace UnitTestProject
{
    [TestClass]
    public class SqlLexerTest
    {
        const string query = @"
--Notes
Select TOP 1000 
%%physloc%% AS [%%physloc%%],
0 AS [%%RowId%%],
* 
FROM 
Northwind.dbo.[Products]";

        [TestMethod]
        public void TestSelectClause1()
        {

            Position pos = new Position("select", query);
            Error error = new Error(pos);
            StringLex lex = new StringLex(query, error);

            List<string> tokens = new List<string>();
            while (!lex.EOF())
            {
                lex.InSymbol();

                tokens.Add(lex.token.ToString());
            }

            string[] L = tokens.ToArray();

            string code = string.Join("|", L);
            string result = "SELECT|TOP|1000|%|%|physloc|%|%|AS|[|%|%|physloc|%|%|]|,|0|AS|[|%|%|RowId|%|%|]|,|*|FROM|Northwind|.|dbo|.|[|Products|]";

            Debug.Assert(code == result, "error");
        }

        [TestMethod]
        public void TestParse1()
        {
            SqlCode code = new SqlCode(query);
            code.Parse();


        }
    }
}
