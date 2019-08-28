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

        internal const string untitled = "untitled.sql";
        public SqlEditor(ApplicationCommand cmd, ConnectionProvider provider, FileLink link)
        {
            this.cmd = cmd;
            this.cfg = cmd.Configuration;

            InitializeComponent(cfg);
            scriptTabControl.SelectionChanged += ScriptTabControl_SelectionChanged;
            this.provider = provider;

            if (link == null)
                link = FileLink.CreateLink(untitled);

            Display(link);
        }

        private void ScriptTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileLink link = SelectedPane?.Link;
            if (link != null)
                this.Title = $"{link} - sqlcon";
        }


        private void Execute()
        {
            IDataPath name = comboPath.SelectedValue as IDataPath;
            if (name is DatabaseName)
                provider = (name as DatabaseName).Provider;

            (SelectedPane as ScriptResultPane)?.Execute(provider);
        }

        private void commandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ExecuteCommand)
            {
                e.CanExecute = (SelectedPane as ScriptResultPane)?.Text != string.Empty;
            }
            else if (e.Command == ApplicationCommands.Save)
                e.CanExecute = scriptTabControl.SelectedItem != null;
            else
                e.CanExecute = true;
        }

        private void commandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ExecuteCommand)
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

        public void New()
        {
            FileLink link = FileLink.CreateLink(untitled);
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
