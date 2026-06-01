using gView.Framework.Core.Common;
using gView.Framework.Core.Data;
using gView.Framework.Core.FDB;
using gView.Framework.Core.Geometry;
using gView.Framework.Core.IO;
using gView.Framework.Data;
using gView.Framework.Geometry;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace gView.DataSources.Unknown;

[RegisterPlugIn("3A9BC6DA-2A23-4F80-8326-4FEBB83E7D6A")]
public class UnknownFeatureDataset : IFeatureDataset
{
    public string ConnectionString { get; set; } = "";

    public string DatasetGroupName => "Unknown";

    public string DatasetName => "Unknown Dataset Type";

    public string ProviderName => "";

    public DatasetState State => DatasetState.opened;

    public string Query_FieldPrefix => "";

    public string Query_FieldPostfix => "";

    public IDatabase Database => null!;

    public string LastErrorMessage { get => ""; set {} }

    public void Dispose()
    {

    }

    public Task<IDatasetElement> Element(string title)
    {
        var layer = new Layer() { Class = new UnknownFeatureClass(this, title) };

        return Task.FromResult<IDatasetElement>(layer);  
    }

    public Task<List<IDatasetElement>> Elements()
    {
        throw new NotImplementedException();
    }

    public Task<IEnvelope> Envelope()
    {
        return Task.FromResult<IEnvelope>(new Envelope());
    }

    public Task<IEnumerable<IMetadataProvider>> GetMetadataProviders()
    {
        return Task.FromResult<IEnumerable<IMetadataProvider>>([]);
    }

    public Task<ISpatialReference> GetSpatialReference()
    {
        return Task.FromResult<ISpatialReference>(null!);
    }

    public IMetadataProvider MetadataProvider(Guid guid)
    {
        return null!;
    }

    public Task<bool> Open()
    {
        return Task.FromResult(true);
    }

    public void ReadMetadata(IPersistStream stream)
    {
        
    }

    public Task RefreshClasses()
    {
        return Task.CompletedTask;
    }

    public Task<bool> LoadAsync(IPersistStream stream)
    {
        this.ConnectionString = (string)stream.Load("connectionstring", "");
        return Task.FromResult(true);
    }

    public void Save(IPersistStream stream)
    {
        stream.Save("connectionstring", this.ConnectionString);
    }

    public Task<bool> SetConnectionString(string connectionString)
    {
        this.ConnectionString = connectionString;
        return Task.FromResult(true);
    }

    public void SetSpatialReference(ISpatialReference sRef)
    {
        
    }

    public Task UpdateMetadataProviders()
    {
        return Task.CompletedTask;
    }

    public Task WriteMetadata(IPersistStream stream)
    {
        return Task.CompletedTask;
    }
}
