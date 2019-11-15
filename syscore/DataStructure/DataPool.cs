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

    class PoolItem<T> where T : class
    {
        T t;
        public DateTime age;
        public long hit;

        public PoolItem(T t)
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

        public override string ToString()
        {
            return $"hit:{hit}, age={age}, data={t}";
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
        where T : class
    {
        private Dictionary<K, PoolItem<T>> pool = new Dictionary<K, PoolItem<T>>();
        private int count;

        private Policy policy;
        public Func<K, T> CreateInstance { get; set; }

        public DataPool(int maxCount)
            : this(maxCount, Policy.LRU)
        {
        }

        public DataPool(int maxCount, Policy policy)
        {
            this.count = maxCount;
            this.policy = policy;
            CreateInstance = this.createInstance;
        }

        private T createInstance(K key)
        {
            T t = (T)Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { key }, null);
            return t;
        }

        public T GetItem(K key)
        {
            if (pool.ContainsKey(key))
            {
                return pool[key].Item;
            }

            T t = CreateInstance(key);

            PoolItem<T> m = new PoolItem<T>(t);
            pool.Add(key, m);

            if (pool.Count > count)
            {
                K k = default(K); 


                bool found;
                if (policy == Policy.LRU)
                    found = LRU_Policy(out k);
                else
                    found = LFU_Policy(out k);

                //remove the eldest item
                if (found)
                    pool.Remove(k);
            }

            return m.Item;
        }


        private bool LRU_Policy(out K key)
        {
            bool found = false;
            key = default(K);

            DateTime age = DateTime.Now;
            foreach (KeyValuePair<K, PoolItem<T>> p in pool)
            {
                if (p.Value.age < age)
                {
                    age = p.Value.age;
                    key = p.Key;
                    found = true;
                }
            }

            return found;
        }


        private bool LFU_Policy(out K key)
        {
            bool found = false;

            key = default(K);

            long hit = Int64.MaxValue;
            foreach (KeyValuePair<K, PoolItem<T>> p in pool)
            {
                if (p.Value.hit < hit)
                {
                    hit = p.Value.hit;
                    key = p.Key;
                    found = true;
                }
            }

            return found;
        }

    }

}
