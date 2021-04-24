using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sys.Data
{
    class SqlScriptReader : IDisposable
    {
        private StreamReader reader;
        private int i = 0;
        private string line;

        public SqlScriptReader(string file)
        {
            this.reader = new StreamReader(file);
        }

        public void Dispose()
        {
            Close();
        }

        private bool ReadLine()
        {
            if (!reader.EndOfStream)
            {
                i++;
                line =  reader.ReadLine();
                return true;
            }

            return false;
        }

        public bool NextLine()
        {
            while (ReadLine())
            {
                string formatedLine = line.Trim();
                if (formatedLine == string.Empty || formatedLine.StartsWith("--"))
                    continue;
                else if (formatedLine.StartsWith("/*"))
                {
                    while (ReadLine())
                    {
                        formatedLine = line.Trim();
                        if (formatedLine.EndsWith("*/"))
                            break;
                    }
                }
                else
                    return true;
            }

            return false;
        }

        public string Line { get { return this.line; } }

        public int LineNumber { get { return i -1 ; } }

        public void Close()
        {
            reader.Close();
        }
    }
}
