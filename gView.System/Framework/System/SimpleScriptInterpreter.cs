using System;
using System.Text;

namespace gView.Framework.system
{
    public class SimpleScriptInterpreter
    {
        public SimpleScriptInterpreter(string script)
        {
            this.Script = script;
        }

        public string Script { get; set; }

        public string Interpret()
        {
            var script = this.Script.Replace(Environment.NewLine, "\n");
            var result = this.Script;

            if (script.StartsWith("@@start\n"))
            {
                var expr_lines = script.Split('\n');

                StringBuilder sb = new StringBuilder();
                bool interpret = false, useLine = true;

                for (int i = 1, to = expr_lines.Length; i < to; i++)
                {
                    var expr_line = expr_lines[i];
                    if (expr_line == "@@end")
                    {
                        interpret = true;
                        result = sb.ToString();
                    }
                    else if(expr_line.StartsWith("@@if(")) 
                    {
                        var commandResult = GetCommand(expr_line);
                        useLine = CheckCondition(commandResult.arguments);
                    }
                    else if(expr_line == "@@endif")
                    {
                        useLine = true;
                    }
                    else if (interpret == false && useLine)
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
                return !String.IsNullOrWhiteSpace(args[0]);
            }
            else if (args.Length == 2)
            {
                return args[0] == args[1];
            }
            else if(args.Length == 3)
            {
                switch(args[1]?.ToLower())
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
