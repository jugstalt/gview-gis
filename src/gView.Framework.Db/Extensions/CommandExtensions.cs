using System.Data.Common;

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
