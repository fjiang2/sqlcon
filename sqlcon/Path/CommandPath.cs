using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sys.Data;
using Sys.Stdio;

namespace sqlcon
{
    class CommandPath
    {
        public static TableName[] GetTableNames(ApplicationCommand cmd, PathManager mgr)
        {
            var pt = mgr.current;
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                if (cmd.arg1 != null)
                {
                    PathName path = new PathName(cmd.arg1);
                    var node = mgr.Navigate(path);
                    if (node != null)
                    {
                        var dname = mgr.GetPathFrom<DatabaseName>(node);
                        if (dname != null)
                        {
                            if (cmd.wildcard != null)
                            {
                                var m = new MatchedDatabase(dname, cmd);
                                return m.TableNames();
                            }
                            else
                            {
                                var _tname = mgr.GetPathFrom<TableName>(node);
                                if (_tname != null)
                                    return new TableName[] { _tname };
                                else
                                {
                                    cerr.WriteLine("invalid path");
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            cerr.WriteLine("database is unavailable");
                            return null;
                        }
                    }
                    else
                    {
                        cerr.WriteLine("invalid path");
                        return null;
                    }
                }
            }


            if (pt.Item is TableName)
            {
                var tname = (TableName)pt.Item;
                return new TableName[] { tname };
            }

            return null;
        }
    }
}
