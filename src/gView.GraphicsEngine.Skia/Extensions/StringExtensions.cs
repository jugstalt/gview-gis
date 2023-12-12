using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string RemoveReturns(this string txt)
        {
            if (txt.Contains("\r"))
            {
                return txt.Replace("\r", "");
            }

            return txt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsMultiline(this string txt) => txt.Contains("\n");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string[] GetLines(this string txt) => txt.Split('\n');

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //static public int LinesCount(this string txt) => txt.Count((c) => c == '\n') + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public int LinesCount(this ReadOnlySpan<char> txt)
        {
            int count = 0;
            for (int i = 0, to = txt.Length; i < to; i++)
            {
                if (txt[i] == '\n')
                {
                    count++;
                }
            }
            return count + 1;
        }
    }
}
