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
                bool interpret = false;

                for (int i = 1, to = expr_lines.Length; i < to; i++)
                {
                    var expr_line = expr_lines[i];
                    if (expr_line == "@@end")
                    {
                        interpret = true;
                        result = sb.ToString();
                    }
                    else if (interpret == false)
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

        public static bool IsSimpleScript(string script)
        {
            return script.StartsWith("@@start");
        }
    }
}
