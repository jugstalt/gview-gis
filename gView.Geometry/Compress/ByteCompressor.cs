using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.Geometry.Compress
{
    class ByteCompressor
    {
        static internal byte[] Compress(int val)
        {
            bool isNeg = val < 0;
            if (isNeg) val = -val;

            byte[] c = new byte[5];
            for (int i = 0; i < 5; i++)
            {
                if (i == 0)
                {
                    c[i] = (byte)(val & 0x3f);
                    val = val >> 6;
                    if (isNeg) c[i] |= 0x40;
                }
                else
                {
                    c[i] = (byte)(val & 0x7f);
                    val = val >> 7;
                }
                if (val == 0)
                    break;
            }

            byte[] ret = TrimBytes(c);
            ret[ret.Length - 1] |= 0x80; // letztes als StopByte markieren (oberstes Bit)

            return ret;
        }

        static internal byte[] TrimBytes(byte[] bytes)
        {
            int len = bytes.Length;
            for (int i = len - 1; i >= 0; i--)
            {
                if (bytes[i] != 0)
                    break;
                len--;
            }

            byte[] ret = new byte[len == 0 ? 1 : len];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = bytes[i];
            return ret;
        }

        static internal int[] Decompress(byte[] bytes, int intCount)
        {
            int[] ret = new int[intCount];

            int m = 0, v = 0, index = 0;
            bool isNeg = false;
            for (int i = 0; i < bytes.Length; i++)
            {
                byte b = bytes[i];
                bool stopBit = ((b & 0x80) == 0x80);
                if (stopBit) { b -= (byte)0x80; }

                if (m == 0 && ((b & 0x40) == 0x40))
                {
                    isNeg = true;
                    b -= (byte)0x40;
                }
                v = v + (b << m);
                if (stopBit)
                {
                    if (isNeg) v = -v;
                    ret[index++] = v;
                    v = m = 0;
                    isNeg = false;
                }
                else
                {
                    m += (m == 0 ? 6 : 7);
                }
            }

            return ret;
        }
    }
}
