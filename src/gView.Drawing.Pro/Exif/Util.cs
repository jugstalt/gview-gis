using System;

namespace gView.Drawing.Pro.Exif
{
    internal sealed class Rational
    {
        private Int32 _num;
        private Int32 _denom;

        public Rational(byte[] bytes)
        {
            byte[] n = new byte[4];
            byte[] d = new byte[4];
            Array.Copy(bytes, 0, n, 0, 4);
            Array.Copy(bytes, 4, d, 0, 4);
            _num = BitConverter.ToInt32(n, 0);
            _denom = BitConverter.ToInt32(d, 0);
        }

        public double ToDouble()
        {
            return Math.Round(Convert.ToDouble(_num) / Convert.ToDouble(_denom), 2);
        }

        public string ToString(string separator)
        {
            return _num.ToString() + separator + _denom.ToString();
        }

        public override string ToString()
        {
            return this.ToString("/");
        }
    }

    internal sealed class URational
    {
        private UInt32 _num;
        private UInt32 _denom;

        public URational(byte[] bytes)
        {
            byte[] n = new byte[4];
            byte[] d = new byte[4];
            Array.Copy(bytes, 0, n, 0, 4);
            Array.Copy(bytes, 4, d, 0, 4);
            _num = BitConverter.ToUInt32(n, 0);
            _denom = BitConverter.ToUInt32(d, 0);
        }

        public double ToDouble()
        {
            return Math.Round(Convert.ToDouble(_num) / Convert.ToDouble(_denom), 2);
        }

        public override string ToString()
        {
            return this.ToString("/");
        }

        public string ToString(string separator)
        {
            return _num.ToString() + separator + _denom.ToString();
        }
    }

    internal sealed class GPSRational
    {
        private Rational _hours;
        private URational _minutes;
        private URational _seconds;

        public Rational Hours
        {
            get
            {
                return _hours;
            }
            set
            {
                _hours = value;
            }
        }
        public URational Minutes
        {
            get
            {
                return _minutes;
            }
            set
            {
                _minutes = value;
            }
        }
        public URational Seconds
        {
            get
            {
                return _seconds;
            }
            set
            {
                _seconds = value;
            }
        }

        public GPSRational(byte[] bytes)
        {
            byte[] h = new byte[8]; byte[] m = new byte[8]; byte[] s = new byte[8];

            Array.Copy(bytes, 0, h, 0, 8); Array.Copy(bytes, 8, m, 0, 8); Array.Copy(bytes, 16, s, 0, 8);

            _hours = new Rational(h);
            _minutes = new URational(m);
            _seconds = new URational(s);
        }

        public override string ToString()
        {
            return _hours.ToDouble() + "° "
                + _minutes.ToDouble() + "\' "
                + _seconds.ToDouble() + "\"";
        }

        public string ToString(string separator)
        {
            return _hours.ToDouble() + separator
                + _minutes.ToDouble() + separator +
                _seconds.ToDouble();
        }
    }
}
