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
            cout.WriteLine("Path points to server, database,tables, data rows");
            cout.WriteLine(@"      \server\database\table\filter\filter\....");
            cout.WriteLine("Notes: table names support wildcard matching, e.g. Prod*,Pro?ucts");
            cout.WriteLine("exit                    : quit application");
            cout.WriteLine("help                    : this help");
            cout.WriteLine("?                       : this help");
            cout.WriteLine("rem                     : comments or remarks");
            cout.WriteLine("ver                     : display version");
            cout.WriteLine("cls                     : clears the screen");
            cout.WriteLine("echo /?                 : see more info");
            cout.WriteLine("dir,ls /?               : see more info");
            cout.WriteLine("cd,chdir /?             : see more info");
            cout.WriteLine("md,mkdir /?             : see more info");
            cout.WriteLine("rd,rmdir /?             : see more info");
            cout.WriteLine("type /?                 : see more info");
            cout.WriteLine("set /?                  : see more info");
            cout.WriteLine("let /?                  : see more info");
            cout.WriteLine("del,erase /?            : see more info");
            cout.WriteLine("ren,rename /?           : see more info");
            cout.WriteLine("attrib /?               : see more info");
            cout.WriteLine("copy /?                 : see more info");
            cout.WriteLine("xcopy /?                : see more info");
            cout.WriteLine("comp /?                 : see more info");
            cout.WriteLine("compare path1 [path2]   : compare table scheam or data");
            cout.WriteLine("          /s            : compare schema, otherwise compare data");
            cout.WriteLine("          /e            : compare common existing tables only");
            cout.WriteLine("          /col:c1,c2    : skip columns defined during comparing");
            cout.WriteLine("sync table1 table2      : synchronize, make table2 is the same as table1");
            cout.WriteLine("export /?               : see more info");
            cout.WriteLine("import /?               : see more info");
            cout.WriteLine("clean /?                : see more info");
            cout.WriteLine("mount /?                : see more info");
            cout.WriteLine("umount /?               : see more info");
            cout.WriteLine("open /?                 : see more info");
            cout.WriteLine("save /?                 : see more info");
            cout.WriteLine("edit /?                 : see more info");
            cout.WriteLine("chk,check /?            : see more info");
            cout.WriteLine("last                    : see more info");
            cout.WriteLine();
            cout.WriteLine("<File Command>");
            cout.WriteLine("lcd [path]              : change or display current directory");
            cout.WriteLine("ldir [path]             : display local files on the directory");
            cout.WriteLine("ltype [path]            : display local file content");
            cout.WriteLine("run [path]file          : run a batch program (.sqc)");
            cout.WriteLine("call [path]file [/dump] : call Tie program (.sqt), if option /dump used, memory dumps to output file");
            cout.WriteLine("execute [path]file      : execute sql script(.sql)");
            cout.WriteLine();
            cout.WriteLine("<Schema Commands>");
            cout.WriteLine("find /?                 : see more info");
            cout.WriteLine("show view               : show all views");
            cout.WriteLine("show proc               : show all stored proc and func");
            cout.WriteLine("show index              : show all indices");
            cout.WriteLine("show vw viewnames       : show view structure");
            cout.WriteLine("show pk                 : show all tables with primary keys");
            cout.WriteLine("show npk                : show all tables without primary keys");
            cout.WriteLine();
            cout.WriteLine("<State Command>");
            cout.WriteLine("show connection         : show connection-string list");
            cout.WriteLine("show current            : show current active connection-string");
            cout.WriteLine("show var                : show variable list");
            cout.WriteLine();
            cout.WriteLine("<SQL Command>");
            cout.WriteLine("type [;] to execute following SQL script or functions");
            cout.WriteLine("select ... from table where ...");
            cout.WriteLine("update table set ... where ...");
            cout.WriteLine("delete from table where...");
            cout.WriteLine("create table ...");
            cout.WriteLine("drop table ...");
            cout.WriteLine("alter ...");
            cout.WriteLine("exec ...");
            cout.WriteLine("<Variables>");
            cout.WriteLine("  maxrows               : max number of row shown on select query");
            cout.WriteLine("  DataReader            : true: use SqlDataReader; false: use Fill DataSet");
            cout.WriteLine();
        }
    }
}
