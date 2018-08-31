using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using gView.Framework.Geometry;
using gView.Framework.Data;

namespace gView.DataSources.Raster
{
    public class TFWFile : IRasterWorldFile 
    {
        double _dx_x = 1.0;
        double _dx_y = 0.0;
        double _dy_x = 0.0;
        double _dy_y = -1.0;
        double _X = 0.0, _Y = 0.0;
        double _detA=1.0;
        bool _valid = false;
        string _filename=String.Empty;

        public TFWFile(string filename)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (!fi.Exists) return;
                _filename = filename;

                StreamReader sr = new StreamReader(filename);
                string line;
                int pos = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line == "") continue;
                    double val = 0.0;
                    try
                    {
                        val = Convert.ToDouble(line.Replace(".",","));
                    }
                    catch
                    {
                        continue;
                    }
                    switch (pos)
                    {
                        case 0:
                            _dx_x = val;
                            break;
                        case 1:
                            _dx_y = val;
                            break;
                        case 2:
                            _dy_x = val;
                            break;
                        case 3:
                            _dy_y = val;
                            break;
                        case 4:
                            _X = val;
                            break;
                        case 5:
                            _Y = val;
                            break;
                    }
                    pos++;
                    if (pos > 5) break;
                }
                sr.Close();

                _valid = true;
            }
            catch { }

            // | dx_x   dx_y |
            // | dy_x   dy_y |
            _detA = _dx_x * _dy_y - _dx_y * _dy_x;
        }
        public TFWFile(double x, double y, double dx1, double dx2, double dy1, double dy2)
        {
            _X = x;
            _Y = y;
            _dx_x = dx1;
            _dx_y = dx2;
            _dy_x = dy1;
            _dy_y = dy2;

            // | dx_x   dx_y |
            // | dy_x   dy_y |
            _detA = _dx_x * _dy_y - _dx_y * _dy_x;
            _valid = true;
        }

        public bool isValid
        {
            get { return _valid; }
            internal set { _valid = value; }
        }
        

        public string Filename { get { return _filename; } set { _filename = value; } }

        
        public double dx_X { get { return _dx_x; } }
        public double dx_Y { get { return _dx_y; } }
        public double dy_X { get { return _dy_x; } }
        public double dy_Y { get { return _dy_y; } }

        public double X { get { return _X; } }
        public double Y { get { return _Y; } }

        public double cellX
        {
            get { return Math.Sqrt(_dx_x * _dx_x + _dx_y * _dx_y); }
        }
        public double cellY
        {
            get { return Math.Sqrt(_dy_x * _dy_x + _dy_y * _dy_y); }
        }

        public void World2Image(double X, double Y, out int x, out int y)
        {
            if (_dx_y == 0.0 && _dy_x == 0.0)
            {
                x = (int)((X - _X) / _dx_x);
                y = (int)((Y - _Y) / _dy_y);
            }
            else
            {
                double dX = X - _X;
                double dY = Y - _Y;

                // | dX   dx_y |
                // | dY   dy_y |
                double detX = dX * _dy_y - _dx_y * dY;
                // | dx_x  dX |
                // | dy_x  dY |
                double detY = _dx_x * dY - dX * _dy_x;
                x = (int)(detX / _detA);
                y = (int)(detY / _detA);
            }
        }

        public void Split(string filetitle, string outputPath, int width, int height, int xSplit, int ySplit)
        {
            int w = width / xSplit;
            int h = height / ySplit;

            int count=0;
            for (int y = 0; y < ySplit; y++)
            {
                for (int x = 0; x < xSplit; x++)
                {
                    double X = _X + dx_X * w * x + dx_Y * w * x;
                    double Y = _Y + dy_X * h * y + dy_Y * h * y;

                    StreamWriter sw = new StreamWriter(outputPath + @"\" + filetitle + "_" + count + ".tfw");
                    sw.WriteLine(dx_X.ToString().Replace(",", "."));
                    sw.WriteLine(dx_Y.ToString().Replace(",", "."));
                    sw.WriteLine(dy_X.ToString().Replace(",", "."));
                    sw.WriteLine(dy_Y.ToString().Replace(",", "."));
                    sw.WriteLine(X.ToString().Replace(",", "."));
                    sw.WriteLine(Y.ToString().Replace(",", "."));
                    sw.Close();

                    count++;
                }
            }
        }

        public  Polygon CreatePolygon(int width, int height)
        {
            Polygon polygon = new Polygon();
            Ring ring = new Ring();
            gView.Framework.Geometry.Point p1 = new gView.Framework.Geometry.Point(
                X - dx_X / 2.0 - dy_X / 2.0,
                Y - dx_Y / 2.0 - dy_Y / 2.0);

            ring.AddPoint(p1);
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dx_X * width, p1.Y + dx_Y * width));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dx_X * width + dy_X * height, p1.Y + dx_Y * width + dy_Y * height));
            ring.AddPoint(new gView.Framework.Geometry.Point(p1.X + dy_X * height, p1.Y + dy_Y * height));
            polygon.AddRing(ring);

            return polygon;
        }

        internal void Project(vector2[] vecs)
        {
            //matrix22 A = new matrix22(dx_X, dx_Y, dy_X, dy_Y);
            matrix22 A = new matrix22(dx_X, dy_X, dx_Y, dy_Y);
            vector2 X = new vector2(
                _X - dx_X / 2.0 - dy_X / 2.0, 
                _Y - dx_Y / 2.0 - dy_Y / 2.0);

            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i] = X + A * vecs[i];
            }
        }
        internal bool ProjectInv(vector2[] vecs)
        {
            //matrix22 A = new matrix22(dx_X, dx_Y, dy_X, dy_Y);
            matrix22 A = new matrix22(dx_X, dy_X, dx_Y, dy_Y);
            if (!A.Inv()) return false;

            vector2 X = new vector2(
                _X - dx_X / 2.0 - dy_X / 2.0,
                _Y - dx_Y / 2.0 - dy_Y / 2.0);

            for (int i = 0; i < vecs.Length; i++)
            {
                vecs[i] = A * (vecs[i]-X);
            }
            return true;
        }

        #region IRasterWorldFile Member

        public bool isGeoReferenced
        {
            get
            {
                if (!_valid) return false;

                if (_filename != String.Empty && _filename != null) return true;

                if (X == 0.0 && Y == 0.0 &&
                    dx_X == 1.0 && dx_Y == 0.0 &&
                    dy_Y == 1.0 && dy_X == 0.0)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion
    }

    internal class vector2
    {
        public double x, y;

        public vector2(double X, double Y)
        {
            x = X;
            y = Y;
        }

        static public vector2 operator -(vector2 v1,vector2 v2)
        {
            return new vector2(v1.x - v2.x, v1.y - v2.y);
        }

        static public vector2 operator +(vector2 v1, vector2 v2)
        {
            return new vector2(v1.x + v2.x, v1.y + v2.y);
        }

        static public IEnvelope Envelope(vector2[] vecs)
        {
            Envelope env = null;
            foreach (vector2 vec in vecs)
            {
                if (env == null)
                {
                    env = new Envelope(vec.x, vec.y, vec.x, vec.y);
                }
                else
                {
                    env.minx = Math.Min(env.minx, vec.x);
                    env.miny = Math.Min(env.miny, vec.y);
                    env.maxx = Math.Max(env.maxx, vec.x);
                    env.maxy = Math.Max(env.maxy, vec.y);
                }
            }
            return env;
        }

        static public IEnvelope IntegerEnvelope(vector2[] vecs)
        {
            IEnvelope env = vector2.Envelope(vecs);

            env.minx = Math.Floor(env.minx);
            env.miny = Math.Floor(env.miny);
            env.maxx = Math.Floor(env.maxx) + 1;
            env.maxy = Math.Floor(env.maxy) + 1;

            return env;
        }
    }

    internal class matrix22
    {
        private double a, b, c, d;
        public matrix22(double x00, double x01, double x10, double x11)
        {
            a = x00;
            b = x01;
            c = x10;
            d = x11;
        }

        public bool Inv() 
        {
            double det = a * d - b * c;
            if(det==0.0) return false;
            
            // swap a/d
            double h = a;
            a = d; d = h;

            a /= det;
            d /= det;

            b = -b / det;
            c = -c / det;

            return true;
        }

        static public vector2 operator *(matrix22 m, vector2 v)
        {
            return new vector2(
                m.a * v.x + m.b * v.y,
                m.c * v.x + m.d * v.y);
        }
    }
    public class ImageSplitter
    {
        public ImageSplitter(string filename, string outputPath,int xSplit,int ySplit)
        {
            try
            {
                FileInfo fi = new FileInfo(filename);
                if (!fi.Exists) return;

                Image image = Image.FromFile(filename);
                string fname = filename.Substring(0, filename.Length - fi.Extension.Length);
                string ftitle = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);

                string tfwfilename =  fname + ".tfw";
                TFWFile tfw = new TFWFile(tfwfilename);

                int w = image.Width / xSplit;
                int h = image.Height / ySplit;

                int count = 0;
                for (int y = 0; y < ySplit; y++)
                {
                    for (int x = 0; x < xSplit; x++)
                    {
                        Bitmap bm = new Bitmap(w, h);
                        using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bm))
                        {
                            gr.DrawImage(image,
                                new Rectangle(0, 0, w, h),
                                x * w, y * h, w, h, GraphicsUnit.Pixel);
                        }
                        bm.Save(outputPath + @"\" + ftitle + "_" + count + ".png");
                        bm.Dispose();
                        bm = null;
                        count++;
                    }
                }
                tfw.Split(ftitle, outputPath, image.Width, image.Height, xSplit, ySplit);
            }
            catch
            {
            }
        }
    }
}
