using OptimizePooling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastPooling
{
    class Helper
    {
        public static void CloseWaiter(string windowTitle)
        {
            Thread.Sleep(1000);
            windowTitle = windowTitle.ToLower();
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (process.MainWindowTitle.ToLower().Contains(windowTitle))
                    {
                        process.CloseMainWindow();
                    }
                }
            }
        }
        static public void WriteResult(bool bok)
        {
            string file = Folders.GetOutputFolder() + "result.txt";
            File.WriteAllText(file, bok.ToString());
        }
        public static void WriteRetry(bool bRetry)
        {
            string sFile = Folders.GetExeFolder() + "retry.txt";
            File.WriteAllText(sFile, bRetry.ToString());
        }
    }
}
