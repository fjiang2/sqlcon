using System;

namespace Sys.Data
{
    class SqlTokenizer
    {
        private string[] tokens;
        private int index = 0;

        public SqlTokenizer(string sql)
        {
            this.tokens = sql.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string GetNextToken()
        {
            if (index >= tokens.Length)
                return null;

            string tok = tokens[index];
            index++;
            return tok;
        }

        public bool ExpectInt32(out int result)
        {
            string token = GetNextToken();
            result = 0;
            if (token == null)
                return false;

            if (int.TryParse(token, out int value))
            {
                result = value;
                return true;
            }
            else
                return false;
        }
    }
}
