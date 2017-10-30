using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sqlcon
{
    sealed class stdio
    {

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cout.WriteLine();
            cout.WriteLine("exit application...");
        }



        public static void OpenEditor(string fileName)
        {
            const string notepad = "notepad.exe";
            if (!File.Exists(fileName))
            {
                cerr.WriteLine($"cannot find the file: {fileName}");
                return;
            }

            string editor = Context.GetValue<string>("editor", notepad);
            if (!Launch(fileName, editor))
            {
                if (editor != notepad)
                {
                    //try notepad.exe to open
                    Launch(fileName, notepad);
                }
            }

        }

        private static bool Launch(string fileName, string editor)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.ErrorDialog = true;
            process.StartInfo.UseShellExecute = false;
            //process.StartInfo.WorkingDirectory = startin;
            process.StartInfo.FileName = editor;
            process.StartInfo.Arguments = fileName;


            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"failed to lauch application: {editor} {fileName}, {ex.Message}");
                return false;
            }

            return true;
        }



    }
}
