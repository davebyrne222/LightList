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
    private LogLevel _currentLogLevel;

    public Logger(LogLevel logLevel = LogLevel.Info)
    {
        _currentLogLevel = logLevel;
    }

    public void SetLogLevel(LogLevel logLevel)
    {
        _currentLogLevel = logLevel;
    }

    public void Debug(
        string message,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "")
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Log(LogLevel.Debug, $"{className}.{methodName}", message);
    }

    public void Info(string message,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "")
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Log(LogLevel.Info, $"{className}.{methodName}", message);
    }

    public void Warning(string message,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "")
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Log(LogLevel.Warning, $"{className}.{methodName}", message);
    }

    public void Error(string message,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "")
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Log(LogLevel.Error, $"{className}.{methodName}", message);
    }

    public void Critical(string message,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = "")
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Log(LogLevel.Critical, $"{className}.{methodName}", message);
    }

    private void Log(
        LogLevel logLevel,
        string caller,
        string message
    )
    {
        if (logLevel < _currentLogLevel) return;
        
        var prefix = GetPrefixForLogLevel(logLevel);
        Console.WriteLine($"[{prefix,-7}] [{caller,-50}] - {message}");
    }

    private string GetPrefixForLogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => "DEBUG",
            LogLevel.Info => "INFO",
            LogLevel.Warning => "WARNING",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRITICAL",
            _ => "NA"
        };
    }
}