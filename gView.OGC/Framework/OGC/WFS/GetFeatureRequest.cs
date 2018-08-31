using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Data;
using System.IO;
using gView.Framework.Geometry;

namespace gView.Framework.OGC.WFS
{
    public class GetFeatureRequest
    {
        static public string Create(IFeatureClass fc, string typeName, IQueryFilter filter, string srsName, Filter_Capabilities filterCapabilites, GmlVersion version)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms, Encoding.UTF8);

            sw.WriteLine(
@"<?xml version=""1.0"" ?>
<wfs:GetFeature
version=""1.0.0""
service=""WFS""
maxfeatures=""5000""
handle=""gView Query""
xmlns=""http://www.opengis.net/wfs""
xmlns:wfs=""http://www.opengis.net/wfs""
xmlns:ogc=""http://www.opengis.net/ogc""
xmlns:gml=""http://www.opengis.net/gml""
xmlns:gv=""http://www.gview.com/server""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
xsi:schemaLocation=""http://www.opengis.net/wfs ../wfs/1.1.0/WFS.xsd"">
<wfs:Query typeName=""" + typeName + @"""" + ((srsName != String.Empty) ? " srsName=\"" + srsName + "\"" : "") + " >");
            if (filter.SubFields != "*" && filter.SubFields != "#ALL#")
            {
                foreach (string field in filter.SubFields.Split(' '))
                {
                    sw.Write("<wfs:PropertyName>" + field + "</wfs:PropertyName>");
                }
            }
            else
            {
                foreach (IField field in fc.Fields.ToEnumerable())
                {
                    sw.Write("<wfs:PropertyName>" + field.name + "</wfs:PropertyName>");
                }
            }
            sw.WriteLine(Filter.ToWFS(fc, filter, filterCapabilites, version));

            sw.WriteLine(
@"</wfs:Query>
</wfs:GetFeature>");

            sw.Flush();

            ms.Position = 0;
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, (int)ms.Length);
            sw.Close();

            string ret = Encoding.UTF8.GetString(bytes).Trim();

            return ret;
        }
    }
}
