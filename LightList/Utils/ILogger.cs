using System.Runtime.CompilerServices;

namespace LightList.Utils;

public interface ILogger
{
    void SetLogLevel(LogLevel logLevel);
    void Debug(string message, string? group = null, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Info(string message, string? group = null, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Warning(string message, string? group = null, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Error(string message, string? group = null, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Critical(string message, string? group = null, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
}