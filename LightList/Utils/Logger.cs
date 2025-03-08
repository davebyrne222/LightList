using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace LightList.Utils;

public static class Logger
{
    public static void Log(
        string message,
        [CallerMemberName] string methodName = "",
        [CallerFilePath] string filePath = ""
    )
    {
        var className = Path.GetFileNameWithoutExtension(filePath);
        Console.WriteLine($"[{className}.{methodName}] {message}");
    }
}