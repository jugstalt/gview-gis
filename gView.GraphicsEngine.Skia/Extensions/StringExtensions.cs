using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace gView.GraphicsEngine.Skia.Extensions
{
    static class StringExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string RemoveReturns(this string txt)
        {
            if (txt.Contains("\r"))
                return txt.Replace("\r", "");

            return txt;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool IsMultiline(this string txt) => txt.Contains("\n");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public string[] GetLines(this string txt) => txt.Split('\n');
    }
}
