using System;
using System.IO;
using System.Text;

namespace CoreUtils
{
    public class IO
    {
        public static void AppendTextFile(string path, string content)
        {
            using FileStream w = File.OpenWrite(path);
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            w.Write(bytes);
            w.Flush();
        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static string CurrentDirectory()
        {
            return AppContext.BaseDirectory;
        }
        public static string CurrentDrive()
        {
            return Path.GetPathRoot(AppContext.BaseDirectory);
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

        public static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        
        public static void DeleteFiles(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                // Get list of files in the directory
                string[] files = Directory.GetFiles(directoryPath);

                foreach (string file in files)
                {
                    // Delete the file
                    File.Delete(file);
                }
            }
        }

        public static string ReadFile(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }

            return "";
        }
        public static string[] GetFiles(string dir)
        {
            if (Directory.Exists(dir))
            {
                return Directory.GetFiles(dir);
            }
            return [];
        }
    }
}
