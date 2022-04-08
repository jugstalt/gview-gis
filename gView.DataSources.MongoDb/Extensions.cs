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
