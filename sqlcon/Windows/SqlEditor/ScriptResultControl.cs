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
    class ScriptResultControl : TabControl
    {
        private TabControl tabControl;
        private Dictionary<FileLink, ScriptResultPane> panes { get; } = new Dictionary<FileLink, ScriptResultPane>();

        public SqlEditor Editor { get; }
        public ScriptResultControl(SqlEditor parent)
        {
            this.Editor = parent;
            tabControl = this;
        }

        public ScriptResultPane AddTab(FileLink link)
        {
            ScriptResultPane pane;
            if (panes.ContainsKey(link))
            {
                pane = panes[link];
                tabControl.SelectedItem = pane.TabItem;
                return pane;
            }

            pane = new ScriptResultPane(this)
            {
                Link = link
            };

            panes.Add(link, pane);

            string header = link.ToString();

            const int count = 20;
            if (header.Length > count)
                header = header.Substring(0, count / 2) + "..." + header.Substring(header.Length - count / 2);

            TabItem newTab = new TabItem
            {
                Header = header,
                Content = pane,
                ToolTip = link.ToString(),
            };
            pane.TabItem = newTab;
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;

            pane.Focus();
            return pane;
        }

        public ScriptResultPane SelectedPane
        {
            get
            {
                var tab = SelectedTab();
                if (tab != null)
                    return (ScriptResultPane)tab.Content;

                return null;
            }
        }

        private TabItem SelectedTab()
        {
            return (TabItem)tabControl.SelectedItem;
        }

        public bool IsDirty => panes.Values.Any(x => x.IsDirty && x.Link.IsLocalLink);

        public void Delete()
        {

        }

        public void OnClosing(CancelEventArgs e)
        {
            if (!IsDirty)
                return;

            var result = MessageBox.Show($"Save changes", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                foreach (var pane in panes.Values)
                {
                    pane.Save();
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}
