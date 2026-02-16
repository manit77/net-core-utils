using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreUtils;

public sealed class LoggerFileWriterOptions
{
    public required string LogDir { get; init; } = "logs";
    public string TemplateFilename { get; init; } = "log_{date}_{index}.txt";
    public string FilenameDateFormat { get; init; } = "yyyyMMddHHmm";
    public string LogDateFormat { get; init; } = "yyyy-MM-dd HH:mm:ss.fff";
    public long MaxFileSize { get; init; } = 100_000_000;
    public int FilesToKeep { get; init; } = 10;
    public Func<DateTimeOffset, string>? FuncGetFileNameDateStr { get; init; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(LogDir))
            throw new ArgumentException("LogDir must be provided");

        if (MaxFileSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaxFileSize));

        if (FilesToKeep <= 0)
            throw new ArgumentOutOfRangeException(nameof(FilesToKeep));
    }
}



public sealed class LoggerFileWriter : IDisposable, IAsyncDisposable
{
    private readonly LoggerFileWriterOptions _options;
    private readonly object _lock = new();

    private FileStream? _stream;
    private string _currentFilePath = "";
    private long _currentFileSize;
    private int _currentIndex = 1;

    private readonly string _templateName;
    private readonly string _templateExt;
    private readonly byte[] _newline = Encoding.UTF8.GetBytes(Environment.NewLine);

    public long LinesWritten { get; private set; }

    public LoggerFileWriter(LoggerFileWriterOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        Directory.CreateDirectory(_options.LogDir);

        _templateName = Path.GetFileNameWithoutExtension(_options.TemplateFilename);
        _templateExt = Path.GetExtension(_options.TemplateFilename);

        InitializeCurrentFile();
    }

    private void InitializeCurrentFile()
    {
        var filename = GetFileName(_currentIndex);
        _currentFilePath = Path.Combine(_options.LogDir, filename);

        if (File.Exists(_currentFilePath))
        {
            _currentFileSize = new FileInfo(_currentFilePath).Length;
        }
        else
        {
            _currentFileSize = 0;
        }
    }

    private string GetFileNameDateString()
    {
        return _options.FuncGetFileNameDateStr?.Invoke(DateTimeOffset.UtcNow)
               ?? DateTimeOffset.UtcNow.ToString(_options.FilenameDateFormat);
    }

    private string GetFileName(int index)
    {
        var date = GetFileNameDateString();
        var idx = index.ToString("D3");
        return _templateName.Replace("{date}", date)
                             .Replace("{index}", idx)
                             + _templateExt;
    }

    private void RotateIfNeeded(long incomingBytes)
    {
        if (_currentFileSize + incomingBytes <= _options.MaxFileSize)
            return;

        CloseStream();

        CleanupOldFiles();

        while (true)
        {
            var filename = GetFileName(_currentIndex);
            var full = Path.Combine(_options.LogDir, filename);
            if (!File.Exists(full))
            {
                _currentFilePath = full;
                _currentFileSize = 0;
                break;
            }
            _currentIndex++;
        }
    }

    private void CleanupOldFiles()
    {
        var searchPattern = _templateName.Replace("{date}", "*").Replace("{index}", "*") + _templateExt;
        var files = Directory.GetFiles(_options.LogDir, searchPattern)
                             .Select(f => new FileInfo(f))
                             .OrderBy(f => f.LastWriteTimeUtc)
                             .ToList();

        var excess = files.Count - _options.FilesToKeep + 1;
        for (int i = 0; i < excess; i++)
        {
            files[i].Delete();
        }
    }

    private FileStream OpenStream()
    {
        return new FileStream(
            _currentFilePath,
            FileMode.OpenOrCreate,
            FileAccess.Write,
            FileShare.Read,
            bufferSize: 64 * 1024,
            options: FileOptions.SequentialScan);
    }

    public void Write(string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var payload = Encoding.UTF8.GetBytes(message);
        var datePrefix = Encoding.UTF8.GetBytes($"{DateTimeOffset.UtcNow.ToString(_options.LogDateFormat)} - ");

        lock (_lock)
        {
            RotateIfNeeded(payload.Length + datePrefix.Length + _newline.Length);

            _stream ??= OpenStream();
            _stream.Seek(0, SeekOrigin.End);

            _stream.Write(datePrefix);
            _stream.Write(payload);
            _stream.Write(_newline);

            _currentFileSize += payload.Length + datePrefix.Length + _newline.Length;
            LinesWritten++;
        }
    }

    public async ValueTask WriteAsync(string message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var payload = Encoding.UTF8.GetBytes(message);
        var datePrefix = Encoding.UTF8.GetBytes($"{DateTimeOffset.UtcNow.ToString(_options.LogDateFormat)} - ");

        lock (_lock)
        {
            RotateIfNeeded(payload.Length + datePrefix.Length + _newline.Length);
            _stream ??= OpenStream();
            _stream.Seek(0, SeekOrigin.End);

            // Use synchronous write inside lock
            _stream.Write(datePrefix, 0, datePrefix.Length);
            _stream.Write(payload, 0, payload.Length);
            _stream.Write(_newline, 0, _newline.Length);

            _currentFileSize += payload.Length + datePrefix.Length + _newline.Length;
            LinesWritten++;
        }

        await Task.CompletedTask; // preserve async signature
    }


    private void CloseStream()
    {
        _stream?.Dispose();
        _stream = null;
    }

    public void Dispose()
    {
        CloseStream();
        GC.SuppressFinalize(this);
    }

    public ValueTask DisposeAsync()
    {
        CloseStream();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
