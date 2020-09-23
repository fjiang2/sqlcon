using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Sys.Stdio;

namespace sqlcon
{
    class ZipFileReader
    {
        public static void ProcessZipArchive(string zipPath, Action<string> action)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                    {
                        ProcessFile(entry, action);
                    }
                }
            }
        }

        private static void ProcessFile(ZipArchiveEntry entry, Action<string> action)
        {
            cout.WriteLine($"processing {entry.FullName}");

            using (GZipStream decompressionStream = new GZipStream(entry.Open(), CompressionMode.Decompress, leaveOpen: true))
            using (var reader = new StreamReader(decompressionStream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    action(line);
                }
            }
        }
    }
}
