using System;
using Sys.Data.Manager;
using Sys;

namespace sqlcon
{
    public static partial class ConfigDefaultValue
    {
        //editor
        public const string __EDITOR = "notepad.exe";

        //directory
        public readonly static string __DIRECTORY = $"{Configuration.MyDocuments}";

        //output
        public readonly static string __OUTPUT = null;

        //log
        public readonly static string __LOG = null;

        //xmldb
        public readonly static string __XMLDB = $"{Configuration.MyDocuments}\\db";

        //options.Comparison.IgnoreCase
        public const bool __OPTIONS_COMPARISON_IGNORECASE = true;

        //console.table.grid.MaxColumnWidth
        public const int __CONSOLE_TABLE_GRID_MAXCOLUMNWIDTH = 50;

        //console.table.grid.MaxRows
        public const int __CONSOLE_TABLE_GRID_MAXROWS = 200;

        //gui.table.editor.AlternatingRowBackground
        public const string __GUI_TABLE_EDITOR_ALTERNATINGROWBACKGROUND = "DimGray";

        //gui.table.editor.Foreground
        public const string __GUI_TABLE_EDITOR_FOREGROUND = "LightGray";

        //gui.table.editor.Background
        public const string __GUI_TABLE_EDITOR_BACKGROUND = "LightGray";

        //gui.table.editor.RowBackground
        public const string __GUI_TABLE_EDITOR_ROWBACKGROUND = "Black";

        //gui.sql.editor.Foreground
        public const string __GUI_SQL_EDITOR_FOREGROUND = "White";

        //gui.sql.editor.Background
        public const string __GUI_SQL_EDITOR_BACKGROUND = "Black";

        //gui.sql.result.table.Foreground
        public const string __GUI_SQL_RESULT_TABLE_FOREGROUND = "White";

        //gui.sql.result.table.Background
        public const string __GUI_SQL_RESULT_TABLE_BACKGROUND = "Black";

        //gui.sql.result.table.AlternatingRowBackground
        public const string __GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND = "DimGray";

        //gui.sql.result.table.RowBackground
        public const string __GUI_SQL_RESULT_TABLE_ROWBACKGROUND = "Black";

        //gui.sql.result.message.Foreground
        public const string __GUI_SQL_RESULT_MESSAGE_FOREGROUND = "White";

        //gui.sql.result.message.Background
        public const string __GUI_SQL_RESULT_MESSAGE_BACKGROUND = "Black";

        //generator.dpo.path
        public readonly static string __GENERATOR_DPO_PATH = $"{Configuration.MyDocuments}\\DataModel\\Dpo";

        //generator.dpo.ns
        public const string __GENERATOR_DPO_NS = "Sys.DataModel.Dpo";

        //generator.dpo.suffix
        public const string __GENERATOR_DPO_SUFFIX = Setting.DPO_CLASS_SUFFIX_CLASS_NAME;

        //generator.dpo.level
        public const int __GENERATOR_DPO_LEVEL = 2;     //Level.Application

        //generator.dpo.HasProvider
        public const bool __GENERATOR_DPO_HASPROVIDER = false;

        //generator.dpo.hasTableAttr
        public const bool __GENERATOR_DPO_HASTABLEATTR = true;

        //generator.dpo.hasColumnAttr
        public const bool __GENERATOR_DPO_HASCOLUMNATTR = true;

        //generator.dpo.IsPack
        public const bool __GENERATOR_DPO_ISPACK = true;

        //generator.dc.path
        public readonly static string __GENERATOR_DC_PATH = $"{Configuration.MyDocuments}\\DataModel\\DataContracts";

        //generator.dc.ns
        public const string __GENERATOR_DC_NS = "Sys.DataModel.DataContracts";

        //generator.l2s.path
        public readonly static string __GENERATOR_L2S_PATH = $"{Configuration.MyDocuments}\\DataModel\\L2s";

        //generator.l2s.ns
        public const string __GENERATOR_L2S_NS = "Sys.DataModel.L2s";

        //generator.de.path
        public readonly static string __GENERATOR_DE_PATH = $"{Configuration.MyDocuments}\\DataModel\\DataEnum";

        //generator.de.ns
        public const string __GENERATOR_DE_NS = "Sys.DataModel.DataEnum";

        //generator.ds.path
        public readonly static string __GENERATOR_DS_PATH = $"{Configuration.MyDocuments}\\ds";

        //generator.csv.path
        public readonly static string __GENERATOR_CSV_PATH = $"{Configuration.MyDocuments}\\csv";

        //limit.top
        public const int __LIMIT_TOP = 1000;

        //limit.export_max_count
        public const int __LIMIT_EXPORT_MAX_COUNT = 2000;
    }
}