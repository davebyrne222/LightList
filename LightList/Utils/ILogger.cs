using System.Runtime.CompilerServices;

namespace LightList.Utils;

public interface ILogger
{
    void SetLogLevel(LogLevel logLevel);
    void Debug(string message, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Info(string message, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Warning(string message, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Error(string message, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
    void Critical(string message, [CallerMemberName] string methodName="", [CallerFilePath] string fileName="");
}