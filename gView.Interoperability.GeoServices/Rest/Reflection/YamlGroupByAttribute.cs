using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Reflection
{
    public class YamlGroupByAttribute : Attribute
    {
        public YamlGroupByAttribute(string groupByField)
        {
            this.GroupByField = groupByField;
        }

        public string GroupByField { get; }
    }
}
