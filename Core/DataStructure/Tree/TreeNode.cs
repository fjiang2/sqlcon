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
    /// represents tree node in the class Tree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TreeNode<T> where T : class
    {
        private T item;
        private TreeNode<T> parent;
        private int index;
        private TreeNodeCollection<T> nodes;

        /// <summary>
        /// create tree node from value item
        /// </summary>
        /// <param name="item"></param>
        public TreeNode(T item)
        {
            this.item = item;
            this.index = -1;
            this.parent = null;

            nodes = new TreeNodeCollection<T>(this);
        }


        /// <summary>
        /// Gets the collection of Node objects assigned to the current tree node.
        /// </summary>
        public TreeNodeCollection<T> Nodes
        {
            get { return this.nodes; }
        }


        /// <summary>
        /// Gets the parent tree node of the current tree node.
        /// </summary>
        public TreeNode<T> Parent
        {
            get { return this.parent; }
            internal set { this.parent = value; }
        }


        /// <summary>
        /// Gets the sibling of this node
        /// </summary>
        public TreeNode<T>[] Sibling
        {
            get
            {
                List<TreeNode<T>> sibling = new List<TreeNode<T>>();
                foreach(TreeNode<T> child in this.parent.Nodes)
                {
                    if(child != this)
                        sibling.Add(child);
                }

                return sibling.ToArray();
            }
        }

        /// <summary>
        /// Node position in sibling nodes, index of first node = 0
        /// </summary>
        public int Index
        {
            get { return this.index; }
            internal set { this.index = value; }
        }

        /// <summary>
        /// Gets the first child tree node in the tree node collection.
        /// </summary>
        public TreeNode<T> FirstNode
        {
            get
            {
                if (nodes.Count == 0)
                    return null;
                else
                    return nodes[0];
            }
        }


        /// <summary>
        /// Gets the last child tree node.
        /// </summary>
        public TreeNode<T> LastNode
        {
            get
            {
                if (nodes.Count == 0)
                    return null;
                else
                    return nodes[nodes.Count - 1];
            }
        }

        /// <summary>
        /// Gets the next sibling tree node.
        /// </summary>
        public TreeNode<T> NextNode
        {
            get
            {
                if (this.index < nodes.Count - 1)
                    return this.parent.Nodes[index + 1];
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets the previous sibling tree node.
        /// </summary>
        public TreeNode<T> PrevNode
        {
            get
            {
                if (this.index > 0)
                    return this.parent.Nodes[index - 1];
                else
                    return null;
            }
        }


        /// <summary>
        /// Gets the path from the root tree node to the current tree node.
        /// </summary>
        public string FullPath
        {
            get
            {
                string path = this.item.ToString();
                TreeNode<T> node = this;
                while (node.Parent != null)
                {
                    path = node.Parent.item.ToString() + "\\" + path;
                    node = node.Parent;
                }

                return path;
            }
        }

        /// <summary>
        ///  Gets the zero-based depth of the tree node 
        /// </summary>
        public int Level
        {
            get
            {
                int level = 0;
                TreeNode<T> node = this;
                while (node.Parent != null)
                {
                    level++;
                    node = node.Parent;
                }

                return level;
            }
        }

        /// <summary>
        /// Remove the current node from tree
        /// </summary>
        public void Remove()
        {
            this.parent.Nodes.Remove(this);
        }


        /// <summary>
        /// returns the value item
        /// </summary>
        public T Item
        {
            get { return this.item; }
            set { this.item = value; }
        }

        /// <summary>
        /// returns children nodes in array
        /// </summary>
        /// <returns></returns>
        public T[] ToArray()
        {
            return AsEnumerable().Select(node => node.Item).ToArray();
        }

        /// <summary>
        /// returns children nodes in enumerable
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TreeNode<T>> AsEnumerable()
        {
            List<TreeNode<T>> list = new List<TreeNode<T>>();
            ToList(list, this);

            return list;
        }

        private void ToList(List<TreeNode<T>> list, TreeNode<T> node)
        {
            list.Add(node);
            foreach (TreeNode<T> child in this.nodes)
            {
                ToList(list, child);
            }
        }


        /// <summary>
        /// returns the description of node
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.item.ToString();
        }
    }
}
