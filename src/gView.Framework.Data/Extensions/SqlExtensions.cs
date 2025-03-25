using Microsoft.SqlServer.Management.SqlParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Framework.Data.Extensions
{
    static public class SqlExtensions
    {
        #region Sql Injection

        static public string CheckWhereClauseForSqlInjection(this string where)
        {
            string originalWhere = where;

            if (!string.IsNullOrWhiteSpace(where))
            {
                if (WhereClauseWhiteList.Contains(where))
                {
                    return where;
                }

                where = where
                    //.Replace(" in ", "=")
                    .Replace("<>", "=")
                    .Replace("<=", "=")
                    .Replace(">=", "=")
                    .Replace("<", "=")
                    .Replace(">", "=");

                string sql = "select * from tab where " + where;

                var parseResult = Parser.Parse(sql);

                if (parseResult.Errors != null && parseResult.Errors.Count() > 0)
                {
                    StringBuilder errors = new StringBuilder();

                    foreach (var error in parseResult.Errors)
                    {
                        errors.Append(error.Message + Environment.NewLine);
                    }

                    throw new Exception($"SQL Parser Exception ({where}): {errors.ToString()}");
                }

                var tokens = parseResult.Script.Tokens
                    .Where(t => t.Type.ToUpper() != "LEX_WHITE")
                    .Where(t => !"()[]".Contains(t.Type))
                    .ToArray();

                if (tokens.Where(t => t.Type.ToUpper() == ";").Count() > 0)
                {
                    throw new SqlDangerousStatementExceptions("multiple statements detected");
                }

                if (tokens.Where(t => t.Type.ToUpper() == "LEX_END_OF_LINE_COMMENT").Count() > 0)
                {
                    throw new SqlDangerousStatementExceptions("End of line comment detected");
                }

                for (int i = 1, to = tokens.Length; i < to - 1; i++)
                {
                    if (tokens[i].Type == "=" || tokens[i].Type.Equals("TOKEN_IN", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var tokenField = tokens[i - 1];
                        var tokenValue = tokens[i + 1];

                        if (tokenField.IsConstant() && tokenField.IsConstant())
                        {
                            throw new SqlDangerousStatementExceptions("Suspicious statement: " + tokenField.Text + tokens[i].Text + tokenValue.Text);
                        }
                        else if (tokenField.IsField() && tokenValue.IsField()/* && tokenField.Text.ToLower()==tokenValue.Text.ToLower()*/)
                        {
                            throw new SqlDangerousStatementExceptions("Suspicious statement: " + tokenField.Text + tokens[i].Text + tokenValue.Text);
                        }
                        else if (tokenField.Text.ToLower() == tokenValue.Text.ToLower())
                        {
                            throw new SqlDangerousStatementExceptions("Suspicious statement: " + tokenField.Text + tokens[i].Text + tokenValue.Text);
                        }
                    }
                }
            }

            return originalWhere;
        }

        static private bool IsConstant(this Token token)
        {
            switch (token.Type.ToUpper())
            {
                case "TOKEN_INTEGER":
                case "TOKEN_STRING":
                    return true;
            }

            return false;
        }

        static private bool IsField(this Token token)
        {
            switch (token.Type.ToUpper())
            {
                case "TOKEN_ID":
                    return true;
            }

            return false;
        }

        static IEnumerable<string> WhereClauseWhiteList = new string[]
        {
            "1=1" // used by esri tools, e.g. esri-leaflet!!
        };

        #endregion
    }

    public class SqlDangerousStatementExceptions : Exception
    {
        public SqlDangerousStatementExceptions(string message)
            : base(message)
        {

        }
    }
}
