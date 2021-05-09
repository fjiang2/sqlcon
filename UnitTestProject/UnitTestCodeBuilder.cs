using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sys.CodeBuilder;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTestCodeBuilder
    {
        [TestMethod]
        public void TestClassTypeInfo()
        {
            TypeInfo dict = TypeInfo.Generic<int, string>(new TypeInfo("Dictionary"));
            string code = dict.ToString();
            Debug.Assert(code == "Dictionary<int, string>");

            dict = new TypeInfo(typeof(Dictionary<int, string>));
            code = dict.ToString();
            Debug.Assert(code == "Dictionary<int, string>");
        }

        [TestMethod]
        public void TestNewInstance()
        {
            Statement sent = new Statement();
            sent.ASSIGN("x", new New(typeof(string[]), new Expression[] { new Value("a"), new Value("b"), new Value("c") }));
            string code = sent.ToString();
            Debug.Assert(code == "x = new string[] { \"a\", \"b\", \"c\" };");

            sent = new Statement();
            sent.ASSIGN("x", new New(typeof(List<string>), new Expression[] { new Value("a"), new Value("b"), new Value("c") }));
            code = sent.ToString();
            Debug.Assert(code == "x = new List<string> { \"a\", \"b\", \"c\" };");

            sent = new Statement();
            sent.ASSIGN("x", new New(typeof(List<string>), new Arguments()));
            code = sent.ToString();
            Debug.Assert(code == "x = new List<string>();");

            sent = new Statement();
            sent.ASSIGN("x", new New(typeof(List<int>), new Arguments(new Argument(1), new Argument(2)), new Expression[] { 3, 4, 5 }));
            code = sent.ToString();
            Debug.Assert(code == "x = new List<int>(1, 2) { 3, 4, 5 };");

            sent = new Statement();
            var _string = new TypeInfo(typeof(string));
            var _args = new Arguments(new Argument("EmployeeID"), new Argument(_string));
            var _expr = new Expression[]
            {
               new Expression("Unique", true),
               new Expression("AllowDBNull", true),
               new Expression("MaxLength", 24),
            };
            sent.ASSIGN("x", new New(typeof(DataColumn), _args, _expr));
            code = sent.ToString();
            Debug.Assert(code == "x = new DataColumn(EmployeeID, typeof(string)) { Unique = true, AllowDBNull = true, MaxLength = 24 };");
        }
    }
}

