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
