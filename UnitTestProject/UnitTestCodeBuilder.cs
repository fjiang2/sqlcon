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
    }
}
