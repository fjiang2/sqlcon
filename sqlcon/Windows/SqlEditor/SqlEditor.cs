using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

using Sys.Data;
using Sys.Data.IO;
using System.ComponentModel;

namespace sqlcon.Windows
{
    partial class SqlEditor : Window
    {
        private ApplicationCommand cmd;
        internal Configuration cfg;
        private ConnectionProvider provider;

        public SqlEditor(ApplicationCommand cmd, ConnectionProvider provider, FileLink link)
        {
            this.cmd = cmd;
            this.cfg = cmd.Configuration;

            InitializeComponent(cfg);
            scriptTabControl.SelectionChanged += ScriptTabControl_SelectionChanged;
            this.provider = provider;

            if (link == null)
            {
                link = FileLink.CreateLink(untitled);
                link.TemporaryLink = true;
            }

            Display(link);
        }

        private void ScriptTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileLink link = SelectedPane?.Link;
            if (link != null)
                this.Title = $"{link} - sqlcon";
        }

        private void commandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == SqlCommands.Home)
            {
                e.CanExecute = true;
            }
            else if (e.Command == SqlCommands.Select || e.Command == SqlCommands.Select1000)
            {
                e.CanExecute = SelectedNode != null && SelectedNode.Path is TableName;
            }
            else if (e.Command == SqlCommands.Execute)
            {
                e.CanExecute = (SelectedPane as ScriptResultPane)?.Text != string.Empty;
            }
            else if (e.Command == ApplicationCommands.Save)
            {
                e.CanExecute = scriptTabControl.SelectedItem != null;
            }
            else
                e.CanExecute = true;
        }

        private void commandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == SqlCommands.Home)
                GoHome();
            if (e.Command == SqlCommands.Select)
                Select(top: 0);
            else if (e.Command == SqlCommands.Select1000)
                Select(top: 1000);
            else if (e.Command == SqlCommands.Execute)
                Execute();
            else if (e.Command == ApplicationCommands.New)
                New();
            else if (e.Command == ApplicationCommands.Save)
                SelectedPane.Save();
            else if (e.Command == ApplicationCommands.Open)
                Open();
        }

        private void Display(FileLink link)
        {
            var tab = scriptTabControl.AddTab(link);

            string text = string.Empty;
            if (link.Exists)
                text = link.ReadAllText();

            tab.ShowText(text);
            tab.IsDirty = false;
        }

        private int untitledNumber = 1;
        private string untitled => $"untitled{untitledNumber++}.sql";

        private void GoHome()
        {
            string home = cfg.Home;
            if (home != null)
                treeView.ChangeTreeNode(home);
        }

        private void Select(int top)
        {
            IDataPath path = SelectedNode.Path;
            if (path is TableName)
            {
                DisplaySignleTable(path, top);
            }
        }

        private void Execute()
        {
            IDataPath name = comboPath.SelectedValue as IDataPath;
            if (name is DatabaseName)
                provider = (name as DatabaseName).Provider;

            (SelectedPane as ScriptResultPane)?.Execute(provider);
        }

        public void New()
        {
            FileLink link = FileLink.CreateLink(untitled);
            link.TemporaryLink = true;

            Display(link);
        }

        public void Open()
        {
            var openFile = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Sql Script Files (*.sql)|*.sql|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            };

            if (openFile.ShowDialog(this) == true)
            {
                FileLink link = FileLink.CreateLink(openFile.FileName);
                Display(link);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            scriptTabControl.OnClosing(e);
            base.OnClosed(e);
        }
    }
}
