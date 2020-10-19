using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Sys.Stdio;

namespace Sys
{

    public static class ConfigurationFile
    {
        private const string USER_CFG_TEMPLATE = "user.ini";

        public const string USER_CFG = "user.cfg";

        public static string Company { get; set; } = GetAttribute<AssemblyConfigurationAttribute>().Configuration;
        public static string ProductName { get; private set; } = GetAttribute<AssemblyProductAttribute>().Product;
        public static string MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + ProductName;

        public static Configuration Load(string productName = null)
        {
            if (productName != null)
                ProductName = productName;

            var cfgs = PrepareConfiguration(false);

            var Configuration = new Configuration();
            Configuration.SetValue("MyDocuments", MyDocuments);

            try
            {
                if (!Configuration.Initialize(cfgs))
                    return null;
            }
            catch (Exception ex)
            {
                cout.WriteLine("error on configuration file {0}, {1}:", cfgs.User, ex.Message);
                return null;
            }

            return Configuration;
        }

        private static T GetAttribute<T>() where T : Attribute
        {
            T[] attributes = (T[])Assembly.GetEntryAssembly().GetCustomAttributes(typeof(T), false);
            return attributes[0];
        }

        public static ConfigFiles PrepareConfiguration(bool overwrite)
        {
            string productCfgFile = $"{ProductName}.cfg";

            string usercfgFile = PrepareUserConfiguration(false);

            var cfg = new ConfigFiles
            {
                Product = productCfgFile,
                User = usercfgFile
            };

            return cfg;
        }

        private static string PrepareUserConfiguration(bool overwrite)
        {
            string cfgFile = USER_CFG;
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            folder = Path.Combine(folder, Company, ProductName);
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
                        if (File.Exists(USER_CFG_TEMPLATE))
                        {
                            //copy user.cfg template
                            File.Copy(USER_CFG_TEMPLATE, file);
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
