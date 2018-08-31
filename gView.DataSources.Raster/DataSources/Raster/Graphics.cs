using System;
using System.Collections.Generic;
using System.Text;

namespace gView.DataSources.Raster
{
    internal class Bitmap8pbbIndexed
    {
        private System.Drawing.Bitmap _bm = null;
        private System.Drawing.Imaging.BitmapData _bmdata = null;
        private int _iWidth = 0, _iHeight = 0, _stride = 0, _move = 0;
        private int[] _bit = { 1, 2, 4, 8, 16, 32, 64, 128 };

        public Bitmap8pbbIndexed(System.Drawing.Bitmap bm)
        {
            _bm = bm;
            if (_bm != null)
            {
                _bmdata = _bm.LockBits(new System.Drawing.Rectangle(0, 0, bm.Width, bm.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, _bm.PixelFormat);
                _iWidth = _bm.Width;
                _iHeight = _bm.Height;
                _stride = _bmdata.Stride;

                switch (_bm.PixelFormat)
                {
                    case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                        _move = 0;
                        break;
                    case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                        _move = 3;
                        break;
                }
            }
        }

        public void Dispose(bool disposeBitmap)
        {
            if (_bmdata != null && _bm != null)
            {
                _bm.UnlockBits(_bmdata);
                _bmdata = null;
            }
            if (disposeBitmap && _bm != null) _bm.Dispose();
        }

        public byte ReadPixel(int x, int y)
        {
            unsafe
            {
                if (x >= _iWidth || y >= _iHeight || x < 0 || y < 0) return 0;

                byte* ptr = (byte*)(void*)_bmdata.Scan0;
                ptr += y * _stride + (x >> _move);
                if (_move == 3)
                {
                    return ((int)(*ptr) & _bit[7 - (x % 8)]) > 0 ? (byte)1 : (byte)0;
                }
                return *ptr;
            }
        }

        public void WritePixel(int x, int y, byte val)
        {
            unsafe
            {
                if (x >= _iWidth || y >= _iHeight || x < 0 || y < 0) return;

                byte* ptr = (byte*)(void*)_bmdata.Scan0;
                ptr += y * _stride + (x >> _move);
                if (_move == 3)
                {
                    if (val == 0) return;
                    *ptr = (byte)((int)*ptr | _bit[7 - (x % 8)]);
                    return;
                }
                *ptr = val;
            }
        }

        public void DrawImage(Bitmap8pbbIndexed source, System.Drawing.Rectangle destrect, System.Drawing.Rectangle sourcerect)
        {
            if (destrect.X > _iWidth || destrect.Y > _iHeight) return;

            if (sourcerect.X > source._iWidth ||
                sourcerect.Y > source._iHeight ||
                source._iWidth == 0 ||
                source._iHeight == 0) return;

            if (sourcerect.X + sourcerect.Width > source._iWidth)
                sourcerect.Width = source._iWidth - sourcerect.X;
            if (sourcerect.Y + sourcerect.Height > source._iHeight)
                sourcerect.Height = source._iHeight - sourcerect.Y;

            int minx = Math.Max(0, destrect.X);
            int miny = Math.Max(0, destrect.Y);
            int maxx = Math.Min(destrect.X + destrect.Width, _iWidth);
            int maxy = Math.Min(destrect.Y + destrect.Height, _iHeight);

            float stepX = (float)sourcerect.Width / (float)destrect.Width;
            float stepY = (float)sourcerect.Height / (float)destrect.Height;

            //Console.WriteLine("stepX:" + stepX.ToString() + "  stepY:" + stepY.ToString());
            //stepX = stepY = 2;

            unsafe
            {
                byte* ptr = (byte*)(void*)_bmdata.Scan0;
                int y, x;
                float X, Y;

                for (y = miny, Y = sourcerect.Y; y < maxy; y++, Y += stepY)
                {
                    for (x = minx, X = sourcerect.X; x < maxx; x++, X += stepX)
                    {
                        this.WritePixel(x,y,source.ReadPixel((int)X,(int)Y));
                        //*(ptr + y * _stride + (x >> _move)) = source.ReadPixel((int)X, (int)Y);
                    }
                }
            }
        }
    }
}
