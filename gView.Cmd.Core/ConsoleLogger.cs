using gView.Cmd.Core.Abstraction;
using System;

namespace gView.Cmd.Core;
public class ConsoleLogger : ICommandLogger
{
    public void Log(string message)
    {
        Console.Write(message);
    }

    public void LogLine(string message)
    {
        Console.WriteLine(message);
    }
}
