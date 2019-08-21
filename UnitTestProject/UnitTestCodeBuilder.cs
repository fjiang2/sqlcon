using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            sent.ASSIGN("x", typeof(string[]), new Expression[] { "a", "b", "c" });
            string code = sent.ToString();
            Debug.Assert(code == "x = new string[]\r\n{\r\n\ta,\r\n\tb,\r\n\tc,\r\n};");

            sent = new Statement();
            sent.ASSIGN("x", typeof(List<string>), new Expression[] { "a", "b", "c" });
            code = sent.ToString();
            Debug.Assert(code == "x = new List<string>\r\n{\r\n\ta,\r\n\tb,\r\n\tc,\r\n};");

            sent = new Statement();
            sent.ASSIGN("x", typeof(List<string>), new Expression[] { });
            code = sent.ToString();
            Debug.Assert(code == "x = new List<string>();");

            sent = new Statement();
            sent.ASSIGN("x", typeof(List<int>), new Arguments(new Argument(1), new Argument(2)), new Expression[] { 3, 4, 5 });
            code = sent.ToString();
            Debug.Assert(code == "x = new List<int>(1, 2)\r\n{\r\n\t3,\r\n\t4,\r\n\t5,\r\n};");
        }
    }
}

