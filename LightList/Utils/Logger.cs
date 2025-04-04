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


public static class Logger
{
    public static void Log(
        string message,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = ""
    )
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Console.WriteLine($"[{(className + "." + methodName), -50}] - {message}");
    }
}