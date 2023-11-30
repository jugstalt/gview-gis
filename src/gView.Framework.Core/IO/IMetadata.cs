using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gView.Framework.Core.IO
{
    public interface IMetadata
    {
        void ReadMetadata(IPersistStream stream);
        Task WriteMetadata(IPersistStream stream);

        IMetadataProvider MetadataProvider(Guid guid);
        Task<IEnumerable<IMetadataProvider>> GetMetadataProviders();
        Task UpdateMetadataProviders();
    }
}
