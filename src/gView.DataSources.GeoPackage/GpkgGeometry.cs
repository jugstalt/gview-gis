using gView.Framework.Core.Geometry;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("gView.DataSources.GeoPackage.Tests")]

namespace gView.DataSources.GeoPackage
{
    /// <summary>
    /// GeoPackage Binary (GPB) blob codec - a small fixed header followed by standard OGC WKB
    /// (GeoPackage 1.2 spec, clause 2.1.3). Pure managed code: gView wraps / unwraps the header
    /// itself so a GeoPackage feature database needs no <c>mod_spatialite</c> at runtime.
    /// </summary>
    public static class GpkgGeometry
    {
        private const byte MagicG = 0x47; // 'G'
        private const byte MagicP = 0x50; // 'P'

        /// <summary>True if <paramref name="blob"/> starts with the <c>GP</c> GeoPackage magic.</summary>
        public static bool IsGpb(byte[] blob)
            => blob != null && blob.Length >= 8 && blob[0] == MagicG && blob[1] == MagicP;

        /// <summary>
        /// Wraps standard OGC WKB (as produced by the FDB WKB codec) in a GPB header carrying
        /// <paramref name="srid"/> and, when <paramref name="envelope"/> is a real 2D box, the
        /// [minx,maxx,miny,maxy] envelope.
        /// </summary>
        public static byte[] ToGpb(byte[] wkb, int srid, IEnvelope envelope)
        {
            if (wkb == null)
            {
                return null;
            }

            bool hasEnvelope = envelope != null
                && !double.IsNaN(envelope.MinX) && !double.IsNaN(envelope.MaxX)
                && !double.IsNaN(envelope.MinY) && !double.IsNaN(envelope.MaxY);

            int envBytes = hasEnvelope ? 32 : 0;
            var gpb = new byte[8 + envBytes + wkb.Length];

            gpb[0] = MagicG;
            gpb[1] = MagicP;
            gpb[2] = 0x00;                                   // version 0
            gpb[3] = (byte)(0x01 | (hasEnvelope ? 0x02 : 0x00)); // bit0: little-endian header ints; bits1-3: 1 => xy envelope

            BinaryPrimitives.WriteInt32LittleEndian(gpb.AsSpan(4, 4), srid);

            int pos = 8;
            if (hasEnvelope)
            {
                BinaryPrimitives.WriteDoubleLittleEndian(gpb.AsSpan(pos, 8), envelope.MinX); pos += 8;
                BinaryPrimitives.WriteDoubleLittleEndian(gpb.AsSpan(pos, 8), envelope.MaxX); pos += 8;
                BinaryPrimitives.WriteDoubleLittleEndian(gpb.AsSpan(pos, 8), envelope.MinY); pos += 8;
                BinaryPrimitives.WriteDoubleLittleEndian(gpb.AsSpan(pos, 8), envelope.MaxY); pos += 8;
            }

            Buffer.BlockCopy(wkb, 0, gpb, pos, wkb.Length);
            return gpb;
        }

        /// <summary>Strips the GPB header and returns the contained standard OGC WKB.</summary>
        public static byte[] ToWkb(byte[] gpb)
        {
            if (gpb == null)
            {
                return null;
            }
            if (!IsGpb(gpb))
            {
                return gpb; // already plain WKB (tolerant)
            }

            int flags = gpb[3];
            int envCode = (flags >> 1) & 0x07;
            int envBytes = envCode switch
            {
                0 => 0,
                1 => 32,   // xy
                2 => 48,   // xyz
                3 => 48,   // xym
                4 => 64,   // xyzm
                _ => throw new FormatException($"Invalid GPB envelope indicator {envCode}"),
            };

            int wkbStart = 8 + envBytes;
            if (wkbStart > gpb.Length)
            {
                throw new FormatException("Truncated GeoPackage geometry blob");
            }

            var wkb = new byte[gpb.Length - wkbStart];
            Buffer.BlockCopy(gpb, wkbStart, wkb, 0, wkb.Length);
            return wkb;
        }

        /// <summary>SRS id stored in a GPB header (0 when the blob is plain WKB).</summary>
        public static int ReadSrid(byte[] gpb)
            => IsGpb(gpb) ? BinaryPrimitives.ReadInt32LittleEndian(gpb.AsSpan(4, 4)) : 0;
    }
}
