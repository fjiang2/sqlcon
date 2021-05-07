using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

using Sys;
using Sys.Data;

namespace sqlcon.Windows
{
    public partial class SqlEditor : Window
    {
        private TextBlock lblMessage = new TextBlock { Width = 300 };
        private TextBlock lblCursorPosition = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };

        private ComboBox comboPath;
        private TextBox textFilter;
        private DbTreeUI treeView;
        private ScriptResultControl scriptTabControl;

        private void InitializeComponent(IConnectionConfiguration cfg, IPathManager mgr)
        {
            this.Width = 1280;
            this.Height = 768;

            Button btnHome = WpfUtils.NewImageButton(SqlCommands.Home, "Home", $"Home(Alt-H): {cfg.Home}", "Home_16x16.png");
            Button btnNew = WpfUtils.NewImageButton(ApplicationCommands.New, "New", "New(Ctrl-N)", "New_16x16.png");
            Button btnOpen = WpfUtils.NewImageButton(ApplicationCommands.Open, "Open", "Open(Ctrl-O)", "Open_16x16.png");
            Button btnSave = WpfUtils.NewImageButton(ApplicationCommands.Save, "Save", "Save(Ctrl-S)", "Save_16x16.png");
            Button btnExecute = WpfUtils.NewImageButton(SqlCommands.Execute, "Execute", "Execute(F5)", "Next_16x16.png");
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
            toolBar.Items.Add(btnHome);
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
            statusBar.SetValue(DockPanel.DockProperty, Dock.Bottom);
            dockPanel.Children.Add(statusBar);


            #region tree, editor and results
            Grid grid = new Grid();
            dockPanel.Children.Add(grid);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            Grid grid1 = new Grid();
            grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
            grid1.RowDefinitions.Add(new RowDefinition());

            GridSplitter vSplitter = new GridSplitter { Width = 5, VerticalAlignment = VerticalAlignment.Stretch };
            scriptTabControl = new ScriptResultControl(this);

            grid1.SetValue(Grid.ColumnProperty, 0);
            vSplitter.SetValue(Grid.ColumnProperty, 1);
            scriptTabControl.SetValue(Grid.ColumnProperty, 2);

            grid.Children.Add(grid1);
            grid.Children.Add(vSplitter);
            grid.Children.Add(scriptTabControl);

            textFilter = new TextBox
            {
                Margin = new Thickness(2),
                ToolTip = "Search table name",
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            textFilter.TextChanged += TextFilter_TextChanged;

            //Database Tree
            this.treeView = new DbTreeUI
            {
                //Width = 160,
                Foreground = Brushes.White,
                Background = Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MinWidth = 120,
            };

            //Style style = new Style(typeof(TreeViewItem));
            //style.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = Brushes.White });
            //style.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = Brushes.Black });
            //style.Resources.Add(SystemColors.ControlBrushKey, Brushes.Black);
            //treeView.ItemContainerStyle = style;

            textFilter.SetValue(Grid.RowProperty, 0);
            treeView.SetValue(Grid.RowProperty, 1);
            grid1.Children.Add(textFilter);
            grid1.Children.Add(treeView);

            treeView.CreateTree(cfg, mgr);
            treeView.PathChanged += TreeView_PathChanged;

            #endregion


            CommandBinding binding;
            RoutedUICommand[] commands = new RoutedUICommand[]
            {
                ApplicationCommands.New,
                ApplicationCommands.Open,
                ApplicationCommands.Save,
                SqlCommands.Home,
                SqlCommands.Execute,
                SqlCommands.Select,
                SqlCommands.Select1000,
            };

            foreach (var cmd in commands)
            {
                binding = new CommandBinding(cmd);
                binding.Executed += commandExecute;
                binding.CanExecute += commandCanExecute;
                this.CommandBindings.Add(binding);
            }
        }

        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            treeView.RunFilter(textFilter.Text);
        }

        private void ComboPath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string path = combo.SelectedValue as string;
        }

        private IResultPane SelectedPane => scriptTabControl.SelectedPane;
        private DbTreeNodeUI SelectedNode => (DbTreeNodeUI)treeView.SelectedItem;

        private void TreeView_PathChanged(object sender, EventArgs<TreeNode<IDataPath>> e)
        {
            TreeNode<IDataPath> node = e.Value;
            IDataPath name = node.Item;
            if (CtrlPressed && name is TableName)
            {
                DisplaySignleTable(name, top: 1000);
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
        private bool CtrlPressed => Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

        private void DisplaySignleTable(IDataPath name, int top)
        {
            scriptTabControl.AddTab(name as TableName, top);
        }

        public void ShowCursorPosition(int row, int col)
        {
            lblCursorPosition.Text = $"Ln {row}, Col {col}";
        }

        public void ShowStatus(string text)
        {
            lblMessage.Text = "saved successfully";
        }
    }
}