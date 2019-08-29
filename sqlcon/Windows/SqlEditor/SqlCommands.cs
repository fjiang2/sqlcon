using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace sqlcon.Windows
{
    public class SqlCommands
    {
        public static readonly RoutedUICommand Execute = new RoutedUICommand("Execute", "execute", typeof(SqlEditor), new InputGestureCollection { new KeyGesture(Key.F5, ModifierKeys.None, "F5") });

        public static readonly RoutedUICommand Select1000 = new RoutedUICommand("Select 1000", "select1000", typeof(SqlEditor), new InputGestureCollection { new KeyGesture(Key.F7, ModifierKeys.None, "F7") });
        public static readonly RoutedUICommand Select = new RoutedUICommand("Select", "select", typeof(SqlEditor), new InputGestureCollection { new KeyGesture(Key.F8, ModifierKeys.None, "F8") });
    }
}
