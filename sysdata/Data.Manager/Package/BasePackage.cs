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
using Sys.Data;
using Sys.Data.Manager;
using System.Reflection;

namespace Sys.Data.Manager
{
    public abstract class BasePackage<T> : IPacking where T: DPObject
    {
        protected List<T> list = new List<T>();

        public BasePackage()
        {
            Pack();
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public virtual string TableName
        {
            get
            {
                return typeof(T).TableName().Name;
            }
        }

        public virtual Level Level
        {
            get
            {
                return typeof(T).Level();
            }
        }

        public List<T> ToList()
        {
             return this.list; 
        }


        public List<T1> ToList<T1>() where T1: T, new()
        {
            FieldInfo[] publicFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

            List<T1> list1 = new List<T1>();
            foreach (T t in this.list)
            {
                T1 t1 = new T1();
                foreach (FieldInfo fieldInfo in publicFields)
                {
                    object value = fieldInfo.GetValue(t);
                    fieldInfo.SetValue(t1, value);
                }

                list1.Add(t1);
                
            }

            return list1;
        }

        protected abstract void Pack();

        public void Unpack(BackgroundTask worker, SqlTrans transaction, bool insert)
        {
  
            int i = 0;
            foreach (T dpo in list)
            {
                transaction.Add(dpo);

                if (insert)
                {
                    if (dpo.Identity.Length > 0)
                        dpo.InsertIdentityOn = true;

                    dpo.Insert();
                }
                else
                    dpo.Save();

                int progress = (int)(i * 100.0 / Count);
                worker.SetProgress(progress);
                i++;
            }


        }

        public void Unpack(SqlTrans transaction, bool insert)
        {

            foreach (T dpo in list)
            {
                //transaction.Add(dpo);

                if (insert)
                {
                    if (dpo.Identity.Length > 0)
                        dpo.InsertIdentityOn = true;

                    dpo.Insert();
                }
                else
                    dpo.Save();
            }


        }

    }
}
