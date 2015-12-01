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
    /// Numeric Tree, each node has ID(unique integer number), its parent ID, and body(Item)
    /// </summary>
    /// <typeparam name="T">any class</typeparam>
    public class NTree<T> : Tree<T> where T : class
    {

        private IEnumerable<INTreeNode<T>> collection;
        private int parentID;

        /// <summary>
        /// Initializes a new instance from a collection of class T implemeneted interface INTreeNode
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="parentID">parent id</param>
        public NTree(IEnumerable<INTreeNode<T>> collection, int parentID)
        {
            this.collection = collection;
            this.parentID = parentID;

            BuildTree(base.Nodes, parentID);
        }

        private void BuildTree(TreeNodeCollection<T> nodes, int parentID)
        {
            foreach (INTreeNode<T> node in collection)
            {
                if (node.NodeParentId != parentID)
                    continue;

                TreeNode<T> treeNode2 = new TreeNode<T>(node.NodeItem);
                nodes.Add(treeNode2);

                BuildTree(treeNode2.Nodes, node.NodeId);
            }

        }

        /// <summary>
        /// return node collection
        /// </summary>
        public IEnumerable<INTreeNode<T>> Collection
        {
            get
            {
                return this.collection;
            }
        }

        /// <summary>
        /// get item by node id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetNodeItem(int id)
        {
            var node = this.Collection.Where(item => item.NodeId == id).FirstOrDefault();

            if (node != null)
            {
                return node.NodeItem;
            }
            else
                return null;
        }

        /// <summary>
        /// Returns this instance of NTree
        /// </summary>
        /// <returns>the description of NTree</returns>
        public override string ToString()
        {
            return string.Format("NTree<{0}>(Count={1})", typeof(T).Name, this.Nodes.Count);
        }
    }
}
