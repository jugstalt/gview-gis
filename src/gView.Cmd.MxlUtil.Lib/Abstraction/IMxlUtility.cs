using gView.Cmd.Core.Abstraction;

namespace gView.Cmd.MxlUtil.Lib.Abstraction
{
    public interface IMxlUtility
    {
        string Name { get; }

        Task<bool> Run(string[] args, ICommandLogger? logger = null);

        string Description();
        string HelpText();
    }
}
