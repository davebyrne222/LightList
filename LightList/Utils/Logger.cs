using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LightList.Utils;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public class Logger : ILogger
{
    private readonly LoggerContext _context;
    private LogLevel _currentLogLevel;

    public Logger(LoggerContext context, LogLevel logLevel = LogLevel.Debug)
    {
        _context = context;
        _currentLogLevel = logLevel;
    }

    public void SetLogLevel(LogLevel logLevel)
    {
        _currentLogLevel = logLevel;
    }

    public void Debug(string message, string? group = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "") =>
        LogInternal(LogLevel.Debug, message, group, methodName, filePath);

    public void Info(string message, string? group = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "") =>
        LogInternal(LogLevel.Info, message, group, methodName, filePath);

    public void Warning(string message, string? group = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "") =>
        LogInternal(LogLevel.Warning, message, group, methodName, filePath);

    public void Error(string message, string? group = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "") =>
        LogInternal(LogLevel.Error, message, group, methodName, filePath);

    public void Critical(string message, string? group = null,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "") =>
        LogInternal(LogLevel.Critical, message, group, methodName, filePath);

    private void LogInternal(LogLevel logLevel, string message, string? group, string methodName, string filePath)
    {
        if (logLevel < _currentLogLevel) return;

        string groupName = group ?? _context.Group;
        string levelName = Enum.GetName(typeof(LogLevel), logLevel) ?? "NA";
        string className = Path.GetFileNameWithoutExtension(filePath);

        LogToConsole(levelName, message, groupName, className, methodName);
    }

    // Log to console or if not available, whatever the standard output is
    private void LogToConsole(
        string logLevel,
        string message,
        string group,
        string? className = null,
        string? methodName = null
    )
    {
        Console.WriteLine($"[{logLevel,-7}] [{group,-15}] [{className,-25}] [{methodName,-25}] - {message}");
    }
}