using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IGlobalIDSelectionSet : ISelectionSet
    {
        void AddID(long ID);
        void AddIDs(List<long> IDs);

        void RemoveID(long ID);
        void RemoveIDs(List<long> IDs);

        List<long> IDs { get; }
    }
}