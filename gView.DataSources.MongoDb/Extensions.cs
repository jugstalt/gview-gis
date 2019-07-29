using System;
using System.Collections.Generic;
using System.Text;

namespace gView.DataSources.MongoDb
{
    static public class Extensions
    {
        static public string ToFeatureClassCollectionName(this string name)
        {
            return MongoDbDataset.FeatureCollectionNamePrefix + name.ToLower();
        }
    }
}
