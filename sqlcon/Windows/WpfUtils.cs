using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace sqlcon.Windows
{
    static class WpfUtils
    {
        public static Button NewImageButton(ICommand command, string text, string toolTip, string image)
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(NewImage(image));
            stackPanel.Children.Add(new TextBlock { Text = text });

            return new Button
            {
                Command = command,
                Content = stackPanel,
                Margin = new Thickness(5),
                ToolTip = toolTip
            };
        }

        public static Image NewImage(string image)
        {
            //make sure: build action on image to Resource
            string pack = "pack://application:,,,/sqlcon;component/Windows/images";
            return new Image { Source = new BitmapImage(new Uri($"{pack}/{image}", UriKind.Absolute)) };
        }

        public static StackPanel NewImageLabel(string text, string image)
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(NewImage(image));
            stackPanel.Children.Add(new TextBlock { Text = text });

            return stackPanel;
        }
        
    }
}
