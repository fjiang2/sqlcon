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
        private Dictionary<string, ScriptResultPane> panes { get; } = new Dictionary<string, ScriptResultPane>();

        public SqlEditor Editor { get; }
        public ScriptResultControl(SqlEditor parent)
        {
            this.Editor = parent;
            tabControl = this;
        }

        public RichTextBox ActiveTextBox => ActivePane.TextBox;
        public TabControl ActiveTabControl => ActivePane.TabControl;

        public ScriptResultPane AddTab(string header)
        {
            ScriptResultPane pane;
            if (panes.ContainsKey(header))
            {
                pane = panes[header];
                foreach (TabItem tab in tabControl.Items)
                {
                    if (tab.Header.Equals(header))
                    {
                        tabControl.SelectedItem = tab;
                        break;
                    }
                }
                return pane;
            }

            pane = new ScriptResultPane(this);
            panes.Add(header, pane);
            TabItem newTab = new TabItem
            {
                Header = header,
                Content = pane
            };
            tabControl.Items.Add(newTab);
            tabControl.SelectedItem = newTab;

            pane.Focus();
            return pane;
        }

        public ScriptResultPane ActivePane
        {
            get
            {
                var tab = ActiveTab();
                if (tab != null)
                    return (ScriptResultPane)tab.Content;

                return null;
            }
        }

        private TabItem ActiveTab()
        {
            foreach (TabItem item in tabControl.Items)
            {
                if (item.IsFocused)
                    return item;
            }

            if (tabControl.Items.Count > 0)
                return (TabItem)tabControl.Items[0];
            else
                return null;
        }

        public bool IsDirty => panes.Values.Any(x => x.IsDirty);

        public void Delete()
        {

        }
    }
}
