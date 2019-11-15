using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data.SqlParser;

namespace Sys.Data
{
    public class SqlCode
    {
        public List<SqlClause> Clauses { get; } = new List<SqlClause>();

        private Position pos;
        private Error error;
        private StringLex lex;

        public SqlCode(string sql)
        {
            this.pos = new Position("unknown sql module", sql);
            this.error = new Error(pos);
            this.lex = new StringLex(sql, error);
        }

        private bool expect(SYMBOL sy)
        {
            if (lex.sy == sy)
            {
                lex.InSymbol();
                return true;
            }
            else
            {
                error.OnError(sy); // add code throw a exception
                return false;
            }
        }

        private Token token => lex.token;

        private void Next()
        {
            lex.InSymbol();
        }

        public void Parse()
        {
            while (!lex.EOF())
            {
                Next();

                switch (token.sy)
                {
                    case SYMBOL.SELECT:
                        Next();
                        Clauses.Add(ParseSelect());
                        break;

                    case SYMBOL.UPDATE:
                        Next();
                        break;

                    case SYMBOL.DELETE:
                        Next();
                        break;

                    case SYMBOL.INSERT:
                        Next();
                        break;
                }
            }
        }

        private SelectClause ParseSelect()
        {
            var select = new SelectClause();

            if (token.sy == SYMBOL.TOP)
            {
                Next();
                if (token.sy == SYMBOL.intcon)
                    select.Top = token.inum;
                else
                    error.OnError(SYMBOL.intcon);
            }

            while (token.sy != SYMBOL.FROM)
            {
                ColumnDescriptor descriptor = new ColumnDescriptor();
                descriptor.ColumnName = ParseColumnName();
                if (token.sy == SYMBOL.AS)
                    descriptor.ColumnCaption = ParseColumnName();
                select.Descriptors.Add(descriptor);

                if (token.sy != SYMBOL.COMMA)
                    break;
            }

            Next();


            return select;
        }

        private string ParseColumnName()
        {
            Next();
            string name = string.Empty;
            bool expectRB = false;
            if (token.sy == SYMBOL.LB)
            {
                expectRB = true;
                Next();
            }

            if (token.sy == SYMBOL.MOD)
            {
                Next();
                expect(SYMBOL.MOD);
                name += "%%";
            }

            if (token.sy == SYMBOL.identsy)
            {
                name += token.id;
                lex.InSymbol();
            }

            if (token.sy == SYMBOL.MOD)
            {
                Next();
                expect(SYMBOL.MOD);
                name += "%%";
            }

            if (expectRB)
                expect(SYMBOL.RB);

            return name;
        }
    }
}
