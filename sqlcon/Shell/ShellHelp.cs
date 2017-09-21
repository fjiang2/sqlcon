using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlcon
{
    partial class Shell
    {

        private static void Help()
        {
            stdio.WriteLine("Path points to server, database,tables, data rows");
            stdio.WriteLine(@"      \server\database\table\filter\filter\....");
            stdio.WriteLine("Notes: table names support wildcard matching, e.g. Prod*,Pro?ucts");
            stdio.WriteLine("exit                    : quit application");
            stdio.WriteLine("help                    : this help");
            stdio.WriteLine("?                       : this help");
            stdio.WriteLine("cls                     : clears the screen");
            stdio.WriteLine("dir,ls /?               : see more info");
            stdio.WriteLine("cd,chdir /?             : see more info");
            stdio.WriteLine("lcd [path]              : change or display current directory");
            stdio.WriteLine("ldir [path]             : display files");
            stdio.WriteLine("md,mkdir /?             : see more info");
            stdio.WriteLine("rd,rmdir /?             : see more info");
            stdio.WriteLine("type /?                 : see more info");
            stdio.WriteLine("set /?                  : see more info");
            stdio.WriteLine("let /?                  : see more info");
            stdio.WriteLine("del,erase /?            : see more info");
            stdio.WriteLine("ren,rename /?           : see more info");
            stdio.WriteLine("attrib /?               : see more info");
            stdio.WriteLine("copy /?                 : see more info");
            stdio.WriteLine("comp /?                 : see more info");
            stdio.WriteLine("xcopy /?                : see more info");
            stdio.WriteLine("echo /?                 : see more info");
            stdio.WriteLine("run [drive:][path]file  : run a batch program (.sqc)");
            stdio.WriteLine("call [drive:][path]file : call Tie program (.sqt)");
            stdio.WriteLine("rem                     : records comments/remarks");
            stdio.WriteLine("ver                     : display version");
            stdio.WriteLine("compare path1 [path2]   : compare table scheam or data");
            stdio.WriteLine("          /s            : compare schema otherwise compare data");
            stdio.WriteLine("          /e            : compare common existing tables only");
            stdio.WriteLine("          /col:c1,c2    : skip columns defined during comparing");
            stdio.WriteLine("export /?               : see more info");
            stdio.WriteLine("import /?               : see more info");
            stdio.WriteLine("clean /?                : see more info");
            stdio.WriteLine("mount /?                : see more info");
            stdio.WriteLine("umount /?               : see more info");
            stdio.WriteLine("open /?                 : see more info");
            stdio.WriteLine("save /?                 : see more info");
            stdio.WriteLine("execute /?              : see more info");
            stdio.WriteLine("edit /?                 : see more info");
            stdio.WriteLine("last                    : display last result");
            stdio.WriteLine();
            stdio.WriteLine("<Commands>");
            stdio.WriteLine("<find> pattern          : find table name or column name");
            stdio.WriteLine("<show view>             : show all views");
            stdio.WriteLine("<show proc>             : show all stored proc and func");
            stdio.WriteLine("<show index>            : show all indices");
            stdio.WriteLine("<show vw> viewnames     : show view structure");
            stdio.WriteLine("<show pk>               : show all tables with primary keys");
            stdio.WriteLine("<show npk>              : show all tables without primary keys");
            stdio.WriteLine("<show connection>       : show connection-string list");
            stdio.WriteLine("<show current>          : show current active connection-string");
            stdio.WriteLine("<show var>              : show variable list");
            stdio.WriteLine("<sync table1 table2>    : synchronize, make table2 is the same as table1");
            stdio.WriteLine();
            stdio.WriteLine("type [;] to execute following SQL script or functions");
            stdio.WriteLine("<SQL>");
            stdio.WriteLine("select ... from table where ...");
            stdio.WriteLine("update table set ... where ...");
            stdio.WriteLine("delete from table where...");
            stdio.WriteLine("create table ...");
            stdio.WriteLine("drop table ...");
            stdio.WriteLine("alter ...");
            stdio.WriteLine("exec ...");
            stdio.WriteLine("<Variables>");
            stdio.WriteLine("  maxrows               : max number of row shown on select query");
            stdio.WriteLine("  DataReader            : true: use SqlDataReader; false: use Fill DataSet");
            stdio.WriteLine();
        }
    }
}
