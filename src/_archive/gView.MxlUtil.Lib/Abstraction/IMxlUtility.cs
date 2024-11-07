using System.Threading.Tasks;

namespace gView.MxlUtil.Lib.Abstraction
{
    public interface IMxlUtility
    {
        string Name { get; }

        Task Run(string[] args);

        string Description();
        string HelpText();
    }
}
