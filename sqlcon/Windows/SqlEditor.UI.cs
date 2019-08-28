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
using System.IO;
using System.Data;
using System.Data.SqlClient;

using Sys;
using Sys.Data;
using Sys.Data.IO;
using System.ComponentModel;

namespace sqlcon.Windows
{
    partial class SqlEditor : Window
    {
        private TabControl tabControl = new TabControl();
        private TextBlock lblMessage = new TextBlock { Width = 300 };
        private TextBlock lblCursorPosition = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        private TextBlock lblRowCount = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };

        private ComboBox comboPath;
        private RichTextBox textBox = new RichTextBox
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        private static RoutedUICommand ExecuteCommand = new RoutedUICommand("Execute", "execute", typeof(SqlEditor), new InputGestureCollection { new KeyGesture(Key.F5, ModifierKeys.None, "F5") });

        private void InitializeComponent(Configuration cfg)
        {
            this.Width = 1024;
            this.Height = 768;

            Button btnNew = WpfUtils.NewImageButton(ApplicationCommands.New, "New", "New(Ctrl-N)", "New_16x16.png");
            Button btnOpen = WpfUtils.NewImageButton(ApplicationCommands.Open, "Open", "Open(Ctrl-O)", "Open_16x16.png");
            Button btnSave = WpfUtils.NewImageButton(ApplicationCommands.Save, "Save", "Save(Ctrl-S)", "Save_16x16.png");
            Button btnExecute = WpfUtils.NewImageButton(ExecuteCommand, "Execute", "Execute(F5)", "Next_16x16.png");
            this.comboPath = new ComboBox { Width = 300, HorizontalAlignment = HorizontalAlignment.Right };
            this.comboPath.SelectionChanged += ComboPath_SelectionChanged;
            DockPanel dockPanel = new DockPanel();
            this.Content = dockPanel;


            //Tool bar
            ToolBarTray tray = new ToolBarTray();
            tray.SetValue(DockPanel.DockProperty, Dock.Top);
            dockPanel.Children.Add(tray);

            ToolBar toolBar;
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(btnNew);
            toolBar.Items.Add(btnOpen);
            toolBar.Items.Add(btnSave);
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(btnExecute);
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(comboPath);

            //status bar
            StatusBar statusBar = new StatusBar { Height = 20 };
            statusBar.Items.Add(new StatusBarItem { Content = lblMessage, HorizontalAlignment = HorizontalAlignment.Left });
            statusBar.Items.Add(new StatusBarItem { Content = lblCursorPosition, HorizontalAlignment = HorizontalAlignment.Right });
            statusBar.Items.Add(new StatusBarItem { Content = lblRowCount, HorizontalAlignment = HorizontalAlignment.Right });
            statusBar.SetValue(DockPanel.DockProperty, Dock.Bottom);
            dockPanel.Children.Add(statusBar);


            #region tree, editor and results
            Grid grid = new Grid();
            dockPanel.Children.Add(grid);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            Grid grid1 = new Grid();
            GridSplitter vSplitter = new GridSplitter { Width = 5, VerticalAlignment = VerticalAlignment.Stretch };
            Grid grid2 = new Grid();

            grid1.SetValue(Grid.ColumnProperty, 0);
            vSplitter.SetValue(Grid.ColumnProperty, 1);
            grid2.SetValue(Grid.ColumnProperty, 2);
            grid.Children.Add(grid1);
            grid.Children.Add(vSplitter);
            grid.Children.Add(grid2);

            //Database Tree
            DbTreeUI treeView = new DbTreeUI
            {
                //Width = 160,
                Foreground = Brushes.White,
                //Background = Brushes.Black
            };
            grid1.Children.Add(treeView);

            treeView.CreateTree(cfg);
            treeView.PathChanged += TreeView_PathChanged;

            grid2.RowDefinitions.Add(new RowDefinition());
            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid2.RowDefinitions.Add(new RowDefinition());

            textBox.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            textBox.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            //Paragraph space
            Style style = new Style { TargetType = typeof(Paragraph) };
            style.Setters.Add(new Setter { Property = Block.MarginProperty, Value = new Thickness(0) });
            textBox.Resources.Add(typeof(Paragraph), style);

            GridSplitter hSplitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            tabControl.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            tabControl.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            textBox.SetValue(Grid.RowProperty, 0);
            hSplitter.SetValue(Grid.RowProperty, 1);
            tabControl.SetValue(Grid.RowProperty, 2);
            grid2.Children.Add(textBox);
            grid2.Children.Add(hSplitter);
            grid2.Children.Add(tabControl);

            #endregion


            CommandBinding binding;
            RoutedUICommand[] commands = new RoutedUICommand[]
               {
                  ApplicationCommands.New,
                  ApplicationCommands.Open,
                  ApplicationCommands.Save,
                  ExecuteCommand
               };

            foreach (var cmd in commands)
            {
                binding = new CommandBinding(cmd);
                binding.Executed += commandExecute;
                binding.CanExecute += commandCanExecute;
                this.CommandBindings.Add(binding);
            }
        }

        private void ComboPath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string path = combo.SelectedValue as string;
        }

        private void TreeView_PathChanged(object sender, EventArgs<TreeNode<IDataPath>> e)
        {
            TreeNode<IDataPath> node = e.Value;
            IDataPath name = node.Item;
            if (name is TableName)
            {
                tabControl.Items.Clear();
                var dt = new TableReader(name as TableName, cmd.Top).Table;
                var tab = new TabItem { Header = name.Path, Content = DisplayTable(dt) };
                tabControl.Items.Add(tab);
                tab.Focus();
                return;
            }

            if (!(name is DatabaseName))
                return;

            //string path = node.XPath(x => x.Path);

            var found = comboPath.Items.OfType<IDataPath>().FirstOrDefault(x => x == name);
            if (found == null)
            {
                comboPath.Items.Add(name);
                found = name;
            }

            comboPath.SelectedValue = found;
        }

    }
}