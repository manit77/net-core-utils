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

            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter("testlogs", "testlog.txt", 200, 3);

            Console.WriteLine($"CurrentDir: {logger.CurrentDir}");
            Console.WriteLine($"FileNameTemplate: {logger.FileNameTemplate}");
            Console.WriteLine($"CurrentFileName: {logger.CurrentFileName}");
            // Console.WriteLine($"CurrentFileIndex: {logger.CurrentFileIndex}");
            Console.WriteLine($"CurrentFilePath: {logger.CurrentFilePath}");
            Console.WriteLine($"CurrentFileSize: {logger.CurrentFileSize}");

            Assert.AreEqual("testlogs", logger.CurrentDir);
            Assert.AreEqual("testlog.txt", logger.FileNameTemplate);
            Assert.AreEqual("testlog_001.txt", logger.CurrentFileName);
            Assert.AreEqual("testlogs\\testlog_001.txt", logger.CurrentFilePath);
            Assert.AreEqual(0, logger.CurrentFileSize);
            Assert.AreEqual(200, logger.MaxFileSize);
            Assert.AreEqual(3, logger.FilesToKeep);
        }

        [TestMethod]
        public void TestWriteLog()
        {
            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter("testlogs", "testlog.txt", 200, 3);
            var bytes = new byte[200];
            string str = Encoding.UTF8.GetString(bytes);
            logger.WriteLog(str);
        }

        [TestMethod]
        public void TestWriteLogRollingLogs()
        {
            Directory.Delete("testlogs", true);

            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter("testlogs", "testlog.txt", 200, 3);
            var bytes = new byte[200];

            string log = "Hello World! line1";
            byte[] logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            string str = Encoding.UTF8.GetString(bytes);
            logger.WriteLog(str);

            log = "Hello World! line2";
            logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            str = Encoding.UTF8.GetString(bytes);
            logger.WriteLog(str);

            log = "Hello World! line3";
            logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            str = Encoding.UTF8.GetString(bytes);
            logger.WriteLog(str);

            var files = Directory.GetFiles("testlogs");
            Assert.AreEqual(3, files.Length);

            Assert.AreEqual("testlogs\\testlog_001.txt", files[0]);
            Assert.AreEqual("testlogs\\testlog_002.txt", files[1]);
            Assert.AreEqual("testlogs\\testlog_003.txt", files[2]);


            //this will roll the logs
            log = "Hello World! line4";
            logBytes = Encoding.UTF8.GetBytes(log);
            logBytes.CopyTo(bytes, 0);
            str = Encoding.UTF8.GetString(bytes);
            logger.WriteLog(str);

            logger.Dispose();

            files = Directory.GetFiles("testlogs");
            Assert.AreEqual(3, files.Length);

            Assert.AreEqual("testlogs\\testlog_001.txt", files[0]);
            Assert.AreEqual("testlogs\\testlog_002.txt", files[1]);
            Assert.AreEqual("testlogs\\testlog_003.txt", files[2]);

            string contents = File.ReadAllText("testlogs\\testlog_001.txt");
            Assert.IsTrue(contents.Contains("Hello World! line4"));

            contents = File.ReadAllText("testlogs\\testlog_002.txt");
            Assert.IsTrue(contents.Contains("Hello World! line3"));

            contents = File.ReadAllText("testlogs\\testlog_003.txt");
            Assert.IsTrue(contents.Contains("Hello World! line2"));

        }


        [TestMethod]
        public void TestWriteLogPerfTest()
        {
            Directory.Delete("testlogs", true);

            CoreUtils.LoggerFileWriter logger = new CoreUtils.LoggerFileWriter("testlogs", "testlog.txt", 1000, 3);
            byte[] bytes = null;
            string log = "";
            byte[] logBytes = null;
            for (int i = 0; i < 1000; i++)
            {
                bytes = new byte[100];
                log = "Line " + i;                
                logBytes = Encoding.UTF8.GetBytes(log);
                logBytes.CopyTo(bytes, 0);                
                logger.WriteLog(Encoding.UTF8.GetString(bytes));
            }

        }
    }
}
