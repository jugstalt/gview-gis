using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace gView.Framework.Db.Extensions
{
    static public class CommandExtensions
    {
        static public void SetCustomCursorTimeout(this DbCommand command)
        {
            if (command != null && Globals.CustomCursorTimeoutSeconds >= 0)
            {
                command.CommandTimeout = Globals.CustomCursorTimeoutSeconds;
            }
        }
    }
}
