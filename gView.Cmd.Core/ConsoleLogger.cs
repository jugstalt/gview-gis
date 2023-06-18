using gView.Cmd.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Cmd.Core;
internal class ConsoleLogger : ICommandLogger
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
