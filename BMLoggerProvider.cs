namespace BMLogger;

public class BMLoggerProvider : IDisposable
{
    private readonly List<BMLogger> _loggers = new();
    private readonly DirectoryInfo _directoryInfo;
    private readonly bool _logInConsole;

    public List<BMLogger> Loggers { get { return _loggers; } }

    public BMLogger this[string name] => _loggers.Find(x => x.Name == name) ?? throw new Exception($"{name} not exist.");

    public BMLoggerProvider(string dir = "bmlogger", bool logInConsole = true)
    {
        _logInConsole = logInConsole;
        _directoryInfo = GetDirectory(dir);
    }

    ~BMLoggerProvider()
    {
        foreach(var logger in _loggers)
            logger.Dispose();
        _loggers.Clear();
    }

    public BMLogger CreateLogger(string name, bool logDateTime = true, bool logCallerPath = true, bool logCallerMember = true)
    {
        var logger = new BMLogger(name, _directoryInfo.FullName, _logInConsole, logDateTime, logCallerPath, logCallerMember);
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
