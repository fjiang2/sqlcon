using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace sqlcon.Windows
{
    static class Themes
    {
        static Configuration cfg = Program.Configuration;

        public static class SqlEditor
        {
            public static Brush Foreground => cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.White);
            public static Brush Background => cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.Black);
        }

        public static class TableEditor
        {
            public static Brush Foreground => cfg.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_FOREGROUND, Colors.LightGray);
            public static Brush Background => cfg.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_BACKGROUND, Colors.Black);
            public static Brush AlternatingRowBackground => cfg.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_ALTERNATINGROWBACKGROUND, Colors.DimGray);
            public static Brush RowBackground => cfg.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_ROWBACKGROUND, Colors.DimGray);
        }

        public static class SqlResult
        {
            public static class Table
            {
                public static Brush Foreground => cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_FOREGROUND, Colors.White);
                public static Brush Background => cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_BACKGROUND, Colors.Black);
                public static Brush AlternatingRowBackground => cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND, Colors.DimGray);
                public static Brush RowBackground => cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ROWBACKGROUND, Colors.Black);
            }

            public static class Message
            {
                public static Brush Foreground => cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_FOREGROUND, Colors.White);
                public static Brush Background => cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_BACKGROUND, Colors.Black);
            }
        }
    }

}
