﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileCopyPaste.classes
{
    class DataOp
    {
        // https://blog.csdn.net/joyhen/article/details/17798241
        static String XML=  "data.xml";
        private static void CreateXmlFile(string xmlpath)
        {
            XDocument doc = new XDocument(///创建XDocument类的实例
				                new XDeclaration("1.0", "utf-8", "yes"),///XML的声明，包括版本，编码，xml文件是否独立
				                new XElement("root",
                                    new XElement("path0", "D:\\Workspace\\godot\\FireEmblem3"),
                                    new XElement("path1", "D:\\Workspace\\python\\lex-talionis"),
                                    new XElement("path2", Environment.GetFolderPath(Environment.SpecialFolder.Desktop)),
                                    new XElement("key_paste", "ctrl+r"),
                                    new XElement("key_refresh_tree0", "f1"),
                                    new XElement("key_delete", "Delete"),
                                    new XElement("key_enter", "Return"),
                                    new XElement("key_new_folder", "n"),
                                    new XElement("open_file_apps",
                                        new XElement("app",
                                            new XElement("file_ext","png,jpg,bmp,jpeg"),
                                            new XElement("open_exe", "start"),
                                            new XElement("use_cmd", "1")
                                        ),
                                        new XElement("app",
                                            new XElement("file_ext", "txt"),
                                            new XElement("open_exe", "notepad++"),
                                            new XElement("use_cmd", "0")
                                        )
                                    )
                                )
                            );
            ///保存XML文件到指定地址
            doc.Save(xmlpath);
        }
        private static void IfnotExistcreateFile()
        {
            if (!File.Exists(XML))
            {
                CreateXmlFile(XML);
            }
        }
        public static string ReadKey(string key)
        {
            IfnotExistcreateFile();
            XElement xe = XElement.Load(XML);
            var tt = xe.Element(key);
            var value = tt.Value;
            return value;
        }

        public static bool SetKey(String key,String value)
        {
            IfnotExistcreateFile();
            //Boolean success = true;
            XElement root = XElement.Load(XML);
            var node = root.Element(key);
            node.Value = value;
            root.Save(XML);
            return true;
        }

        public class OpenFileApp
        {
            public OpenFileApp(String ext,String exe,String useCmd)
            {
                this.ext = ext;
                this.exe = exe;
                if (!String.IsNullOrEmpty(useCmd) && (useCmd.Equals("1") || useCmd.ToLower().Equals("true")))
                {
                    this.useCmd = true;
                }
                else this.useCmd = false;
            }
            private String ext;
            private String exe;
            private bool useCmd;

            public bool isSameExt(string extension)
            {
                bool same = false;                
                if (!String.IsNullOrEmpty(extension) && !String.IsNullOrEmpty(ext) && !String.IsNullOrEmpty(exe))
                {
                    extension = extension.Substring(1);
                    var exts = ext.Split(',');
                    foreach(var item in exts)
                    {
                        if (item.ToLower().Equals(extension.ToLower()))
                        {
                            same = true;
                            break;
                        }
                    }
                }                
                return same;
            }

            public void openFile(string fullName)
            {
                MyUtils.callProcess(exe, fullName);
            }
        }

        public static List<OpenFileApp> ReadOpenFileApps()
        {
            List<OpenFileApp> list = new List<OpenFileApp>();
            IfnotExistcreateFile();
            XElement root = XElement.Load(XML);
            var apps = root.Element("open_file_apps").Elements("app");
            foreach(var app in apps)
            {
                var use_cmd = "";
                if (app.Element("use_cmd") != null) use_cmd = app.Element("use_cmd").Value;
                var openApp = new OpenFileApp(app.Element("file_ext").Value,app.Element("open_exe").Value, use_cmd);
                list.Add(openApp);
            }
            return list;
        }
    }
}
