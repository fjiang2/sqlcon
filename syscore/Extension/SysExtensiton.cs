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
using System.Data;

namespace Sys
{
    public static class SysExtension
    {
       
        /// <summary>
        /// Make any string be a valid ident, remove invaild letter
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ident ToIdent(this string str)
        {
            string id = ident.Identifier(str);
            return new ident(id);
        }

        /// <summary>
        /// Has attribute(T) defined?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return GetAttributes<T>(type).Length != 0;
        }

        /// <summary>
        /// Get attributes(T) from type(argument)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T[] GetAttributes<T>(this Type type) where T : Attribute
        {
            return (T[])type.GetCustomAttributes(typeof(T), true);

        }

        /// <summary>
        /// Has interface defined in the type(argument)
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasInterface<TInterface>(this Type type)
        {
            Type[] I = type.GetInterfaces();
            foreach (Type i in I)
            {
                if (i == typeof(TInterface))
                    return true;
            }

            return false;

        }

        /// <summary>
        /// get innullable type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type InnullableType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GetGenericArguments()[0];
            else
                return type;
        }

        public static Assembly[] GetInstalledAssemblies()
        {
            /*
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name.StartsWith("Sys.") || assembly.GetName().Name.StartsWith("App."))
                .ToArray();
            */

            List<Assembly> list = new List<Assembly>();
            string path = Directory.GetCurrentDirectory();
            string[] wildcards = new string[] { "*.dll", "*.exe" };

            foreach (string wildcard in wildcards)
            {
                string[] files = Directory.GetFiles(path, wildcard, SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    try
                    {
                        string f = Path.GetFileNameWithoutExtension(file);
                        Assembly assembly = Assembly.Load(f);

                        string name = assembly.GetName().Name;
                        if (!name.StartsWith("DevExpress"))
                            list.Add(assembly);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return list.ToArray();
        }



        public static void CopyFields(object source, object sink)
        {
            Type t1 = source.GetType();
            Type t2 = sink.GetType();

            FieldInfo[] f2 = t2.GetFields();
            foreach (FieldInfo f in f2)
            {
                FieldInfo f1 = t1.GetField(f.Name);
                if (f1 != null)
                {
                    object val = f1.GetValue(source);
                    f.SetValue(sink, val);
                }
            }
        }


        /// <summary>
        /// write a file, create path/folds automatically
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        public static void WriteIntoFile(this string text, string fileName)
        {
            string path = System.IO.Path.GetDirectoryName(fileName);
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(text);
            }
        }


        #region Xml <==> DataSet

        public static DataSet ToDataSet(this string xml)
        {
            DataSet ds = new DataSet();
            return ToDataSet(xml, ds);
        }

        public static DataSet ToDataSet(this string xml, DataSet ds)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(xml);
                writer.Flush();
                stream.Position = 0;

                try
                {
                    ds.ReadXml(stream, XmlReadMode.ReadSchema);
                }
                catch (Exception)
                {
                    throw new Exception(xml);
                }
            }
            return ds;
        }


        public static string ToXml(this DataSet ds)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ds.WriteXml(stream, XmlWriteMode.WriteSchema);
                stream.Flush();
                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion


        #region Xml <==> DataTable

        public static DataTable ToDataTable(this string xml, DataTable dt)
        {
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(xml);
                writer.Flush();
                stream.Position = 0;

                try
                {
                    dt.ReadXml(stream);
                }
                catch (Exception)
                {
                    throw new Exception(xml);
                }
            }
            return dt;
        }


        public static string ToXml(this DataTable dt)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                dt.WriteXml(stream, XmlWriteMode.WriteSchema);
                stream.Flush();
                stream.Position = 0;

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion


    }
}
