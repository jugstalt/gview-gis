using System;

namespace gView.DataSources.GDAL
{
    public class Int24
    {
        private int _absValue;
        private bool _isNegative;

        public Int24(int val)
        {
            _isNegative = val < 0;

            _absValue = Math.Abs(val);
            _absValue &= 0x7fffff;
        }

        public Int24(float val)
            : this((int)val)
        {

        }

        public int Value => _absValue * (_isNegative ? -1 : 1);

        public byte[] GetBytes()
        {
            var bytes = BitConverter.GetBytes(_absValue);

            if (_isNegative)
            {
                bytes[2] += 0x80;
            }

            return new byte[] { bytes[0], bytes[1], bytes[2] };
        }
    }

    public class UInt24
    {
        private int _value;

        public UInt24(int val)
        {
            if (val < 0)
            {
                throw new ArgumentException("No negative values allowed");
            }

            _value = Math.Abs(val);
            _value &= 0xffffff;
        }

        public UInt24(float val)
            : this((int)val)
        {

        }

        public int Value => _value;

        public byte[] GetBytes()
        {
            var bytes = BitConverter.GetBytes(_value);

            return new byte[] { bytes[0], bytes[1], bytes[2] };
        }
    }
}
