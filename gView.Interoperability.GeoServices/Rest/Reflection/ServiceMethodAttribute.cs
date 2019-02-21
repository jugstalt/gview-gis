using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Reflection
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ServiceMethodAttribute : Attribute
    {
        public ServiceMethodAttribute(string name, string method)
        {
            this.Name = name;
            this.Method = method;
        }

        public string Name { get; set; }
        public string Method { get; set; }
    }
}
