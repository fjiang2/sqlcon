using System.Windows.Controls;
using System.Windows.Media;
using Sys.Data;

namespace sqlcon.Windows
{
    public class DbTreeNodeUI : TreeViewItem
    {
        public IDataPath Path { get; set; }
        public string Text { get; }

        public DbTreeNodeUI(string text, string imageName)
        {
            this.Text = text;
            var label = WpfUtils.NewImageLabel(text, imageName);
            this.Header = label;

            Foreground = Brushes.White;
            Background = Brushes.Black;
        }

        public void ChangeImage(string imageName)
        {
            StackPanel panel = (StackPanel)this.Header;
            Image image = panel.Children[0] as Image;
            image.Source = WpfUtils.NewBitmapImage(imageName);
        }

        public override string ToString()
        {
            return Path.Path;
        }
    }
}
