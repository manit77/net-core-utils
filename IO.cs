using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;

namespace CoreUtils
{
    public class IO
    {
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static string CurrentDirectory()
        {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\\\", "").Replace("file:\\", "");
        }
        public static string CurrentDrive()
        {
            return Path.GetPathRoot(System.Reflection.Assembly.GetEntryAssembly().Location);
        }
        public static void CreateTextFile(string path, string content, bool autocreatedirectory = true)
        {
            if (autocreatedirectory)
            {
                if (!string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(path)))
                {
                    CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }
            }
            using StreamWriter w = File.CreateText(path);
            w.Write(content);
            w.Flush();
        }
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
