using gView.Framework.Core.Data;
using Proj4Net.Core.Units;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace gView.Blazor.Core.Extensions;
static public class SelectionSetExtensions
{
    static public HashSet<IRow> SelectedRows(this ISelectionSet? selectionSet, IEnumerable<IRow> rows)
        => selectionSet switch
        {
            IIDSelectionSet idSelection => new HashSet<IRow>(rows.Where(r => idSelection.IDs.Contains(r.OID))),
            _ => new HashSet<IRow>()
        };

    static public bool TryFromSelectedRows(this ISelectionSet? selectionSet, IEnumerable<IRow> rows)
    {
        if(selectionSet is IIDSelectionSet idSelection)
        {
            idSelection.Clear();
            idSelection.AddIDs(rows.Select(r => r.OID).ToList());

            return true;
        }

        return false;
    }

    static public bool CanHandleRowsSelection(this ISelectionSet? selectionSet)
        => selectionSet switch
        {
            IIDSelectionSet => true,
            _ => false
        };

    static public bool IsEqual(this ISelectionSet? selectionSet, ISelectionSet? canditate)
    {
        if(selectionSet is null)
        {
            return canditate is null;
        }

        if (canditate is null)
        {
            return false;
        }

        if(selectionSet is IIDSelectionSet idSelectionSet 
            && canditate is IIDSelectionSet idCandiate)
        {
            if (idSelectionSet.Count != idCandiate.Count)
            {
                return false;
            }

            foreach(int id in idSelectionSet.IDs)
            {
                if(idCandiate.IDs.Contains(id) == false)
                {
                    return false;
                } 
            }

            return true;
        }

        return false;
    }

    static public bool IsNotEqual(this ISelectionSet? selectionSet, ISelectionSet? canditate)
        => selectionSet.IsEqual(canditate) == false;
}
