using System;
using System.Linq;
using Tie;

namespace Sys.Data
{
    class SqlTokenizer
    {
        private token[] tokens;
        private int index = 0;

        public SqlTokenizer(string sql)
        {
            this.tokens = Script.Tokenize(sql).ToArray();
        }


        public bool EndOfToken => index >= tokens.Length;
        public token GetNextToken()
        {
            var tok = tokens[index];
            index++;
            return tok;
        }

        public bool ExpectInt32(out int result)
        {
            result = 0;

            if (EndOfToken)
                return false;

            var token = GetNextToken();
            if (token.ty != tokty.number)
                return false;

            if (int.TryParse(token.tok, out int value))
            {
                result = value;
                return true;
            }
            else
                return false;
        }
    }
}
