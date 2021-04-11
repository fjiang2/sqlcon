using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Sys.Stdio;

namespace sqlcon
{
    static class Helper
    {
        public static Version ApplicationVerison
        {
            get
            {
                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    System.Deployment.Application.ApplicationDeployment ad = System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
                    Version version = ad.CurrentVersion;
                    return version;
                }
                return System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
            }
        }

        public static string OutputFile(this ApplicationCommand cmd, string defaultOutputFile, bool createDirectoryIfNotExists = true)
        {
            string outputFile = cmd.OutputPath();
            if (!string.IsNullOrEmpty(outputFile))
            {
                try
                {
                    if (Directory.Exists(outputFile))
                    {
                        string directory = outputFile;
                        if (string.IsNullOrEmpty(defaultOutputFile))
                        {
                            return Path.Combine(directory, "sqlcon.out");
                        }
                        else
                        {
                            if (Path.IsPathRooted(defaultOutputFile))
                                return Path.Combine(directory, Path.GetFileName(defaultOutputFile));
                            else
                                return Path.Combine(directory, defaultOutputFile);
                        }
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(outputFile);
                        if (directory != string.Empty && !Directory.Exists(directory))
                        {
                            if (createDirectoryIfNotExists)
                                Directory.CreateDirectory(directory);
                        }

                        return outputFile;
                    }
                }
                catch (Exception ex)
                {
                    cerr.WriteLine($"invalid file or directory \"{outputFile}\", {ex.Message}");
                }
            }

            if (Path.IsPathRooted(defaultOutputFile))
            {
                return defaultOutputFile;
            }
            else
            {
                return Path.Combine(Directory.GetCurrentDirectory(), defaultOutputFile);
            }
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
