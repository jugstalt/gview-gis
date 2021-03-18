using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode.Configuration
{
    public interface IConfigParser
    {
        string Parse(string configValue);
    }
}
