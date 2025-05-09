﻿using System.Threading.Tasks;

namespace gView.Framework.Core.IO
{
    public interface IMetadataProvider : IPersistable
    {
        Task<bool> ApplyTo(object Object, bool setDefaults = false);
        string Name { get; }
    }
}
