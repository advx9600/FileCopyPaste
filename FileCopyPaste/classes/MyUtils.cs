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
            exe = CustomProcessString(exe);
            fullName = CustomProcessString(fullName);
            // https://superuser.com/questions/239565/can-i-use-the-start-command-with-spaces-in-the-path
            // start 命令包含空格时，特殊处理
            String startCmdSpecialProcessStr = exe.Trim().ToLower().Equals("start") && fullName.Contains("\"") ? "\"\"" : "";

            var command = exe + " " + startCmdSpecialProcessStr + " " + fullName;
            // https://stackoverflow.com/questions/6376113/how-do-i-use-spaces-in-the-command-prompt
            // 包含四个引号，如"c:/program file/vlc.exe" "1 2.ogg" 必须写成 ""c:/program file/vlc.exe" "1 2.ogg""
            if (command.Split('\"').Length > 4)
                command = "\""+command +"\"";
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe ";
            p.StartInfo.Arguments = "/c " + command;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }

        private static string CustomProcessString(String str)
        {
            if (!String.IsNullOrEmpty(str) && !str.Contains("\"") && ( str.Contains(" ")))
            {
                str = "\"" + str + "\"";
            }
            if (String.IsNullOrEmpty(str)) str = "";
            return str;
        }
        public static void delDirecotry(DirectoryInfo info)
        {            
            foreach (var file in info.GetFiles())
            {
                file.Delete();
                //try { 
                //    file.Delete();
                //}catch(Exception e)
                //{
                //    File.SetAttributes(file.FullName, FileAttributes.Normal);
                //    file.Delete();
                //}
            }
            foreach (var dir in info.GetDirectories())
            {
                delDirecotry(dir);
            }
            Console.WriteLine("dir   "+info.FullName);
            info.Delete();
        }

    }
}
