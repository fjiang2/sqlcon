using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Data.Common;
using System.Windows.Media;
using System.IO;
using Sys.Stdio;

namespace sqlcon
{
    static class Helper
    {
        public static string OutputFile(this ApplicationCommand cmd)
        {
            string outputFile = cmd.OutputPath();
            if (!string.IsNullOrEmpty(outputFile))
            {
                try
                {
                    if (Directory.Exists(outputFile))
                    {
                        string file = Path.GetFileName(cmd.Configuration.OutputFile);
                        return Path.Combine(outputFile, "sqlcon.out");
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(outputFile);
                        if (directory != string.Empty && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        return outputFile;
                    }
                }
                catch (Exception ex)
                {
                    cerr.WriteLine($"invalid file \"{outputFile}\", {ex.Message}");
                }
            }

            return cmd.Configuration.OutputFile;
        }


        public static StreamWriter CreateStreamWriter(this string fileName, bool append = false)
        {
            try
            {
                string folder = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch (ArgumentException)
            {
            }

            return new StreamWriter(fileName, append);
        }



        public static bool parse(this string arg, out string t1, out string t2)
        {
            if (string.IsNullOrEmpty(arg) || arg.StartsWith("/"))
            {
                t1 = null;
                t2 = null;
                return false;
            }

            string[] x = arg.Split(':');
            if (x.Length == 1)
            {
                t1 = x[0];
                t2 = x[0];
            }
            else
            {
                t1 = x[0];
                t2 = x[1];
            }

            return true;
        }



        public static bool IsGoodConnectionString(this SqlConnectionStringBuilder cs)
        {
            SqlConnection conn = new SqlConnection(cs.ConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("EXEC sp_databases", conn);
                cmd.ExecuteScalar();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }

            return true;
        }

        public static string Message(this SqlException ex)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                var err = ex.Errors[i];
                builder.AppendLine($"Msg {err.Number}, Level {err.Class}, State {err.State}, Line {err.LineNumber}");
                builder.AppendLine(err.Message);
            }

            return builder.ToString();
        }
    }
}
