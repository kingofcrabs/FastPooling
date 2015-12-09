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
            List<Process> processlist = Process.GetProcesses().ToList();
            if (processlist.Exists(x => x.MainWindowTitle == windowTitle))
            {
                var toClose = processlist.Where(x=>x.MainWindowTitle == windowTitle).ToList();
                toClose.ForEach(x => x.CloseMainWindow());
                
            }
        }
        static public void WriteResult(bool bok)
        {
            string file = Folders.GetOutputFolder() + "result.txt";
            File.WriteAllText(file, bok.ToString());
        }
        public static void WriteRetry(bool bRetry)
        {
            string sFile = Folders.GetOutputFolder() + "retry.txt";
            File.WriteAllText(sFile, bRetry.ToString());
        }

        internal static void WriteTotalDstWellCnt(int dstNeeded)
        {
            string sFile = Folders.GetOutputFolder() + "totalDstWellCnt.txt";
            File.WriteAllText(sFile, dstNeeded.ToString());
        }
    }
}
