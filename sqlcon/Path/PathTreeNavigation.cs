using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys;
using Sys.Data;
using Sys.Stdio;


namespace sqlcon
{
    partial class PathManager
    {
        internal TreeNode<IDataPath> current;

        public IDataPath Current
        {
            get { return this.current.Item; }
        }

        public TreeNode<IDataPath> RootNode
        {
            get { return tree.RootNode; }
        }

        public T GetCurrentPath<T>() where T : IDataPath
        {
            return GetPathFrom<T>(this.current);
        }

        public TreeNode<IDataPath> GetCurrentNode<T>() where T : IDataPath
        {
            return GetNodeFrom<T>(this.current);
        }

        public T GetPathFrom<T>(TreeNode<IDataPath> current) where T : IDataPath
        {
            if (current == RootNode)
                return default(T);

            var pt = current;
            while (!(pt.Item is T))
            {
                pt = pt.Parent;

                if (pt == null)
                    return default(T);
            }

            return (T)pt.Item;
        }


        public TreeNode<IDataPath> GetNodeFrom<T>(TreeNode<IDataPath> current) where T : IDataPath
        {
            if (current == RootNode)
                return null;

            var pt = current;
            while (!(pt.Item is T))
            {
                pt = pt.Parent;

                if (pt == null)
                    return null;
            }

            return pt;
        }

        public Locator GetCombinedLocator(TreeNode<IDataPath> pt1)
        {
            if (pt1.Item is Locator)
            {
                Locator locator = new Locator((Locator)pt1.Item);
                var pt2 = pt1;
                while (pt2.Parent.Item is Locator)
                {
                    pt2 = pt2.Parent;
                    locator.And((Locator)pt2.Item);
                }

                return locator;
            }
            else
                return null;
        }

        public TreeNode<IDataPath> Navigate(PathName pathName)
        {
            string[] segments = pathName.FullSegments;

            if (segments.Length == 0)
                return current;

            var node = current;
            foreach (string segment in segments)
            {
                node = Navigate(node, segment);
                if (node == null)
                    return null;
            }

            return node;
        }

        private TreeNode<IDataPath> Navigate(TreeNode<IDataPath> node, string segment)
        {
            if (segment == "\\")
            {
                return RootNode;
            }
            else if (segment == ".")
            {
                return node;
            }
            else if (segment == "..")
            {
                if (node != RootNode)
                    return node.Parent;
                else
                    return node;
            }
            else if (segment == "...")
            {
                return Navigate(Navigate(node, ".."), "..");
            }
            else if (segment == "~~")
            {
                return Navigate(new PathName(cfg.DefaultServerPath));
            }
            else if (segment == "~")
            {
                if (node == RootNode)
                {
                    return Navigate(new PathName(cfg.DefaultServerPath));
                }
                else if (node.Item is DatabaseName)
                {
                    var dname = node.Item as DatabaseName;
                    if (dname != null)
                    {
                        return NavigateToDefaultDatabase(dname.Provider) ?? node;
                    }
                }
                else if (node.Item is ServerName)
                {
                    var sname = node.Item as ServerName;
                    if (sname != null)
                    {
                        return NavigateToDefaultDatabase(sname.Provider) ?? node;
                    }
                }
            }

            Expand(node, this.Refreshing);

            string seg = segment;
            if (node.Item is DatabaseName && segment.IndexOf(".") == -1)
                seg = TableName.dbo + "." + segment;

            var xnode = node.Nodes.Find(x => x.Item.Path.ToUpper() == seg.ToUpper());
            if (xnode != null)
                return xnode;
            else
            {
                int result;
                if (int.TryParse(segment, out result))
                {
                    result--;

                    if (result >= 0 && result < node.Nodes.Count)
                        return node.Nodes[result];
                }

                return null;
            }
        }

        private TreeNode<IDataPath> NavigateToDefaultDatabase(ConnectionProvider provider)
        {
            DatabaseName d = provider.DefaultDatabaseName;
            if (DbSchemaProvider.IsSystemDatabase(d.Name))
            {
                cerr.WriteLine($"cannot navigate to system database: \"{d.Name}\"");
                return null;
            }

            return Navigate(new PathName(d.FullPath));
        }

        public TreeNode<IDataPath> TryAddWhereOrColumns(TreeNode<IDataPath> pt, ApplicationCommand cmd)
        {
            if (!(pt.Item is Locator) && !(pt.Item is TableName))
            {
                return pt;
            }

            if (string.IsNullOrEmpty(cmd.arg1))
            {
                cerr.WriteLine("argument cannot be empty");
            }

            TableName tname = GetCurrentPath<TableName>();

            var locator = new Locator(cmd.arg1) { Name = cmd.GetValue("name") };
            if (locator.Name == null)
            {
                locator.Name = $"filter{pt.Nodes.Count + 1}";
            }

            var builder = new SqlBuilder().SELECT.TOP(1).COLUMNS().FROM(tname).WHERE(locator);
            if (builder.Invalid())
            {
                cerr.WriteLine($"invalid path: {cmd.arg1}");
                return pt;
            }

            var xnode = new TreeNode<IDataPath>(locator);
            pt.Nodes.Add(xnode);

            return xnode;
        }



    }
}
