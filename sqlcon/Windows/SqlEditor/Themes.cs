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
            public static Brush Foreground => Config.gui.sql.editor.Foreground.GetBrush(Colors.White);
            public static Brush Background => Config.gui.sql.editor.Background.GetBrush(Colors.Black);
        }

        public static class TableEditor
        {
            public static Brush Foreground => Config.gui.table.editor.Foreground.GetBrush(Colors.LightGray);
            public static Brush Background => Config.gui.table.editor.Background.GetBrush(Colors.Black);
            public static Brush AlternatingRowBackground => Config.gui.table.editor.AlternatingRowBackground.GetBrush(Colors.DimGray);
            public static Brush RowBackground => Config.gui.table.editor.RowBackground.GetBrush(Colors.DimGray);
        }

        public static class SqlResult
        {
            public static class Table
            {
                public static Brush Foreground => Config.gui.sql.result.table.Foreground.GetBrush(Colors.White);
                public static Brush Background => Config.gui.sql.result.table.Background.GetBrush(Colors.Black);
                public static Brush AlternatingRowBackground => Config.gui.sql.result.table.AlternatingRowBackground.GetBrush(Colors.DimGray);
                public static Brush RowBackground => Config.gui.sql.result.table.RowBackground.GetBrush(Colors.Black);
            }

            public static class Message
            {
                public static Brush Foreground => Config.gui.sql.result.message.Foreground.GetBrush(Colors.White);
                public static Brush Background => Config.gui.sql.result.message.Background.GetBrush(Colors.Black);
            }
        }
    }

}
