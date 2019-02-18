using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    public class NotAuthorizedException : Exception 
    {

    }

    public class MapServerException : Exception
    {
        public MapServerException(string messsage)
            :base(messsage)
        {

        }
    }
}
