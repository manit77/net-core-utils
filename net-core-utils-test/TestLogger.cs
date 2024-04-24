using CoreUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net_core_utils_test
{
    [TestClass]
    public class TestLogger
    {
        [TestMethod]
        public void TestLoggerInit()
        {
            //delete all files in the test folder
            Directory.Delete("testlogs", true);

            var currentDateStr = DateTime.Now.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions();
            options.MaxFileSize = 300;
            options.FilesToKeep = 3;
            options.LogDir = "testlogs";
            options.Template_Filename = "log_{date}_{index}.txt";
            options.FuncGetFileNameDateStr = (datetime) =>
            {
                return currentDateStr;
            };

            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter(options);

            Console.WriteLine($"LogDir: {logger.Options.LogDir}");
            Console.WriteLine($"Template_Filename: {logger.Options.Template_Filename}");
            Console.WriteLine($"CurrentFileName: {logger.CurrentFileName}");
            // Console.WriteLine($"CurrentFileIndex: {logger.CurrentFileIndex}");
            Console.WriteLine($"CurrentFilePath: {logger.CurrentFilePath}");
            Console.WriteLine($"CurrentFileSize: {logger.CurrentFileSize}");

            Assert.AreEqual("testlogs", logger.Options.LogDir);
            Assert.AreEqual($"log_{currentDateStr}_001.txt", logger.CurrentFileName);
            Assert.AreEqual($"testlogs\\log_{currentDateStr}_001.txt", logger.CurrentFilePath);
            Assert.AreEqual(0, logger.CurrentFileSize);
        }

        [TestMethod]
        public void TestWriteLog()
        {
            Directory.Delete("testlogs", true);

            var currentDateStr = DateTime.Now.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions();
            options.MaxFileSize = 300;
            options.FilesToKeep = 3;
            options.LogDir = "testlogs";
            options.Template_Filename = "log_{date}_{index}.txt";
            options.FuncGetFileNameDateStr = (datetime) =>
            {
                return currentDateStr;
            };

            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter(options);
            var bytes = new byte[200];
            string str = Encoding.UTF8.GetString(bytes);
            logger.Write(str);

            Assert.IsTrue(File.Exists(Path.Combine(options.LogDir, $"log_{currentDateStr}_001.txt")));
        }

        [TestMethod]
        public void TestWriteLogRollingLogs()
        {
            Directory.Delete("testlogs", true);
            var currentDateStr = DateTime.Now.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions();
            options.MaxFileSize = 300;
            options.FilesToKeep = 3;
            options.LogDir = "testlogs";
            options.Template_Filename = "log_{date}_{index}.txt";
            options.FuncGetFileNameDateStr = (datetime) =>
            {
                return currentDateStr;
            };

            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter(options);
            var bytes = new byte[200];

            string log = "Hello World! line1";
            byte[] logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            string str = Encoding.UTF8.GetString(bytes);
            logger.Write(str);

            log = "Hello World! line2";
            logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            str = Encoding.UTF8.GetString(bytes);
            logger.Write(str);

            log = "Hello World! line3";
            logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            str = Encoding.UTF8.GetString(bytes);
            logger.Write(str);

            var files = Directory.GetFiles("testlogs");
            Assert.AreEqual(3, files.Length);

            Assert.AreEqual(Path.Combine(options.LogDir, $"log_{currentDateStr}_001.txt"), files[0]);
            Assert.AreEqual(Path.Combine(options.LogDir, $"log_{currentDateStr}_002.txt"), files[1]);
            Assert.AreEqual(Path.Combine(options.LogDir, $"log_{currentDateStr}_003.txt"), files[2]);


            //this will roll the logs
            log = "Hello World! line4";
            logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            str = Encoding.UTF8.GetString(bytes);
            logger.Write(str);

            logger.Dispose();

            files = Directory.GetFiles("testlogs");
            Assert.AreEqual(3, files.Length);

            Assert.AreEqual(Path.Combine(options.LogDir, $"log_{currentDateStr}_002.txt"), files[0]);
            Assert.AreEqual(Path.Combine(options.LogDir, $"log_{currentDateStr}_003.txt"), files[1]);
            Assert.AreEqual(Path.Combine(options.LogDir, $"log_{currentDateStr}_004.txt"), files[2]);

            string contents = File.ReadAllText(Path.Combine(options.LogDir, $"log_{currentDateStr}_002.txt"));
            Assert.IsTrue(contents.Contains("Hello World! line2"));

            contents = File.ReadAllText(Path.Combine(options.LogDir, $"log_{currentDateStr}_003.txt"));
            Assert.IsTrue(contents.Contains("Hello World! line3"));

            contents = File.ReadAllText(Path.Combine(options.LogDir, $"log_{currentDateStr}_004.txt"));
            Assert.IsTrue(contents.Contains("Hello World! line4"));

        }

        [TestMethod]
        public async Task TestWriteLogPerfTest()
        {
            Directory.Delete("testlogs", true);

            var currentDateStr = DateTime.Now.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions();
            options.MaxFileSize = 300;
            options.FilesToKeep = 3;
            options.LogDir = "testlogs";
            options.Template_Filename = "log_{date}_{index}.txt";
            options.FuncGetFileNameDateStr = (datetime) =>
            {
                return currentDateStr;
            };

            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter(options);
            Action<int, int> action = (int start, int end) =>
            {
                byte[] bytes = null;
                string log = "";
                byte[] logBytes = null;
                for (int i = start; i < end; i++)
                {
                    bytes = new byte[100];
                    log = "Line " + i;
                    logBytes = Encoding.UTF8.GetBytes(log);
                    logBytes.CopyTo(bytes, 0);
                    logger.Write(Encoding.UTF8.GetString(bytes));                  
                }
            };

            List<Func<Task>> tasks = new List<Func<Task>>();
            tasks.Add(() => Task.Run(() => action(0, 400)));
            tasks.Add(() => Task.Run(() => action(400, 800)));
            tasks.Add(() => Task.Run(() => action(800, 1000)));
            //tasks.Add(() => Task.Run(() => action(667, 1000)));

            await Task.WhenAll(tasks.Select(func => func()));

            Assert.AreEqual(logger.linesWritten, 1000);


        }
    }
}
