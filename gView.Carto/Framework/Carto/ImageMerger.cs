using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using gView.Framework.Data;
using System.Collections.Generic;
using gView.Framework.system;
using gView.Framework.Symbology;
using gView.Framework.Carto.Graphics;

namespace gView.Framework.Carto
{
	/// <summary>
	/// Zusammenfassung für ImageMerger.
	/// </summary>
	internal class ImageMerger
	{
		protected ArrayList m_picList,m_orderList;
		protected int m_max=0;
		protected string m_outputUrl="";
		protected string m_outputPath="";
		protected double m_scale=-1.0,m_dpi=96.0;
		 
		public ImageMerger()
		{
			m_picList=new ArrayList();
			m_orderList=new ArrayList();
		}

		public void clear() 
		{
			m_picList.Clear();
			m_orderList.Clear();
			m_max=0;
			m_scale=-1.0;
		}
		public int Count 
		{
			get 
			{
				return m_picList.Count;
			}
		}
		public double mapScale 
		{
			set { m_scale=value; }
		}
		public double dpi 
		{
			set { m_dpi=value; }
		}
		public string outputPath 
		{
			set { m_outputPath=value; }
			get { return m_outputPath; }
		}
		public string outputUrl
		{
			set { m_outputUrl=value; }
			get { return m_outputPath; }
		}
		public void Add(string path,int order) 
		{
			for(int i=0;i<m_picList.Count;i++) 
			{
				int o=Convert.ToInt32(m_orderList[i]);
				if(order<o) 
				{
					m_picList.Insert(i,path);
					m_orderList.Insert(i,order);
					return;
				}
			}

			m_picList.Add(path);
			m_orderList.Add(order);
		}
		
		public int max
		{
			get { return m_max; }
			set { m_max=value; }
		}

		public string Merge(int iWidth,int iHeight,out string imageUrl)
		{
			imageUrl="";

			try 
			{
				DateTime time=DateTime.Now;
				string filename="merged_"+
					time.Day.ToString()+time.Hour.ToString()+
					time.Second.ToString()+time.Millisecond.ToString()+".png";
			
				Bitmap image=new Bitmap(iWidth,iHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				System.Drawing.Graphics gr=System.Drawing.Graphics.FromImage(image);
				SolidBrush brush=new SolidBrush(Color.White);
				gr.FillRectangle(brush,0,0,iWidth,iHeight);
				brush.Dispose(); brush=null;

				foreach(string pic in m_picList) 
				{
					Image img=Image.FromFile(pic);
					gr.DrawImage(img,
                        new Rectangle(0,0,iWidth,iHeight),
                        new Rectangle(0,0,img.Width,img.Height),
                        GraphicsUnit.Pixel);
                }

				if(m_scale>0) 
				{
					scalebar bar=new scalebar(m_scale,m_dpi);
					bar.Create(ref image,image.Width-(int)(50*m_dpi/96.0)-bar.ScaleBarWidth,image.Height-(int)(32*m_dpi/96.0));
				}
				imageUrl=m_outputUrl+"/"+filename;
				filename=m_outputPath+@"\"+filename;

				image.Save(filename,ImageFormat.Png);
				return filename;
			} 
			catch { return ""; }
		}
	}


	internal class ImageMerger2 : IDisposable
	{
        protected List<GeorefBitmap> m_picList;
		protected List<int> m_orderList;
		protected int m_max=0;
		protected double m_scale=-1.0,m_dpi=96.0;

        private string _errMsg = String.Empty;
        private object lockThis = new object();

		public ImageMerger2()
		{
            m_picList = new List<GeorefBitmap>();
            m_orderList = new List<int>();
		}

        public void Dispose()
        {
            this.Clear();
        }

		public void Clear() 
		{
            foreach (var georefBitmap in m_picList)
                georefBitmap.Dispose();

			m_picList.Clear();
			m_orderList.Clear();
			m_max=0;
			m_scale=-1.0;
		}
		public int Count 
		{
			get 
			{
				return m_picList.Count;
			}
		}
		public double mapScale 
		{
			set { m_scale=value; }
		}
		public double dpi 
		{
			set { m_dpi=value; }
		}
		
		public void Add(GeorefBitmap image, int order) 
		{
            lock (lockThis)
            {
                for (int i = 0; i < m_picList.Count; i++)
                {
                    int o = Convert.ToInt32(m_orderList[i]);
                    if (order < o)
                    {
                        m_picList.Insert(i, image);
                        m_orderList.Insert(i, order);
                        return;
                    }
                }

                m_picList.Add(image);
                m_orderList.Add(order);
            }
		}
		
		public int max
		{
			get { return m_max; }
			set { m_max=value; }
		}

        public string LastErrorMessage
        {
            get { return _errMsg; }
        }

		public bool Merge(Bitmap image, IDisplay display)
		{
			try 
			{
				int iWidth=image.Width;
				int iHeight=image.Height;

                using (System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(image))
                {
                    gr.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

                    foreach (GeorefBitmap geoBmp in m_picList)
                    {
                        if (geoBmp == null || geoBmp.Bitmap == null) continue;
                        if (image != geoBmp.Bitmap)
                        {
                            if (geoBmp.Envelope != null)
                            {
                                double x0, y0, x1, y1, x2, y2;
                                gView.Framework.Geometry.IGeometry geom = gView.Framework.Geometry.GeometricTransformer.Transform2D(geoBmp.Envelope, geoBmp.SpatialReference, display.SpatialReference);
                                if (geom is gView.Framework.Geometry.IPolygon)
                                {
                                    gView.Framework.Geometry.IRing ring = ((gView.Framework.Geometry.IPolygon)geom)[0];

                                    x0 = ring[1].X; y0 = ring[1].Y;
                                    x1 = ring[2].X; y1 = ring[2].Y;
                                    x2 = ring[0].X; y2 = ring[0].Y;

                                    /////////////////////////////////////////////////////////
                                    Display d = new Display(false);
                                    d.Limit = d.Envelope = geoBmp.Envelope;
                                    d.iWidth = geoBmp.Bitmap.Width;
                                    d.iHeight = geoBmp.Bitmap.Height;
                                    d.SpatialReference = geoBmp.SpatialReference;
                                    Resample(image, display, geoBmp.Bitmap, d);
                                    continue;

                                    gView.Framework.Geometry.GeometricTransformer transformer = new gView.Framework.Geometry.GeometricTransformer();
                                    //transformer.FromSpatialReference = geoBmp.SpatialReference;
                                    //transformer.ToSpatialReference = display.SpatialReference;
                                    transformer.SetSpatialReferences(geoBmp.SpatialReference, display.SpatialReference);

                                    for (int y = 0; y < image.Height; y++)
                                    {
                                        for (int x = 0; x < image.Width; x++)
                                        {
                                            double xx = x, yy = y;
                                            display.Image2World(ref xx, ref yy);
                                            gView.Framework.Geometry.IPoint point = (gView.Framework.Geometry.IPoint)transformer.InvTransform2D(new gView.Framework.Geometry.Point(xx, yy));
                                            xx = point.X; yy = point.Y;
                                            d.World2Image(ref xx, ref yy);
                                            try
                                            {
                                                int x_ = (int)xx, y_ = (int)yy;
                                                if (x_ >= 0 && y_ < geoBmp.Bitmap.Width &&
                                                    y_ > 0 && y_ < geoBmp.Bitmap.Height)
                                                {

                                                    image.SetPixel(x, y, geoBmp.Bitmap.GetPixel(x_, y_));

                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                    transformer.Release();
                                    continue;
                                    //////////////////////////////////////////////////////////
                                }
                                else
                                {
                                    x0 = geoBmp.Envelope.minx; y0 = geoBmp.Envelope.maxy;
                                    x1 = geoBmp.Envelope.maxx; y1 = geoBmp.Envelope.maxy;
                                    x2 = geoBmp.Envelope.minx; y2 = geoBmp.Envelope.miny;
                                }

                                display.World2Image(ref x0, ref y0);
                                display.World2Image(ref x1, ref y1);
                                display.World2Image(ref x2, ref y2);

                                PointF[] points ={
                                new PointF((float)x0,(float)y0),
                                new PointF((float)x1,(float)y1),
                                new PointF((float)x2,(float)y2) };

                                if (geoBmp.Opacity >= 0 && geoBmp.Opacity < 1.0)
                                {
                                    float[][] ptsArray ={ 
                                        new float[] {1, 0, 0, 0, 0},
                                        new float[] {0, 1, 0, 0, 0},
                                        new float[] {0, 0, 1, 0, 0},
                                        new float[] {0, 0, 0, geoBmp.Opacity, 0}, 
                                        new float[] {0, 0, 0, 0, 1}};

                                    System.Drawing.Imaging.ColorMatrix clrMatrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
                                    System.Drawing.Imaging.ImageAttributes imgAttributes = new System.Drawing.Imaging.ImageAttributes();
                                    imgAttributes.SetColorMatrix(clrMatrix,
                                                                 System.Drawing.Imaging.ColorMatrixFlag.Default,
                                                                 System.Drawing.Imaging.ColorAdjustType.Bitmap);

                                    // Bitmap "kopieren", sonst kann es Out Of Memory Exceptions kommen!!!
                                    using (Bitmap bm_ = new Bitmap(geoBmp.Bitmap.Width, geoBmp.Bitmap.Height, NonIndedexedPixelFormat(geoBmp)))
                                    {
                                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm_))
                                        {
                                            g.DrawImage(geoBmp.Bitmap, new Point(0, 0));
                                            g.Dispose();
                                        }
                                        gr.DrawImage(bm_,
                                            points,
                                            new RectangleF(0, 0, geoBmp.Bitmap.Width, geoBmp.Bitmap.Height),
                                            GraphicsUnit.Pixel, imgAttributes);
                                        bm_.Dispose();
                                    }

                                }
                                else
                                {
                                    gr.DrawImage(geoBmp.Bitmap,
                                        points,
                                        new RectangleF(0, 0, geoBmp.Bitmap.Width, geoBmp.Bitmap.Height),
                                        GraphicsUnit.Pixel);
                                }
                            }
                            else
                            {
                                if (geoBmp.Opacity >= 0 && geoBmp.Opacity < 1.0)
                                {
                                    float[][] ptsArray ={ 
                                        new float[] {1, 0, 0, 0, 0},
                                        new float[] {0, 1, 0, 0, 0},
                                        new float[] {0, 0, 1, 0, 0},
                                        new float[] {0, 0, 0, geoBmp.Opacity, 0}, 
                                        new float[] {0, 0, 0, 0, 1}};

                                    System.Drawing.Imaging.ColorMatrix clrMatrix = new System.Drawing.Imaging.ColorMatrix(ptsArray);
                                    System.Drawing.Imaging.ImageAttributes imgAttributes = new System.Drawing.Imaging.ImageAttributes();
                                    imgAttributes.SetColorMatrix(clrMatrix,
                                                                 System.Drawing.Imaging.ColorMatrixFlag.Default,
                                                                 System.Drawing.Imaging.ColorAdjustType.Bitmap);

                                     // Bitmap "kopieren", sonst kann es Out Of Memory Exceptions kommen!!!
                                    using (Bitmap bm_ = new Bitmap(geoBmp.Bitmap.Width, geoBmp.Bitmap.Height, NonIndedexedPixelFormat(geoBmp)))
                                    {
                                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bm_))
                                        {
                                            g.DrawImage(geoBmp.Bitmap, new Point(0, 0));
                                            g.Dispose();
                                        }
                                        gr.DrawImage(bm_,
                                            new Rectangle(0, 0, iWidth, iHeight),
                                            0, 0, geoBmp.Bitmap.Width, geoBmp.Bitmap.Height,
                                            GraphicsUnit.Pixel,
                                            imgAttributes);
                                        bm_.Dispose();
                                    }
                                }
                                else
                                {
                                    gr.DrawImage(geoBmp.Bitmap,
                                        new Rectangle(0, 0, iWidth, iHeight),
                                        new Rectangle(0, 0, geoBmp.Bitmap.Width, geoBmp.Bitmap.Height),
                                        GraphicsUnit.Pixel);
                                }
                            }
                        }
                    }
                    gr.Dispose();
                }

				if(m_scale>0) 
				{
					scalebar bar=new scalebar(m_scale,m_dpi);
					bar.Create(ref image,image.Width-(int)(50*m_dpi/96.0)-bar.ScaleBarWidth,image.Height-(int)(32*m_dpi/96.0));
				}
				return true;
			} 
			catch(Exception ex) 
			{
                _errMsg = ex.Message + "\n" + ex.Source + "\n" + ex.StackTrace;
				return false; 
			}
		}

        private PixelFormat NonIndedexedPixelFormat(GeorefBitmap bm)
        {
            if (bm == null || bm.Bitmap == null) return PixelFormat.Format32bppArgb;

            switch (bm.Bitmap.PixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Indexed:
                case PixelFormat.Undefined:
                case PixelFormat.PAlpha:
                    return PixelFormat.Format32bppArgb;
            }

            return bm.Bitmap.PixelFormat;
        }
        public Image Merge()
        {
            System.Drawing.Graphics gr = null;
            Image ret = null;
            try
            {
                foreach (GeorefBitmap geoBmp in m_picList)
                {
                    if (geoBmp == null || geoBmp.Bitmap == null) continue;

                    if (gr == null)
                    {
                        gr = System.Drawing.Graphics.FromImage(ret = geoBmp.Bitmap);
                    }
                    else
                    {
                        gr.DrawImage(geoBmp.Bitmap, 
                            new Rectangle(0, 0, ret.Width, ret.Height), 0, 0,
                            geoBmp.Bitmap.Width, geoBmp.Bitmap.Height, 
                            GraphicsUnit.Pixel);
                        geoBmp.Dispose();
                    }
                }

                this.Clear();
                return ret;
            }
            catch
            {
                return ret;
            }
            finally
            {
                if (gr != null) gr.Dispose();
            }
        }

        public static void Resample(Bitmap dest, IDisplay destDisplay, Bitmap source, IDisplay sourceDisplay)
        {
            BitmapData destData = null, sourceData = null;
            gView.Framework.Geometry.GeometricTransformer transformer = new gView.Framework.Geometry.GeometricTransformer();

            try
            {
                //transformer.FromSpatialReference = sourceDisplay.SpatialReference;
                //transformer.ToSpatialReference = destDisplay.SpatialReference;
                transformer.SetSpatialReferences(sourceDisplay.SpatialReference, destDisplay.SpatialReference);

                destData = dest.LockBits(new Rectangle(0, 0, dest.Width, dest.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int sWidth = source.Width, sHeight = source.Height;
                int dWidth = dest.Width, dHeight = dest.Height;

                unsafe
                {
                    byte* ptr = (byte*)destData.Scan0;

                    for (int y = 0; y < dHeight; y++)
                    {
                        for (int x = 0; x < dWidth; x++)
                        {
                            double xx = x, yy = y;
                            destDisplay.Image2World(ref xx, ref yy);
                            gView.Framework.Geometry.IPoint point = (gView.Framework.Geometry.IPoint)transformer.InvTransform2D(new gView.Framework.Geometry.Point(xx, yy));
                            xx = point.X; yy = point.Y;
                            sourceDisplay.World2Image(ref xx, ref yy);

                            int x_ = (int)xx, y_ = (int)yy;
                            if (x_ >= 0 && x_ < sWidth &&
                                y_ >= 0 && y_ < sHeight)
                            {
                                byte* p = (byte*)sourceData.Scan0;
                                p += (y_ * destData.Stride + x_ * 4);

                                if (p[3] != 0) // Transparenz!!!
                                {
                                    ptr[0] = p[0];
                                    ptr[1] = p[1];
                                    ptr[2] = p[2];
                                    ptr[3] = p[3];
                                }
                            }

                            ptr += 4;
                        }
                        ptr += destData.Stride - destData.Width * 4;
                    }
                }
            }
            catch { }
            finally
            {
                transformer.Release();
                if (destData != null)
                    dest.UnlockBits(destData);
                if (sourceData != null)
                    source.UnlockBits(sourceData);
            }
        }
	}

}
