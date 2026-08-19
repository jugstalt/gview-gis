using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.Common
{
    public class SimpleScriptInterpreter
    {
        public SimpleScriptInterpreter(string script)
        {
            Script = script;
        }

        public string Script { get; set; }

        public string Interpret()
        {
            var script = Script.Replace(Environment.NewLine, "\n");
            var result = Script;

            if (script.StartsWith("@@start\n"))
            {
                var expr_lines = script.Split('\n');

                StringBuilder sb = new StringBuilder();
                bool interpret = false;
                // Stack of active "@@if(...)" conditions - a line is only emitted while every
                // enclosing condition is true, so "@@if(...)" blocks can be nested to express an
                // AND of several checks (e.g. "field A present" nested inside "field B present").
                // An empty stack means "no active condition" => line is shown (matches the
                // original, non-nested behaviour where a single @@if/@@endif pair was allowed).
                var conditionStack = new Stack<bool>();

                for (int i = 1, to = expr_lines.Length; i < to; i++)
                {
                    var expr_line = expr_lines[i];
                    if (expr_line == "@@end")
                    {
                        interpret = true;
                        result = sb.ToString();
                    }
                    else if (expr_line.StartsWith("@@if("))
                    {
                        var commandResult = GetCommand(expr_line);
                        conditionStack.Push(CheckCondition(commandResult.arguments));
                    }
                    else if (expr_line == "@@endif")
                    {
                        if (conditionStack.Count > 0)
                        {
                            conditionStack.Pop();
                        }
                    }
                    else if (interpret == false && conditionStack.All(c => c))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(Environment.NewLine);
                        }

                        sb.Append(expr_line);
                    }

                    if (interpret == true)
                    {
                        var commandResult = GetCommand(expr_line);
                        if (commandResult.command != null)
                        {
                            switch (commandResult.command)
                            {
                                case "replace":
                                    if (commandResult.arguments.Length == 2)
                                    {
                                        result = result.Replace(commandResult.arguments[0], commandResult.arguments[1]);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private (string command, string[] arguments) GetCommand(string line)
        {
            line = line.Trim();

            if (line.StartsWith("@@") && line.EndsWith(")"))
            {
                var index = line.IndexOf("(");

                string command = line.Substring(2, index - 2);
                var args = line.Substring(index + 1, line.Length - index - 2).Split(',');

                return (command.ToLower(), args);
            }

            return (null, null);
        }

        private bool CheckCondition(string[] args)
        {
            if (args.Length == 1)
            {
                return !string.IsNullOrWhiteSpace(args[0]);
            }
            else if (args.Length == 2)
            {
                return args[0] == args[1];
            }
            else if (args.Length == 3)
            {
                switch (args[1]?.ToLower())
                {
                    case "eq":
                        return args[0] == args[2];
                    case "not":
                        return args[0] != args[2];
                }
            }

            return false;
        }

        public static bool IsSimpleScript(string script)
        {
            return script.StartsWith("@@start");
        }
    }
}
