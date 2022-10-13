using System.Runtime.CompilerServices;
using System.Text;

namespace BMLogger;

public enum LogLevel
{
    INFO,
    SUCCESS,
    WARN,
    ERROR,
    FATAL
}

public class BMLogger : IDisposable
{
    private readonly bool _logDateTime;
    private readonly bool _logCallerPath;
    private readonly bool _logCallerMember;
    private readonly bool _logInConsole;
    private readonly StreamWriter _stream;
    private object _lock = new object();

    public string Name { get; }

    public BMLogger(
        string name,
        string dir,
        bool logInConsole = true,
        bool logDateTime = true,
        bool logCallerPath = true,
        bool logCallerMember = true
        )
    {
        Name = name;
        _logDateTime = logDateTime;
        _logCallerPath = logCallerPath;
        _logCallerMember = logCallerMember;
        _logInConsole = logInConsole;
        _stream = GetStream(name, dir);
    }

    ~BMLogger()
    {
        if (_stream != null)
            _stream.Close();
    }

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
            if (_logInConsole)
            {
                WriteConsoleColoredLine(
                    resultMessage,
                    logLevel == LogLevel.SUCCESS ? ConsoleColor.Green : logLevel == LogLevel.WARN ? ConsoleColor.DarkYellow : logLevel == LogLevel.ERROR ? ConsoleColor.DarkRed : null,
                    logLevel == LogLevel.FATAL ? ConsoleColor.Red : null
                    );
            }
            _stream.WriteLine(resultMessage);
            _stream.Flush();
        }
    }

    private static void WriteConsoleColoredLine(string message, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        if(background != null)
            Console.BackgroundColor = background.Value;
        if(foreground != null)
            Console.ForegroundColor = foreground.Value;
        Console.WriteLine(message);
        if(background != null || foreground != null)
            Console.ResetColor();
    }

    public async Task LogAsync(string message, LogLevel logLevel = LogLevel.INFO, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await Task.Run(() => Log(message, logLevel, callerPath, callerMember, callerLine));

    public void Info(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.INFO, callerPath, callerMember, callerLine);

    public void Success(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.SUCCESS, callerPath, callerMember, callerLine);

    public void Warn(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.WARN, callerPath, callerMember, callerLine);

    public void Error(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.ERROR, callerPath, callerMember, callerLine);

    public void Fatal(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => Log(message, LogLevel.FATAL, callerPath, callerMember, callerLine);

    public async Task InfoAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.INFO, callerPath, callerMember, callerLine);

    public async Task WarnAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.WARN, callerPath, callerMember, callerLine);

    public async Task ErrorAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.ERROR, callerPath, callerMember, callerLine);

    public async Task FatalAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.FATAL, callerPath, callerMember, callerLine);

    public async Task SuccessAsync(string message, [CallerFilePath] string callerPath = "", [CallerMemberName] string callerMember = "", [CallerLineNumber] int callerLine = 0)
        => await LogAsync(message, LogLevel.SUCCESS, callerPath, callerMember, callerLine);

    private static StreamWriter GetStream(string name, string dir)
    {
        var path = Path.Combine(dir, name + ".log");
        var stream = new StreamWriter(path, true);
        return stream;
    }

    public void Dispose()
    {
        if(_stream != null)
            _stream.Close();
        GC.SuppressFinalize(this);
    }
}
