using System;
using System.Collections.Generic;
using System.Text;

namespace gView.DataSources.Fdb
{
    /// <summary>
    /// Builds a single SQL predicate covering the complete set of spatial-index node ids (NIDs)
    /// that a spatial query resolves to.
    /// <para>
    /// The FDB feature cursors used to run one SQL statement per NID batch and concatenate the
    /// readers, which loses global <c>ORDER BY</c> ordering (and therefore <c>ORDER BY</c> +
    /// <c>LIMIT</c>/<c>OFFSET</c> paging). Emitting one combined <c>WHERE</c> lets the database
    /// evaluate <c>ORDER BY</c> / <c>LIMIT</c> / <c>OFFSET</c> once, globally.
    /// </para>
    /// </summary>
    /// <remarks>
    /// NID list encoding (see <c>BinarySearchTree2.CollectNIDs</c> / <c>CollectNIDsPlus</c>):
    /// a value &gt;= 0 is a single node number; a value &lt; 0 is the negated lower bound of a
    /// range whose upper bound is the immediately following list element. The node numbers /
    /// ranges are disjoint, so the disjunction never produces duplicate rows.
    /// </remarks>
    public static class FdbNidWhereClause
    {
        /// <summary>
        /// Returns a predicate such as
        /// <c>(FDB_NID IN (1,4,9) OR FDB_NID BETWEEN 20 AND 40 OR FDB_NID BETWEEN 55 AND 90)</c>,
        /// always wrapped in parentheses so it can be AND-combined with a user where clause.
        /// NID values are inlined as integer literals (they originate from the trusted spatial
        /// index, and inlining sidesteps every provider's parameter-count limit).
        /// An empty <paramref name="nids"/> list yields <c>(1=0)</c>.
        /// </summary>
        /// <param name="nids">The NID list produced by the spatial search tree.</param>
        /// <param name="nidColumn">The (provider-quoted) NID column name, e.g. <c>"FDB_NID"</c>.</param>
        public static string Build(IReadOnlyList<long> nids, string nidColumn)
        {
            if (String.IsNullOrEmpty(nidColumn))
            {
                throw new ArgumentException("NID column name is required.", nameof(nidColumn));
            }

            List<(long From, long To)> intervals = ParseIntervals(nids);
            if (intervals.Count == 0)
            {
                return "(1=0)";
            }

            intervals.Sort((a, b) => a.From.CompareTo(b.From));

            var merged = new List<(long From, long To)>();
            foreach (var iv in intervals)
            {
                if (merged.Count > 0 && iv.From <= merged[merged.Count - 1].To + 1)
                {
                    var last = merged[merged.Count - 1];
                    if (iv.To > last.To)
                    {
                        merged[merged.Count - 1] = (last.From, iv.To);
                    }
                }
                else
                {
                    merged.Add(iv);
                }
            }

            var inValues = new List<long>();
            var betweens = new List<string>();
            foreach (var (from, to) in merged)
            {
                if (to - from <= 1)
                {
                    for (long v = from; v <= to; v++)
                    {
                        inValues.Add(v);
                    }
                }
                else
                {
                    betweens.Add(nidColumn + " BETWEEN " + from + " AND " + to);
                }
            }

            var terms = new List<string>();
            if (inValues.Count > 0)
            {
                terms.Add(nidColumn + " IN (" + String.Join(",", inValues) + ")");
            }
            terms.AddRange(betweens);

            return terms.Count == 1
                ? "(" + terms[0] + ")"
                : "(" + String.Join(" OR ", terms) + ")";
        }

        /// <summary>
        /// Appends <paramref name="oidColumn"/> as the final <c>ORDER BY</c> term so that
        /// <c>LIMIT</c>/<c>OFFSET</c> paging is deterministic even when the caller's sort key has
        /// duplicate values. If <paramref name="orderBy"/> already references an OID column the
        /// string is returned unchanged; if it is empty, <paramref name="oidColumn"/> alone is
        /// returned.
        /// </summary>
        public static string OrderByWithOidTiebreaker(string orderBy, string oidColumn)
        {
            if (String.IsNullOrWhiteSpace(orderBy))
            {
                return oidColumn;
            }

            return ReferencesOid(orderBy) ? orderBy : orderBy + ", " + oidColumn;
        }

        private static bool ReferencesOid(string orderBy)
        {
            foreach (string raw in orderBy.Split(
                new[] { ' ', ',', '\t', '\r', '\n', '(', ')', '[', ']', '"', '`' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                if (raw.Equals("OID", StringComparison.OrdinalIgnoreCase) ||
                    raw.EndsWith("_OID", StringComparison.OrdinalIgnoreCase) ||
                    raw.EndsWith(".OID", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<(long From, long To)> ParseIntervals(IReadOnlyList<long> nids)
        {
            var result = new List<(long, long)>();
            if (nids == null)
            {
                return result;
            }

            for (int i = 0; i < nids.Count; i++)
            {
                long value = nids[i];
                if (value < 0)
                {
                    long from = -value;
                    long to = (i + 1 < nids.Count) ? nids[i + 1] : from;
                    i++;

                    if (to < from)
                    {
                        (from, to) = (to, from);
                    }

                    result.Add((from, to));
                }
                else
                {
                    result.Add((value, value));
                }
            }

            return result;
        }
    }
}
