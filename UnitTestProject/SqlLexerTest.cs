using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Sys.Data;
using Sys.Data.SqlParser;

namespace UnitTestProject
{
    [TestClass]
    public class SqlLexerTest
    {
        [TestMethod]
        public void TestSelectClause1()
        {
            string query = "SELECT TOP 1000 %%physloc%% AS [%%physloc%%],0 AS [%%RowId%%],* FROM Northwind.dbo.[Products]";

            Position pos = new Position("select", query);
            Error error = new Error(pos);
            StringLex lex = new StringLex(query, error);

            while (!lex.EOF())
            {
                lex.InSymbol();

                Console.Write(lex.sy);
                Console.Write(lex.token);

                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
