using Org.BouncyCastle.Crypto.Engines;
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
    public class LoggerFileWriter : IDisposable
    {
        public string CurrentDir = "";
        public string CurrentFilePath = "";
        public string CurrentFileName = "";
        // public int CurrentFileIndex = 0;
        public long CurrentFileSize = 0;
        public string FileNameTemplate = "";

        public long MaxFileSize = 0;
        public long FilesToKeep = 0;
        string Template_Filename = "";
        string Template_Ext = "";

        public FileStream FSStream = null;

        object lockObj = new object();

        public LoggerFileWriter(string logDir = "logs", string filenameTemplate = "log.txt", int maxFileSizeBytes = 100000000, int filesToKeep = 10)
        {
            CoreUtils.IO.CreateDirectory(logDir);
            this.CurrentDir = logDir;
            this.FileNameTemplate = filenameTemplate;
            this.MaxFileSize = maxFileSizeBytes;
            this.FilesToKeep = filesToKeep;
            this.Template_Filename = Path.GetFileNameWithoutExtension(this.FileNameTemplate);
            this.Template_Ext = Path.GetExtension(this.FileNameTemplate);

            GetCurrentInfo();
            IncrementFileCheck(0);
        }

        public void GetCurrentInfo()
        {
            //load defaults
            this.CurrentFileName = this.Template_Filename + "_" + "1".PadLeft(3, '0') + this.Template_Ext;
            this.CurrentFilePath = Path.Combine(this.CurrentDir, this.CurrentFileName);
            this.CurrentFileSize = 0;            

            if(File.Exists(this.CurrentFilePath))
            {
                FileInfo fileInfo = new FileInfo(this.CurrentFilePath);
                this.CurrentFileSize = fileInfo.Length;
            }
        }

        void IncrementFileCheck(long lengthToWrite)
        {
            bool increment = false;
            if (this.CurrentFileSize + lengthToWrite > this.MaxFileSize)
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

                string filename = "";
                string newfilename = "";

                //move files 2 and up
                for (int i = Convert.ToInt32(this.FilesToKeep) - 1; i > 0; i--)
                {
                    filename = Path.Combine(this.CurrentDir, this.Template_Filename + "_" + i.ToString().PadLeft(3, '0') + Template_Ext);
                    newfilename = Path.Combine(this.CurrentDir, this.Template_Filename + "_" + (i + 1).ToString().PadLeft(3, '0') + Template_Ext);
                    if (File.Exists(filename))
                    {
                        File.Move(filename, newfilename, true);
                    }
                }                
                
                //this.CurrentFileName = this.Template_Filename + "_" + (1).ToString().PadLeft(3, '0') + Template_Ext;
                //this.CurrentFilePath = Path.Combine(this.CurrentDir, this.CurrentFileName);
                this.CurrentFileSize = 0;

                //delete the current file
                if (File.Exists(this.CurrentFilePath))
                {
                    File.Delete(this.CurrentFilePath);
                }
            }
        }

        public void WriteLog(string log)
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
                byte[] dateBytes = Encoding.UTF8.GetBytes(DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss") + " - ");

                this.FSStream.Write(dateBytes);
                this.FSStream.Write(bytesToWrite);
                this.FSStream.Write(Encoding.UTF8.GetBytes("\r\n"));
                this.FSStream.Flush();

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
