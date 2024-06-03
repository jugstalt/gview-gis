using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public interface IIDSelectionSet : ISelectionSet
    {
        void AddID(int ID);
        void AddIDs(List<int> IDs);

        void RemoveID(int ID);
        void RemoveIDs(List<int> IDs);

        bool Contains(int id);

        List<int> IDs { get; }
    }
}