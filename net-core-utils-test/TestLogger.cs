using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreUtils;
using FluentAssertions;
using Xunit;

namespace net_core_utils_test
{
    [Collection("SequentialLoggerTests")]
    public class TestLogger : IDisposable
    {
        private const string TestDir = "testlogs";

        public TestLogger()
        {
            // Cleanup before each test
            if (Directory.Exists(TestDir))
                Directory.Delete(TestDir, true);
        }

        public void Dispose()
        {
            // Cleanup after tests
            if (Directory.Exists(TestDir))
                Directory.Delete(TestDir, true);
        }

        [Fact(DisplayName = "TestLoggerInit")]
        public void TestLoggerInit()
        {
            var currentDateStr = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions
            {
                MaxFileSize = 300,
                FilesToKeep = 3,
                LogDir = TestDir,
                TemplateFilename = "log_{date}_{index}.txt",
                FuncGetFileNameDateStr = dt => currentDateStr
            };

            using var logger = new LoggerFileWriter(options);

            // logger.Options.LogDir.Should().Be(TestDir);
            // logger.CurrentFileName.Should().Be($"log_{currentDateStr}_001.txt");
            // logger.CurrentFilePath.Should().Be(Path.Combine(TestDir, logger.CurrentFileName));
            // logger.CurrentFileSize.Should().Be(0);
        }

        [Fact(DisplayName = "TestWriteLog")]
        public void TestWriteLog()
        {
            var currentDateStr = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions
            {
                MaxFileSize = 300,
                FilesToKeep = 3,
                LogDir = TestDir,
                TemplateFilename = "log_{date}_{index}.txt",
                FuncGetFileNameDateStr = dt => currentDateStr
            };

            using var logger = new LoggerFileWriter(options);
            logger.Write("hello");

            File.Exists(Path.Combine(TestDir, $"log_{currentDateStr}_001.txt"))
                .Should().BeTrue("the log file should have been created");
        }

        [Fact(DisplayName = "TestWriteLogRollingLogs")]
        public void TestWriteLogRollingLogs()
        {
            var currentDateStr = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions
            {
                MaxFileSize = 10,
                FilesToKeep = 4,
                LogDir = TestDir,
                TemplateFilename = "log_{date}_{index}.txt",
                FuncGetFileNameDateStr = dt => currentDateStr
            };

            using var logger = new LoggerFileWriter(options);

            for (int i = 1; i <= 4; i++)
            {
                logger.Write($"Hello World! line{i}");
            }

            logger.Dispose();

            var files = Directory.GetFiles(TestDir).OrderBy(f => f).ToArray();
            files.Should().HaveCount(4, "four rolling log files should exist");

            foreach (var i in Enumerable.Range(1, 4))
            {
                var expectedFile = Path.Combine(TestDir, $"log_{currentDateStr}_{i:000}.txt");
                files.Should().Contain(expectedFile, $"file {i} should exist");
                var contents = File.ReadAllText(expectedFile);
                contents.Should().Contain($"Hello World! line{i}", $"file {i} should contain its log line");
            }

            logger.LinesWritten.Should().Be(4, "only first four lines should be in previous files before rolling");
        }

        [Fact(DisplayName = "TestWriteLogPerfTest")]
        public async Task TestWriteLogPerfTest()
        {
            var currentDateStr = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmm");
            var options = new LoggerFileWriterOptions
            {
                MaxFileSize = 300,
                FilesToKeep = 4,
                LogDir = TestDir,
                TemplateFilename = "log_{date}_{index}.txt",
                FuncGetFileNameDateStr = dt => currentDateStr
            };

            await using var logger = new LoggerFileWriter(options);

            async Task WriteRange(int start, int end)
            {
                for (int i = start; i < end; i++)
                {
                    await logger.WriteAsync($"Line {i}");
                }
            }

            var tasks = new[]
            {
                WriteRange(0, 400),
                WriteRange(400, 800),
                WriteRange(800, 1000)
            };

            await Task.WhenAll(tasks);

            logger.LinesWritten.Should().Be(1000, "all lines should have been written concurrently");
        }
    }
}
