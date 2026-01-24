using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreUtils
{
    public class LoggerFileWriterOptions
    {
        public string LogDir { get; set; } = "logs";
        public string Template_Filename { get; set; } = "log_{date}_{index}.txt";
        public string Template_Filename_Dateformat { get; set; } = "yyyyMMddHHmm";
        public string Log_Dateformat { get; set; } = "yyyy-MM-dd-HH:mm:ss";
        public int MaxFileSize { get; set; } = 100000000;
        public int FilesToKeep { get; set; } = 10;
        public Func<DateTime, string> FuncGetFileNameDateStr = null;
    }

    public class LoggerFileWriter : IDisposable
    {

        public string CurrentFilePath = "";
        public string CurrentFileName = "";
        public long CurrentFileSize = 0;
        public int CurrentIndex = 1;

        string Template_Filename = "";
        string Template_Ext = "";
        public FileStream FSStream = null;

        object lockObj = new object();
        byte[] newLine = null;
        public long linesWritten = 0;

        public LoggerFileWriterOptions Options { get; set; }

        public LoggerFileWriter(LoggerFileWriterOptions opts)
        {
            this.Options = opts;
            if (!Directory.Exists(this.Options.LogDir))
            {
                Directory.CreateDirectory(this.Options.LogDir);
            }

            this.Template_Filename = Path.GetFileNameWithoutExtension(this.Options.Template_Filename);
            this.Template_Ext = Path.GetExtension(this.Options.Template_Filename);

            GetCurrentInfo();
            IncrementFileCheck(0);
            newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        }

        public void GetCurrentInfo()
        {
            this.CurrentFileName = GetFileName(1);
            this.CurrentFilePath = Path.Combine(this.Options.LogDir, this.CurrentFileName);
            this.CurrentFileSize = 0;

            if (File.Exists(this.CurrentFilePath))
            {
                FileInfo fileInfo = new FileInfo(this.CurrentFilePath);
                this.CurrentFileSize = fileInfo.Length;
            }
        }

        void IncrementFileCheck(long lengthToWrite)
        {
            bool increment = false;
            if (this.CurrentFileSize + lengthToWrite > this.Options.MaxFileSize)
            {
                increment = true;
            }

            if (increment)
            {
                if (this.FSStream != null)
                {
                    this.FSStream.Flush();
                    this.FSStream.Close();
                    this.FSStream.Dispose();
                    this.FSStream = null;
                }

                //get all files that match the extension
                List<FileInfo> infos = new List<FileInfo>();
                string searchPattern = this.Template_Filename.Replace("{date}", "*").Replace("{index}", "*") + this.Template_Ext;
                var filesWithExt = Directory.GetFiles(this.Options.LogDir, searchPattern);
                foreach (var file in filesWithExt)
                {
                    infos.Add(new FileInfo(file));
                }

                var ordered = infos.OrderBy(f => f.CreationTime).ToArray();
                if (ordered.Length >= this.Options.FilesToKeep)
                {
                    int deleteCount = (ordered.Length - this.Options.FilesToKeep) + 1;
                    for (int i = 0; i < deleteCount; i++)
                    {
                        File.Delete(ordered[i].FullName);
                    }
                }

                string newFileName = "";
                while (true)
                {
                    newFileName = GetFileName(this.CurrentIndex);
                    if (File.Exists(Path.Combine(this.Options.LogDir, newFileName)))
                    {
                        this.CurrentIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                this.CurrentFilePath = Path.Combine(this.Options.LogDir, newFileName);
                this.CurrentFileName = newFileName;

                if (File.Exists(this.CurrentFilePath))
                {
                    FileInfo info = new FileInfo(this.CurrentFilePath);
                    this.CurrentFileSize = info.Length;
                }
                else
                {
                    this.CurrentFileSize = 0;
                }
            }
        }

        string GetFileNameDateString()
        {
            if (this.Options.FuncGetFileNameDateStr != null)
            {
                return this.Options.FuncGetFileNameDateStr(DateTime.Now);
            }
            return DateTime.Now.ToString(this.Options.Template_Filename_Dateformat);
        }

        string GetFileName(int index)
        {
            string dateString = this.GetFileNameDateString();
            string indexString = index.ToString().PadLeft(3, '0');
            return this.Template_Filename.Replace("{date}", dateString).Replace("{index}", indexString) + this.Template_Ext;
        }

        public void Write(string log)
        {
            lock (lockObj)
            {
                byte[] bytesToWrite = Encoding.UTF8.GetBytes(log);

                IncrementFileCheck(bytesToWrite.Length);

                if (this.FSStream == null)
                {
                    this.FSStream = File.Open(this.CurrentFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    this.FSStream.Seek(0, SeekOrigin.End);
                }
                byte[] dateBytes = Encoding.UTF8.GetBytes(DateTime.Now.ToString(this.Options.Log_Dateformat) + " - ");

                this.FSStream.Write(dateBytes);
                this.FSStream.Write(bytesToWrite);
                this.FSStream.Write(newLine);
                this.FSStream.Flush();
                linesWritten++;
                this.CurrentFileSize += bytesToWrite.Length;
            }
        }

        public void Dispose()
        {
            if (this.FSStream != null)
            {
                this.FSStream.Flush();
                this.FSStream.Close();
                this.FSStream.Dispose();
                this.FSStream = null;
            }
        }
    }
}
