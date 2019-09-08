using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using Tie;
using Sys.Stdio;

namespace sqlcon
{
    class ConnectionString
    {
        public static string SearchTieConnectionString(VAL val, Memory DS)
        {
            if (val.Size != 2)
            {
                cerr.WriteLine("required 2 parameters on function cfg(file,variable), 1: app.cfg/web.cfg name; 2: variable to reach connection string");
                return null;
            }

            if (val[0].VALTYPE != VALTYPE.stringcon || val[1].VALTYPE != VALTYPE.stringcon)
            {
                cerr.WriteLine("error on function cfg(file,variable) argument type, 1: string, 2: string");
                return null;
            }

            string cfgFile = (string)val[0];
            string variable = (string)val[1];

            try
            {
                Memory localDS = new Memory();
                if (File.Exists(cfgFile))
                {
                    using (var reader = new StreamReader(cfgFile))
                    {
                        string code = reader.ReadToEnd();
                        try
                        {
                            Script.Execute(code, localDS);
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine($"configuration file format error in {cfgFile}, {ex.Message}");
                            return null;
                        }
                    }
                }
                else
                {
                    cerr.WriteLine($"cannot find configuration file: {cfgFile}");
                    return null;
                }

                VAL value = localDS.GetValue(variable);
                if (value.Undefined)
                {
                    cerr.WriteLine($"undefined variable {variable}");
                    return null;
                }
                else if (!(value.Value is string))
                {
                    cerr.WriteLine($"connection string must be string, {variable}={value}");
                    return null;
                }
                else
                    return cleanConnectionString((string)value);
            }
            catch (Exception)
            {
                cerr.WriteLine($"cannot find connection string on {cfgFile}, variable={variable}");
                return null;
            }
        }

        /// <summary>
        /// search *.config file
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string SearchConnectionString(string xmlFile, string path, string valueAttr)
        {
            if (!File.Exists(xmlFile))
            {
                cerr.WriteLine($"warning: not found {xmlFile}");
                return null;
            }

            string[] segments = path.Split('|');
            XElement X = XElement.Load(xmlFile);
            for (int i = 0; i < segments.Length - 1; i++)
            {
                X = X.Element(segments[i]);
            }

            string attr = segments.Last();
            string[] pair = attr.Split('=');
            var connectionString = X.Elements()
                .Where(x => x.Attribute(pair[0]).Value == pair[1])
                .Select(x => x.Attribute(valueAttr).Value)
                .FirstOrDefault();

            return cleanConnectionString(connectionString);
        }

        internal static string SearchXmlConnectionString(VAL val)
        {
            if (val.Size != 3)
            {
                cerr.WriteLine("required 2 parameters on function config(file,path,value), 1: app.config/web.config name; 2: path to reach connection string; 3:connection string attribute");
                return null;
            }

            if (val[0].VALTYPE != VALTYPE.stringcon || val[1].VALTYPE != VALTYPE.stringcon || val[2].VALTYPE != VALTYPE.stringcon)
            {
                cerr.WriteLine("error on function config(file,path,value) argument type, 1: string, 2: string, 3:string");
                return null;
            }

            string xmlFile = (string)val[0];
            string path = (string)val[1];
            string value = (string)val[2];

            try
            {
                return ConnectionString.SearchConnectionString(xmlFile, path, value);
            }
            catch (Exception)
            {
                cerr.WriteLine($"cannot find connection string on {xmlFile}, path={path}");
                return null;
            }
        }

        private static string cleanConnectionString(string connectionString)
        {
            string[] L = connectionString.Split(';');
            for (int i = 0; i < L.Length; i++)
            {
                if (L[i].ToUpper() == "Provider=sqloledb".ToUpper())
                {
                    connectionString = connectionString.Replace(L[i] + ";", "");
                    break;
                }
            }

            return connectionString;
        }

    }
}
