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
    /// Sequential List, Order by ID 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SequentialList<T> : List<T>
    {
        public SequentialList()
        {
        }

        public SequentialList(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public SequentialList(int capacity)
            : base(capacity)
        {
        }

        public bool MoveUp(T item)
        {
            int index = this.IndexOf(item);
            if (index == 0)
                return false;

            this.Remove(item);
            this.Insert(index - 1, item);

            return true;
        }

        public bool MoveDown(T item)
        {
            int index = this.IndexOf(item);
            if (index == this.Count - 1)
                return false;

            this.Remove(item);
            this.Insert(index + 1, item);
            return true;
        }

        public void MoveToFirst(T item)
        {
            this.Remove(item);
            this.Insert(0, item);
        }

        public void MoveToLast(T item)
        {
            this.Remove(item);
            this.Insert(this.Count-1, item);
        }

        public void MoveTo(int index, T item)
        {
            this.Remove(item);
            this.Insert(index, item);
        }

        /// <summary>
        /// Order By Id, e.g. Selecor = row => row.Id;
        /// </summary>
        public Func<T, int> Selector { get; set; }

        /// <summary>
        /// Sequential Id Array 
        /// </summary>
        public int[] Sequence
        {
            get
            {
                if (Selector == null)
                    throw new InvalidOperationException("Selector is not defined");

                List<int> seq = new List<int>();
                foreach (var item in this)
                {
                    seq.Add(Selector(item));
                }

                return seq.ToArray();
            }

            set
            {
                if (Selector == null)
                    throw new InvalidOperationException("Selector is not defined");

                int[] seq = value;

                if (seq == null)
                    return;

                if (seq.Length != this.Count)
                    throw new InvalidOperationException("number of sequence does not match list");

                List<T> list = new List<T>();
                try
                {
                    for (int i = 0; i < seq.Length; i++)
                        list.Add(this.Where(r => Selector(r) == seq[i]).First());
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("sequence id does not match list");
                }

                this.Clear();
                foreach (var item in list)
                {
                    this.Add(item);
                }
            }
        }



        public Func<T, T, double> GetDistance;

        /// <summary>
        /// Insert item based on the distance between items
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int InsertNear(T item)
        {
            if (GetDistance == null)
                throw new InvalidOperationException("GetDistance is not defined");

            return InsertNear(item, GetDistance);
        }

       /// <summary>
       /// Insert item into list based on distance
       /// </summary>
       /// <param name="item"></param>
       /// <param name="distance"></param>
       /// <returns>index inserted</returns>
        public int InsertNear(T item, Func<T, T, double> distance)
        {

            int n = this.Count();

            if (n <= 1)
            {
                this.Add(item);
                return n;
            }

            double d1;
            double d2;

            for (int i1 = 0; i1 < n - 1; i1++)
            {
                int i2 = i1 + 1;

                double d = distance(this[i1], this[i2]);
                d1 = distance(this[i1], item);
                d2 = distance(this[i2], item);

                // x is between Items[i1] and Items[i2]
                if (d1 < d && d2 < d)
                {
                    this.Insert(i2, item);
                    return i2;
                }
            }

            d1 = distance(this[0], item);
            d2 = distance(this[n - 1], item);

            if (d1 < d2)
            {
                this.Insert(0, item);   // x is located before the 1st item
                return 0;
            }
            else
            {
                this.Add(item);        // x is located after the last item
                return n;
            }
        }
    }
}
