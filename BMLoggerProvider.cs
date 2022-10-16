namespace BMLogger;

public class BMLoggerProvider : IDisposable
{
    public static ConsoleColor ConsoleDefaultForeground { get; set; } = ConsoleColor.White;
    public static ConsoleColor ConsoleDefaultBackground { get; set; } = ConsoleColor.Black;

    private readonly List<BMLogger> _loggers = new();
    private readonly DirectoryInfo _directoryInfo;
    private readonly ConsoleLogs _logInConsole;
    private readonly int _maxFileSizeInMb;

    public List<BMLogger> Loggers { get { return _loggers; } }

    public BMLogger this[string name] => _loggers.Find(x => x.Name == name) ?? throw new Exception($"{name} not exist.");

    /// <summary>
    /// BMLogger provider (fabric)
    /// </summary>
    /// <param name="dir">path to directory of logs </param>
    /// <param name="expiration">time from last log in file. If greater - delete</param>
    /// <param name="maxFileSizeInMb">max file size on provider init. If greater - cut</param>
    /// <param name="logInConsole">which messages should be logged in console</param>
    public BMLoggerProvider(string dir = "bmlogger", TimeSpan? expiration = null, int maxFileSizeInMb = 20, ConsoleLogs logInConsole = ConsoleLogs.LogAll)
    {
        _logInConsole = logInConsole;
        _directoryInfo = GetDirectory(dir);
        _maxFileSizeInMb = maxFileSizeInMb;
        DeleteOldFiles(expiration ?? new TimeSpan(30, 0, 0, 0, 0), _directoryInfo, maxFileSizeInMb * 1000000);
        _loggers.Add(CreateLogger("default"));
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

    /// <summary>
    /// Creating new BMLogger with name
    /// </summary>
    /// <param name="name">Logger and file name</param>
    /// <param name="logDateTime">flag to log date and time of message  </param>
    /// <param name="logCallerPath">flag to log path to file from where log function was called</param>
    /// <param name="logCallerMember">flag to log method and line from where log function was called</param>
    /// <param name="logInConsole">which messages should be logged in console</param>
    /// <returns>logger</returns>
    public BMLogger CreateLogger(string name, bool logDateTime = true, bool logCallerPath = true, bool logCallerMember = true, ConsoleLogs? logInConsole = null)
    {
        var logger = new BMLogger(name, _directoryInfo.FullName, _maxFileSizeInMb, logInConsole ?? _logInConsole, logDateTime, logCallerPath, logCallerMember);
        _loggers.Add(logger);
        return logger;
    }

    /// <summary>
    /// </summary>
    /// <param name="name">logger name</param>
    /// <returns>Logger which created with CreateLogger</returns>
    public BMLogger GetLogger(string name) => this[name];

    /// <summary>
    /// Removing logger from list, NOT FROM COMPUTER FOLDER
    /// </summary>
    /// <param name="name">logger name</param>
    /// <exception cref="Exception">throw if logger not exist in list</exception>
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
