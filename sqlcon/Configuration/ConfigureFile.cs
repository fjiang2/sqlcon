using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Sys.Stdio;
using Sys;

namespace sqlcon
{
    public class ConfigureFile
    {
        private const string _USER_CFG_TEMPLATE = "user.ini";

        public const string _USER_CFG = "user.cfg";

        public readonly static string Company = GetAttribute<AssemblyConfigurationAttribute>().Configuration;
        public readonly static string Product = GetAttribute<AssemblyProductAttribute>().Product;

        public ConfigureFile()
        {
        }

        public static Configuration Load()
        {
            string usercfgFile = PrepareUserConfigureFile(false);

            var Configuration = new Configuration();
            try
            {
                if (!Configuration.Initialize(usercfgFile))
                    return null;
            }
            catch (Exception ex)
            {
                cout.WriteLine("error on configuration file {0}, {1}:", usercfgFile, ex.Message);
                return null;
            }

            return Configuration;
        }

        private static T GetAttribute<T>() where T : Attribute
        {
            T[] attributes = (T[])Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false);
            return attributes[0];
        }

        public static string PrepareUserConfigureFile(bool overwrite)
        {
            string cfgFile = _USER_CFG;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folder = Path.Combine(folder, Company, Product);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            bool exists = File.Exists(cfgFile);
            string file = Path.Combine(folder, cfgFile);
            try
            {
                if (!File.Exists(file))
                {
                    if (!exists)
                    {
                        if (File.Exists(_USER_CFG_TEMPLATE))
                        {
                            //copy user.cfg template
                            File.Copy(_USER_CFG_TEMPLATE, file);
                        }
                        else
                        {
                            //create empty text file if file is missing
                            File.Create(file).Dispose();
                        }
                    }
                    else
                        File.Copy(cfgFile, file);

                    return file;
                }

                if (exists)
                {
                    FileInfo f = new FileInfo(file);
                    if (f.Length == 0)
                        overwrite = true;

                    if (!overwrite)
                    {
                        FileInfo c = new FileInfo(cfgFile);
                        overwrite = c.LastWriteTime > f.LastWriteTime;
                    }

                    if (overwrite)
                        File.Copy(cfgFile, file, true);
                }
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"failed to initialize {file}, {ex.Message}");
            }

            return file;
        }
    }
}
