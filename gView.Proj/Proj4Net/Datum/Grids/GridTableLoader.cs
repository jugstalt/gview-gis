using System;
using System.IO;

namespace Proj4Net.Datum.Grids
{
    /// <summary>
    /// Grid table loader class
    /// </summary>
    public abstract class GridTableLoader
    {
        protected readonly Uri _uri;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="location"></param>
        protected GridTableLoader(Uri location)
        {
            _uri = location;
        }

        /// <summary>
        /// Gets the stream from the
        /// </summary>
        /// <returns></returns>
        protected Stream OpenGridTableStream()
        {
            if (UriEx.IsFile(_uri))
                return File.OpenRead(_uri.LocalPath);
            throw new NotSupportedException();
        }

        /// <summary>
        /// Parses the grid table header and returns an appropriate <see cref="GridTable"/>
        /// </summary>
        /// <param name="table">The grid tabel to initialize</param>
        /// <returns>true if the header could be read.</returns>
        internal abstract bool ReadHeader(GridTable table);
        
        /// <summary>
        /// Parses the grid table data
        /// </summary>
        /// <param name="table">The table to fill</param>
        /// <returns>true if the data could be read.</returns>
        internal abstract bool ReadData(GridTable table);

        protected double ReadBigEndianDouble(BinaryReader reader)
        {
            var bytes = reader.ReadBytes(8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        protected double GetBigEndianDouble(byte[] buffer, int offset)
        {
            var bytes = new byte[8];
            Buffer.BlockCopy(bytes, offset, bytes, 0, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }
    }

    public static class UriEx
    {
        public static bool IsFile(Uri self)
        {
#if SILVERLIGHT
            return false;
#else
            return self.IsFile;
#endif
        }
    }

}