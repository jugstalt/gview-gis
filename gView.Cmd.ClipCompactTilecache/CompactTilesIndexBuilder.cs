using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace gView.Cmd.ClipCompactTilecache
{
    class CompactTilesIndexBuilder
    {
        private int[] _index = new int[128 * 128 * 2];

        public CompactTilesIndexBuilder()
        {
            InitIndex();
        }

        private void InitIndex()
        {
            for (int i = 0; i < _index.Length; i++)
                _index[i] = -1;
        }

        public void SetValue(int row, int col, int position, int length)
        {
            if (row < 0 || row > 128 || col < 0 || col > 128)
                throw new ArgumentException("Compact Tile Index out of range");

            int indexPosition = ((row * 128) + col) * 2;
            if (indexPosition > _index.Length - 2)
                throw new AggregateException("Index!!!");

            _index[indexPosition] = position;
            _index[indexPosition + 1] = length;
        }

        public void Save(string filename)
        {
            try { File.Delete(filename); }
            catch { }

            using (var stream = new FileStream(filename, FileMode.Create))
            {
                for (int i = 0; i < _index.Length; i++)
                {
                    byte[] data = BitConverter.GetBytes(_index[i]);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
