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
using System.IO;
using Sys;
using Sys.Data;


namespace Sys.Data.Manager
{
 

    public class Unpacking
    {

        public static void Unpack(Level level, BackgroundTask worker, bool insert)
        {
            SqlTrans transaction = new SqlTrans();

            Assembly[] assemblies = SysExtension.GetInstalledAssemblies();
            
            int i = 0;
            foreach (Assembly asm in assemblies)
            {
                int progress = (int)(i * 100.0 / assemblies.Length);
                worker.SetProgress(progress, 0, asm.GetName().Name);

                Unpack(level, asm, worker, transaction, insert);
                
                i++;
            }

            transaction.Commit();

            return;
        }

        private static void Unpack(Level level, Assembly asm, BackgroundTask worker, SqlTrans transaction, bool insert)
        {
            
            foreach (Type type in asm.GetExportedTypes())
            {
                if (type.HasInterface<IPacking>() && type != typeof(BasePackage<>))
                {
                    IPacking packing = (IPacking)Activator.CreateInstance(type);
                    if (level == packing.Level)
                    {
                        worker.SetProgress(0, string.Format("Unpacking #record={0} of table [{1}].", packing.Count, packing.TableName));
                        packing.Unpack(worker, transaction, insert);
                    }
                }
            }

            return;
        }


        public static void Unpack(Assembly asm, bool insert)
        {
            SqlTrans transaction = new SqlTrans();

            
            foreach (Type type in asm.GetExportedTypes())
            {
                if (type.HasInterface<IPacking>())
                {
                    IPacking packing = (IPacking)Activator.CreateInstance(type);
                    packing.Unpack(transaction, insert);
                }
            }

            transaction.Commit();
            return;
        }

        #region Create Table
        
        
        /// <summary>
        ///  Create basic tables of PersistentObjects
        /// </summary>
        private static Type[] CreateBasicTable()
        {
            
            Type[] basicTypes = new Type[] { 
                //typeof(dictDatabaseDpo), 
                //typeof(dictDataTableDpo), 
                //typeof(dictDataColumnDpo), 
                //typeof(dictEnumTypeDpo), 
                //typeof(logDataSetDpo), 
                //typeof(logDataTableDpo), 
                //typeof(logDataColumnDpo),
                //typeof(RecordLockDpo),
                //typeof(DataProviderDpo),
                //typeof(logDpoClassDpo)
            };

            foreach (Type type in basicTypes)
            {
                DPObject dpo = (DPObject)Activator.CreateInstance(type);
                dpo.CreateTable();
            }

            foreach (Type type in basicTypes)
            {
                DPObject dpo = (DPObject)Activator.CreateInstance(type);
                //dpo.RegisterEntireTable();
            }
             
            return basicTypes;
        }



        /// <summary>
        /// Create tables used for installation
        /// </summary>
        /// <param name="level"></param>
        /// <param name="worker"></param>
        public static void CreateTable(Level level, BackgroundTask worker)
        {

            Type[] baiscTypes = CreateBasicTable();
            Assembly basicAssembly = baiscTypes[0].Assembly;

            Assembly[] assemblies = SysExtension.GetInstalledAssemblies();
            
            int i = 0;
            foreach (Assembly asm in assemblies)
            {

                MessageBuilder messages = new MessageBuilder();
                foreach (Type type in asm.GetTypes())
                {
                    if (type.BaseType != typeof(DPObject))
                        continue;

                    if (asm == basicAssembly && Array.IndexOf(baiscTypes, type) >= 0)
                        continue;

                    TableAttribute[] A = type.GetAttributes<TableAttribute>();
                    if (A.Length == 0)
                        continue;

      
                    if (A[0].Level == level)
                        messages.AddRange(CreateTable(type));
                }

                worker.ReportProgress((int)(i * 100.0 / assemblies.Length), messages.ToString());
                i++;

            }

        }


        public static MessageBuilder CreateTable(Assembly assembly)
        {
            MessageBuilder messages = new MessageBuilder();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(DPObject))
                    continue;

                if (!type.HasAttribute<TableAttribute>())
                    continue;

                messages.AddRange(CreateTable(type));
            }

            return messages;

        }


        private static MessageBuilder CreateTable(Type dpoType)
        {
            MessageBuilder messages = new MessageBuilder();

            DPObject dpo = (DPObject)Activator.CreateInstance(dpoType);

            int tableId = dpo.TableName.Id;

            if (dpo.CreateTable())
            {
                messages.Add(Message.Information(string.Format("Table {0} created.", dpo.TableName)));

                //if (tableId == -1)  //register new table to dictionary
                    //tableId = dpo.RegisterEntireTable();
            }
            else
                messages.Add(Message.Information(string.Format("Table {0} exists, cannot be created.", dpo.TableName)));

            return messages;

        }
        
        #endregion

    }
}
