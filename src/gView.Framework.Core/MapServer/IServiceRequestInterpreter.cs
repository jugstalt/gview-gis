using System.Threading.Tasks;

namespace gView.Framework.Core.MapServer
{
    public interface IServiceRequestInterpreter
    {
        void OnCreate(IMapServer mapServer);
        Task Request(IServiceRequestContext context);
        AccessTypes RequiredAccessTypes(IServiceRequestContext context);

        string IdentityName { get; }
        string IdentityLongName { get; }

        InterpreterCapabilities Capabilities { get; }

        int Priority { get; }
    }
}
