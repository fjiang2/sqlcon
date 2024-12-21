using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Stdio;

namespace SqlCon
{
    partial class Shell
    {

        private static void Help()
        {
            Cout.WriteLine("Path points to server, database,tables, data rows");
            Cout.WriteLine(@"      \server\database\table\filter\filter\....");
            Cout.WriteLine("Notes: table names support wildcard matching, e.g. Prod*,Pro?ucts");
            Cout.WriteLine("exit                    : quit application");
            Cout.WriteLine("help                    : this help");
            Cout.WriteLine("?                       : this help");
            Cout.WriteLine("rem                     : comments or remarks");
            Cout.WriteLine("ver                     : display version");
            Cout.WriteLine("cls                     : clears the screen");
            Cout.WriteLine("echo /?                 : display text");
            Cout.WriteLine("dir,ls /?               : display path(server, database, table)");
            Cout.WriteLine("cd,chdir /?             : change path");
            Cout.WriteLine("md,mkdir /?             : create path or filter");
            Cout.WriteLine("rd,rmdir /?             : remove path or filter");
            Cout.WriteLine("type /?                 : type content of table");
            Cout.WriteLine("set /?                  : update values");
            Cout.WriteLine("let /?                  : assign value to variable, see more info");
            Cout.WriteLine("del,erase /?            : delete path");
            Cout.WriteLine("ren,rename /?           : rename database, table, column name");
            Cout.WriteLine("attrib /?               : add/remove primary key, foreign key and identity key");
            Cout.WriteLine("copy /?                 : copy table schema or rows");
            Cout.WriteLine("xcopy /?                : copy large size table");
            Cout.WriteLine("comp /?                 : compare table schema or data");
            Cout.WriteLine("compare path1 [path2]   : compare table scheam or data");
            Cout.WriteLine("          /s            : compare schema, otherwise compare data");
            Cout.WriteLine("          /e            : compare common existing tables only");
            Cout.WriteLine("          /col:c1,c2    : skip columns defined during comparing");
            Cout.WriteLine("sync table1 table2      : synchronize, make table2 is the same as table1");
            Cout.WriteLine("import /?               : import data into database");
            Cout.WriteLine("export /?               : generate SQL script, JSON, C# code");
            Cout.WriteLine("clean /?                : clean duplicated rows");
            Cout.WriteLine("mount /?                : mount new database server");
            Cout.WriteLine("umount /?               : unmount database server");
            Cout.WriteLine("open /?                 : open result file");
            Cout.WriteLine("load /?                 : load JSON, XML data and cfg file");
            Cout.WriteLine("save /?                 : save data");
            Cout.WriteLine("edit /?                 : open GUI edit window");
            Cout.WriteLine("chk,check /?            : check syntax of key-value table");
            Cout.WriteLine("last                    : display last result");
            Cout.WriteLine();
            Cout.WriteLine("<File Command>");
            Cout.WriteLine("lcd [path]              : change or display current directory");
            Cout.WriteLine("ldir [path]             : display local files on the directory");
            Cout.WriteLine("ltype [path]            : display local file content");
            Cout.WriteLine("path [path]             : set environment variable PATH");
            Cout.WriteLine("run [path]file          : run a batch program (.sqc)");
            Cout.WriteLine("call [path]file [/dump] : call Tie program (.sqt), if option /dump used, memory dumps to output file");
            Cout.WriteLine("execute [path]file      : execute sql script(.sql)");
            Cout.WriteLine();
            Cout.WriteLine("<Schema Commands>");
            Cout.WriteLine("find /?                 : see more info");
            Cout.WriteLine("show view               : show all views");
            Cout.WriteLine("show proc               : show all stored proc and func");
            Cout.WriteLine("show index              : show all indices");
            Cout.WriteLine("show vw viewnames       : show view structure");
            Cout.WriteLine("show pk                 : show all tables with primary keys");
            Cout.WriteLine("show npk                : show all tables without primary keys");
            Cout.WriteLine();
            Cout.WriteLine("<State Command>");
            Cout.WriteLine("show connection         : show connection-string list");
            Cout.WriteLine("show current            : show current active connection-string");
            Cout.WriteLine("show var                : show variable list");
            Cout.WriteLine();
            Cout.WriteLine("<SQL Command>");
            Cout.WriteLine("type [;] to execute following SQL script or functions");
            Cout.WriteLine("select ... from table where ...");
            Cout.WriteLine("update table set ... where ...");
            Cout.WriteLine("delete from table where...");
            Cout.WriteLine("create table ...");
            Cout.WriteLine("drop table ...");
            Cout.WriteLine("alter ...");
            Cout.WriteLine("exec ...");
            Cout.WriteLine("<Variables>");
            Cout.WriteLine("  maxrows               : max number of row shown on select query");
            Cout.WriteLine("  DataReader            : true: use SqlDataReader; false: use Fill DataSet");
            Cout.WriteLine();
        }
    }
}
