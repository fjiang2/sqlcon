//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys
{
    /// <summary>
    /// Represents a strongly typed list of TreeNode that can be accessed by index.
    ///     Provides methods to add, sort, and manipulate lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNodeCollection<T> : List<TreeNode<T>> where T : class
    {
        private TreeNode<T> parent;

        internal TreeNodeCollection(TreeNode<T> parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Add a node into this collection
        /// </summary>
        /// <param name="node"></param>
        public new void Add(TreeNode<T> node)
        {
            node.Parent = this.parent;
            node.Index = this.Count;
            base.Add(node);
        }


        /// <summary>
        /// Add a list of nodes into this collection
        /// </summary>
        /// <param name="collection"></param>
        public new void AddRange(IEnumerable<TreeNode<T>> collection)
        {
            foreach (TreeNode<T> node in collection)
                this.Add(node);
        }

        /// <summary>
        /// Append a tree into a node
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="rootItem"></param>
        public void Add(Tree<T> tree, T rootItem)
        {
            tree.RootNode.Item = rootItem;
            this.Add(tree.RootNode);
        }

        private void ToList(List<TreeNode<T>> list, TreeNodeCollection<T> nodes)
        {
            foreach (TreeNode<T> child in nodes)
            {
                list.Add(child);
                ToList(list, child.Nodes);
            }
        }


        /// <summary>
        /// Make linq operational
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeNode<T>> AsEnumerable()
        {
            List<TreeNode<T>> list = new List<TreeNode<T>>();
            ToList(list, this);

            return list;
        }



        /// <summary>
        /// return description of this collection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("TreeNodeCollection<{0}>(Count={1})", typeof(T).Name, this.Count);
        }

    }
}
