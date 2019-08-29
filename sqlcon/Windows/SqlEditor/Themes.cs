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
            public static Brush Foreground;
            public static Brush Background;
        }

        public static class TableEditor
        {
            public static Brush Foreground;
            public static Brush Background;
            public static Brush AlternatingRowBackground;
        }

        public static class SqlResult
        {
            public static class Table
            {
                public static Brush Foreground;
                public static Brush Background;
                public static Brush AlternatingRowBackground;
                public static Brush RowBackground;
            }
            public static class Message
            {
                public static Brush Foreground;
                public static Brush Background;
            }

        }

        static Themes()
        {
            Configuration cfg = Program.cfg;

            TableEditor.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_FOREGROUND, Colors.LightGray);
            TableEditor.Background = cfg.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_ROWBACKGROUND, Colors.Black);
            TableEditor.AlternatingRowBackground = cfg.GetSolidBrush(ConfigKey._GUI_TABLE_EDITOR_ALTERNATINGROWBACKGROUND, Colors.DimGray);

            SqlEditor.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            SqlEditor.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            SqlResult.Table.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_FOREGROUND, Colors.White);
            SqlResult.Table.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_BACKGROUND, Colors.Black);
            SqlResult.Table.AlternatingRowBackground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND, Colors.DimGray);
            SqlResult.Table.RowBackground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ROWBACKGROUND, Colors.Black);

            SqlResult.Message.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_FOREGROUND, Colors.White);
            SqlResult.Message.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_BACKGROUND, Colors.Black);
        }
    }

}
