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
using System.Reflection;

namespace Sys
{
    public enum Policy
    {
        LRU,    //Least Recently Used (LRU), discards the least recently used items first
        LFU     //Least Frequently Used (LFU), Those that are used least often are discarded first.
    }

    class DataItem<T> where T : class
    {
        T t;
        public DateTime age;
        public long hit;

        public DataItem(T t)
        {
            this.t = t;
            this.age = DateTime.Now;
            this.hit = 0;
        }

        public T Item
        {
            get
            {
                this.age = DateTime.Now;
                this.hit++;

                return t;
            }
        }
    }


    
    /// <summary>
    /// Pooling support LRU and LFU policy
    /// 
    /// Must implement constructor: T(K key)
    /// </summary>
    /// <typeparam name="K">typeof(Key)</typeparam>
    /// <typeparam name="T">typeof(Value)</typeparam>
    public class DataPool<K, T> 
        where K : class 
        where T : class
    {
        private Dictionary<K, DataItem<T>> pool = new Dictionary<K, DataItem<T>>();
        private int count;

        private Policy policy;

        public DataPool(int maxCount)
            : this(maxCount, Policy.LRU)
        {
        }

        public DataPool(int maxCount, Policy policy)
        {
            this.count = maxCount;
            this.policy = policy;
        }
        
        public T GetItem(K key)
        {
            if (pool.ContainsKey(key))
            {
                return pool[key].Item;
            }

            T t = (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance| BindingFlags.Public| BindingFlags.NonPublic, null, new object[] { key }, null);

            DataItem<T> m = new DataItem<T>(t);
            pool.Add(key, m);

            if (pool.Count > count)
            {
                K k = null;
                
                if(policy == Policy.LRU)
                    k = LRU_Policy();
                else
                    k = LFU_Policy();

                //remove the eldest item
                if (k != null)
                    pool.Remove(k);
            }

            return m.Item;
        }


        private K LRU_Policy()
        { 
            K key = null;

            DateTime age = DateTime.Now;
            foreach (KeyValuePair<K, DataItem<T>> p in pool)
            {
                if (p.Value.age < age)
                {
                    age = p.Value.age;
                    key = p.Key;
                }
            }

            return key;
        }


        private K LFU_Policy()
        {
            K key = null;

            long hit = Int64.MaxValue;
            foreach (KeyValuePair<K, DataItem<T>> p in pool)
            {
                if (p.Value.hit < hit)
                {
                    hit = p.Value.hit;
                    key = p.Key;
                }
            }

            return key;
        }
    }

  


}
