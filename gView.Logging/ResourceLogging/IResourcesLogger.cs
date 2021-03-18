using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.Logging.ResourceLogging
{
    public interface IResourcesLogger
    {
        Task Init(string initialParameter);

        Task Log(IResourcesLoggerItem item);

        Task Flush();
    }
}
