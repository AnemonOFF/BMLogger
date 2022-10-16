using System.Runtime.CompilerServices;
using System.Text;

namespace BMLogger;

public class BMLogger : IDisposable
{
    private readonly string _dir;
    private readonly string _name;
    private readonly int _maxFileSizeInMb;
    private readonly bool _logDateTime;
    private readonly bool _logCallerPath;
    private readonly bool _logCallerMember;
    private readonly ConsoleLogs _logInConsole;
    private readonly StreamWriter _stream;
    private readonly object _lock = new();

    public string Name { get { return _name; } }

    public BMLogger(
        string name,
        string dir,
        int maxFileSizeInMb = 5,
        ConsoleLogs logInConsole = ConsoleLogs.LogAll,
        bool logDateTime = true,
        bool logCallerPath = true,
        bool logCallerMember = true
        )
    {
        _name = name;
        _dir = dir;
        _maxFileSizeInMb = maxFileSizeInMb;
        _logDateTime = logDateTime;
        _logCallerPath = logCallerPath;
        _logCallerMember = logCallerMember;
        _logInConsole = logInConsole;
        _stream = GetStream(name, dir);
    }

    ~BMLogger()
    {
        Dispose();
    }

    /// <summary>
    /// Log your message with logLevel
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="logLevel">log level of your message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    public void Log(string message, LogLevel logLevel = LogLevel.INFO, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
    {
        var messageBuilder = new StringBuilder();
        if (_logDateTime)
            messageBuilder.Append($"[{DateTime.UtcNow:g}]");
        messageBuilder.Append($"[{logLevel}]");
        if (_logCallerPath || _logCallerMember)
        {
            messageBuilder.Append('[');
            if (_logCallerPath)
                messageBuilder.Append(callerPath + (_logCallerMember ? " " : ""));
            if (_logCallerMember)
                messageBuilder.Append($"{callerMember}:{callerLine}");
            messageBuilder.Append(']');
        }
        messageBuilder.Append(" " + message);
        var resultMessage = messageBuilder.ToString();
        lock (_lock)
        {
            var consoleColors = GetConsoleColors(logLevel);
            switch (_logInConsole)
            {
                case ConsoleLogs.LogAll:
                    WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogWarnsAndHigher:
                    if(logLevel >= LogLevel.WARN)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogErrorsAndHigher:
                    if (logLevel >= LogLevel.ERROR)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogFatals:
                    if (logLevel == LogLevel.FATAL)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogSuccess:
                    if (logLevel == LogLevel.SUCCESS)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogSuccessAndInfos:
                    if (logLevel == LogLevel.SUCCESS || logLevel == LogLevel.INFO)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogSuccessWarnsAndHigher:
                    if (logLevel == LogLevel.SUCCESS || logLevel >= LogLevel.WARN)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogSuccessErrorsAndHigher:
                    if (logLevel == LogLevel.SUCCESS || logLevel >= LogLevel.ERROR)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogSuccessFatals:
                    if (logLevel == LogLevel.SUCCESS || logLevel == LogLevel.FATAL)
                        WriteConsoleColoredLine(resultMessage, consoleColors.foreground, consoleColors.background);
                    break;
                case ConsoleLogs.LogNothing:
                    break;
            }
            _stream.WriteLine(resultMessage);
        }
    }

    private static (ConsoleColor foreground, ConsoleColor background) GetConsoleColors(LogLevel logLevel)
    {
        var fg = logLevel == LogLevel.SUCCESS ? ConsoleColor.Green :
            logLevel == LogLevel.WARN ? ConsoleColor.DarkYellow :
            logLevel == LogLevel.ERROR ? ConsoleColor.DarkRed :
            BMLoggerProvider.ConsoleDefaultForeground;

        var bg = logLevel == LogLevel.FATAL ? ConsoleColor.Red : BMLoggerProvider.ConsoleDefaultBackground;

        return (fg, bg);
    }

    private static void WriteConsoleColoredLine(string message, ConsoleColor foreground, ConsoleColor background)
    {
        Console.BackgroundColor = background;
        Console.ForegroundColor = foreground;
        Console.WriteLine(message);
        Console.BackgroundColor = BMLoggerProvider.ConsoleDefaultBackground;
        Console.ForegroundColor = BMLoggerProvider.ConsoleDefaultForeground;
    }

    /// <summary>
    /// Async implementation of Log method. Log your message with logLevel
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="logLevel">log level of your message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    public async Task LogAsync(string message, LogLevel logLevel = LogLevel.INFO, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await Task.Run(() => Log(message, logLevel, callerPath, callerMember, callerLine));

    /// <summary>
    /// Shortcut for Log with INFO log level. Log your message with INFO log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    public void Info(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.INFO, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for Log with SUCCESS log level. Log your message with SUCCESS log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    public void Success(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.SUCCESS, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for Log with WARN log level. Log your message with WARN log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    public void Warn(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.WARN, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for Log with ERROR log level. Log your message with ERROR log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    public void Error(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.ERROR, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for Log with FATAL log level. Log your message with FATAL log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    public void Fatal(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.FATAL, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for LogAsync with INFO log level. Log your message with INFO log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <returns></returns>
    public async Task InfoAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.INFO, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for LogAsync with WARN log level. Log your message with WARN log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <returns></returns>
    public async Task WarnAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.WARN, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for LogAsync with ERROR log level. Log your message with ERROR log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <returns></returns>
    public async Task ErrorAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.ERROR, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for LogAsync with FATAL log level. Log your message with FATAL log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <returns></returns>
    public async Task FatalAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.FATAL, callerPath, callerMember, callerLine);

    /// <summary>
    /// Shortcut for LogAsync with SUCCESS log level. Log your message with SUCCESS log level.
    /// </summary>
    /// <param name="message">string message</param>
    /// <param name="callerPath">path from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerMember">method from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <param name="callerLine">line from where this method was called. Set this arg only if you are making your own log function.</param>
    /// <returns></returns>
    public async Task SuccessAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.SUCCESS, callerPath, callerMember, callerLine);

    private static StreamWriter GetStream(string name, string dir)
    {
        var path = Path.Combine(dir, name + ".log");
        var stream = new StreamWriter(path, true)
        {
            AutoFlush = true
        };
        return stream;
    }

    public void Dispose()
    {
        if(_stream != null)
            _stream.Close();
        var fileInfo = new FileInfo(Path.Combine(_dir, _name + ".log"));
        var maxSize = _maxFileSizeInMb * 1000000;
        if (fileInfo.Exists && fileInfo.Length > maxSize)
        {
            var newSize = fileInfo.Length;
            using var writer = new StreamWriter(fileInfo.FullName);
            using var reader = new StreamReader(fileInfo.FullName);
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
                    writer.WriteLine(line);
            } while (line != null);
        }
        GC.SuppressFinalize(this);
    }
}
