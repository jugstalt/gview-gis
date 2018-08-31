using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using gView.Framework;
using gView.Framework.Geometry;
using gView.Framework.Carto.Rendering;

namespace gView.Objects
{
	/// <summary>
	/// Zusammenfassung für SelectionRenderer.
	/// </summary>
	/*
	public class SelectionRenderer : IImageRenderer  
	{
		private IEnvelope m_envelope;
		private int iWidth,iHeight;
		double wWidth,wHeight;

		public SelectionRenderer()
		{
		}

		public void World2Image(ref double x,ref double y)
		{
			x=(x-m_envelope.minx)*iWidth/wWidth;
			y=iHeight-(y-m_envelope.miny)*iHeight/wHeight;
		}

		#region IImageRenderer
		public IEnvelope Envelope 
		{
			set 
			{
				m_envelope=value;
				if(value==null) return;
				wWidth =m_envelope.maxx-m_envelope.minx;
				wHeight=m_envelope.maxy-m_envelope.miny;
			}
		}
		public void Render(System.Drawing.Image image,IMap map) 
		{
			
		}
		#endregion

		private void DrawPolyline(System.Drawing.Graphics gr,Pen pen,IPolyline polyline) 
		{
			GraphicsPath gp=new GraphicsPath();
			int o_x=0,o_y=0;

			try 
			{
				for(int i=0;i<polyline.PathCount;i++) 
				{
					IPath path=polyline[i];
					bool first=true;

					gp.StartFigure();

					for(int p=0;p<path.PointCount;p++) 
					{
						IPoint point=(IPoint)path[p];
						double x=point.X,y=point.Y;
					
						World2Image(ref x,ref y);

						if(!first)
							gp.AddLine(o_x,o_y,(int)x,(int)y);
						else
							first=false;
						o_x=(int)x;
						o_y=(int)y;
					}
					//gp.CloseFigure();
					gr.DrawPath(pen,gp);
				}

				gp.Dispose(); gp=null;
			} 
			catch 
			{
				if(gp!=null) gp.Dispose();
				gp=null;
			}
		}
		private void DrawPolygon(System.Drawing.Graphics gr,Pen pen,IPolygon polygon) 
		{
			GraphicsPath gp=new GraphicsPath();
			int o_x=0,o_y=0;

			try 
			{
				for(int i=0;i<polygon.RingCount;i++) 
				{
					IRing ring=polygon[i];
					bool first=true;

					gp.StartFigure();

					for(int p=0;p<ring.PointCount;p++) 
					{
						IPoint point=(IPoint)ring[p];
						double x=point.X,y=point.Y;
					
						World2Image(ref x,ref y);

						if(!first)
							gp.AddLine(o_x,o_y,(int)x,(int)y);
						else
							first=false;
						o_x=(int)x;
						o_y=(int)y;
					}
					gp.CloseFigure();
					gr.DrawPath(pen,gp);
				}

				gp.Dispose(); gp=null;
			} 
			catch 
			{
				if(gp!=null) gp.Dispose();
				gp=null;
			}
		}
	}
	*/
}
