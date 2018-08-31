using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.AppCode
{
    static public class Cache
    {
        public void Init(string folder)
        {
            foreach(var mapFileInfo in new DirectoryInfo(folder).GetFiles("*.mxl"))
            {

            }
        }
    }
}
