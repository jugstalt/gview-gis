using gView.Framework.Data;
using gView.Interoperability.GeoServices.Rest.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Interoperability.GeoServices.Dataset
{
    public class GeoServicesFeatureClass : FeatureClass, IWebFeatureClass
    {
        public GeoServicesFeatureClass(GeoServicesDataset dataset, JsonLayer jsonLayer)
        {
            this.ID = jsonLayer.Id.ToString();
            this.Name = jsonLayer.Name;

            var fields = this.Fields as Fields;
            if (fields != null && jsonLayer.Fields != null)
            {
                foreach(var jsonField in jsonLayer.Fields)
                {
                    fields.Add(new Field(jsonField.Name, jsonField.GetFieldType())
                    {
                        aliasname = jsonField.Alias ?? jsonField.Name
                    });
                }
            }

            this.GeometryType = jsonLayer.GetGeometryType();
        }

        #region IWebFeatureClass

        public string ID { get; private set; }

        #endregion
    }
}
