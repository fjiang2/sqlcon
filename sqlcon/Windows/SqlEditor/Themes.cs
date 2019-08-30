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
        public static class SqlEditor
        {
            public static Brush Foreground => Config.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.White);
            public static Brush Background => Config.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.Black);
        }

        public static class TableEditor
        {
            public static Brush Foreground => Config.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_FOREGROUND, Colors.LightGray);
            public static Brush Background => Config.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_BACKGROUND, Colors.Black);
            public static Brush AlternatingRowBackground => Config.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_ALTERNATINGROWBACKGROUND, Colors.DimGray);
            public static Brush RowBackground => Config.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_ROWBACKGROUND, Colors.DimGray);
        }

        public static class SqlResult
        {
            public static class Table
            {
                public static Brush Foreground => Config.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_FOREGROUND, Colors.White);
                public static Brush Background => Config.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_BACKGROUND, Colors.Black);
                public static Brush AlternatingRowBackground => Config.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND, Colors.DimGray);
                public static Brush RowBackground => Config.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ROWBACKGROUND, Colors.Black);
            }

            public static class Message
            {
                public static Brush Foreground => Config.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_FOREGROUND, Colors.White);
                public static Brush Background => Config.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_BACKGROUND, Colors.Black);
            }
        }
    }

}
