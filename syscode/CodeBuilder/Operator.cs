//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//

namespace Sys.CodeBuilder
{
    /// <summary>
    /// public static Expression operator >(Expression exp1, Expression exp2)
    /// {
    ///    return new Expression($"{exp1} > {exp2}");
    /// }
    /// </summary>
    public class Operator : Member, IBuildable
    {

        /// <summary>
        /// Binary Operator
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="operation"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Operator(TypeInfo returnType, Operation operation, Parameter p1, Parameter p2)
            : base("operator " + ToCodeString(operation))
        {
            base.Modifier = Modifier.Public | Modifier.Static;
            base.Type = returnType;
            Params.Add(p1);
            Params.Add(p2);
        }

        /// <summary>
        /// Unary operator
        /// </summary>
        /// <param name="returnType"></param>
        /// <param name="operation"></param>
        /// <param name="p"></param>
        public Operator(TypeInfo returnType, Operation operation, Parameter p)
            : base("operator " + ToCodeString(operation))
        {
            base.Modifier = Modifier.Public | Modifier.Static;
            base.Type = returnType;
            Params.Add(p);
        }

        protected override string signature => $"{Signature}({Params})";

        private static string ToCodeString(Operation opr)
        {
            switch (opr)
            {
                case Operation.Plus: return "+";
                case Operation.Minus: return "-";
                case Operation.Multiple: return "*";
                case Operation.Divide: return "/";

                case Operation.GT: return ">";
                case Operation.GE: return ">=";
                case Operation.LT: return "<";
                case Operation.LE: return "<=";
                case Operation.NE: return "!=";
                case Operation.EQ: return "==";

                case Operation.NOT: return "!";

                default:
                    return string.Empty;
            }
        }
    }

    public enum Operation
    {
        Plus, Minus, Multiple, Divide,
        GT, GE, LT, LE, NE, EQ,
        NOT
    }
}
