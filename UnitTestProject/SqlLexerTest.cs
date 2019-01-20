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
        public void TestMethod1()
        {
            string query = "SELECT TOP 1000 %%physloc%% AS [%%physloc%%],0 AS [%%RowId%%],* FROM Northwind.dbo.[Products]";
            Position pos = new Position("", query);
            JError error = new JError(pos);
            StringLex lex = new StringLex(query, error);

            while (lex.InSymbol())
            {
                Console.Write(lex.sy);
                Console.Write(lex.sym);

                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
