using System.Collections.Generic;

/// <summary>
/// The <c>gView.Framework</c> provides all interfaces to develope
/// with and for gView
/// </summary>
namespace gView.Framework.Core.Common
{
    public class ListOperations<T>
    {
        static public List<T> Clone(List<T> l)
        {
            if (l == null)
            {
                return null;
            }

            List<T> c = new List<T>();

            foreach (T e in l)
            {
                c.Add(e);
            }

            return c;
        }

        static public List<T> Swap(List<T> l)
        {
            List<T> s = new List<T>();
            for (int i = l.Count - 1; i >= 0; i--)
            {
                s.Add(l[i]);
            }

            return s;
        }

        static public List<T> Sort(List<T> l, IComparer<T> c)
        {
            // Die Routine wird zum Beispiel zum Sortieren von
            // LabelLayers nach der Priorität verwendet.
            //
            // Besonders bei dieser Sortierung ist, dass die ursprüngliche
            // Reihenfolge unverändert bleicht, wenn der Comparer 0 liefert.
            // Beim normal List.Sort ist das nicht unbedingt der Fall...
            //
            if (l == null || c == null)
            {
                return l;
            }

            try
            {
                List<T> ret = new List<T>();
                foreach (T o in l)
                {
                    int pos = 0;
                    foreach (T r in ret)
                    {
                        if (c.Compare(r, o) < 0)
                        {
                            break;
                        }
                        pos++;
                    }
                    ret.Insert(pos, o);
                }

                return ret;
            }
            catch
            {
                return l;
            }
        }
    }
}
