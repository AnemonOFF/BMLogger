namespace BMLogger;

public class BMLoggerProvider : IDisposable
{
    private readonly List<BMLogger> _loggers = new();
    private readonly DirectoryInfo _directoryInfo;
    private readonly bool _logInConsole;
    private readonly int _maxFileSizeInMb;

    public List<BMLogger> Loggers { get { return _loggers; } }

    public BMLogger this[string name] => _loggers.Find(x => x.Name == name) ?? throw new Exception($"{name} not exist.");

    public BMLoggerProvider(string dir = "bmlogger", TimeSpan? expiration = null, int maxFileSizeInMb = 20, bool logInConsole = true)
    {
        _logInConsole = logInConsole;
        _directoryInfo = GetDirectory(dir);
        _maxFileSizeInMb = maxFileSizeInMb;
        DeleteOldFiles(expiration ?? new TimeSpan(30, 0, 0, 0, 0), _directoryInfo, maxFileSizeInMb * 1000000);
    }

    ~BMLoggerProvider()
    {
        foreach(var logger in _loggers)
            logger.Dispose();
        _loggers.Clear();
    }

    private static void DeleteOldFiles(TimeSpan expiration, DirectoryInfo dir, long maxSize)
    {
        foreach(var file in dir.GetFiles())
        {
            if (file.Exists && file.Extension == ".log")
            {
                if (file.LastWriteTimeUtc.AddTicks(expiration.Ticks) < DateTime.UtcNow)
                {
                    file.Delete();
                }
                else if (file.Length > maxSize)
                {
                    var newSize = file.Length;
                    var strings = new List<string>();
                    using (var reader = new StreamReader(file.FullName))
                    {
                        string? line;
                        do
                        {
                            line = reader.ReadLine();
                            if (newSize > maxSize && line != null)
                            {
                                newSize -= line.Length;
                                continue;
                            }
                            if (!reader.EndOfStream)
                                strings.Add(line);
                        } while (line != null);
                    }
                    using var writer = new StreamWriter(file.FullName);
                    foreach(var line in strings)
                        writer.WriteLine(line);
                }
            }
        }
    }

    public BMLogger CreateLogger(string name, bool logDateTime = true, bool logCallerPath = true, bool logCallerMember = true)
    {
        var logger = new BMLogger(name, _directoryInfo.FullName, _maxFileSizeInMb, _logInConsole, logDateTime, logCallerPath, logCallerMember);
        _loggers.Add(logger);
        return logger;
    }

    public BMLogger GetLogger(string name) => this[name];

    public void RemoveLogger(string name)
    {
        var logger = _loggers.Find(x => x.Name == name);
        if (logger == null)
            throw new Exception($"{name} not exist.");
        logger.Dispose();
        _loggers.Remove(logger);
    }

    private static DirectoryInfo GetDirectory(string dir)
    {
        var dirInfo = new DirectoryInfo(dir);
        if(!dirInfo.Exists)
            dirInfo.Create();
        return dirInfo;
    }

    public void Dispose()
    {
        foreach (var logger in _loggers)
            logger.Dispose();
        _loggers.Clear();
        GC.SuppressFinalize(this);
    }
}
