using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyPaste.classes
{
    class MyUtils
    {        
        public static void callProcess(string exe, string fullName)
        {            
            var command = exe + " " + fullName;
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe ";
            p.StartInfo.Arguments = "/c " + command;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }

        public static void delDirecotry(DirectoryInfo info)
        {
            foreach (var file in info.GetFiles()) file.Delete();

            foreach (var dir in info.GetDirectories()) dir.Delete();

            info.Delete();
        }
    }
}
