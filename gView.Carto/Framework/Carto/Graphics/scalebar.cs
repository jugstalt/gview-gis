using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace gView.Framework.Carto.Graphics
{
	/// <summary>
	/// Zusammenfassung für scalebar.
	/// </summary>
	internal class scalebar
	{
		double m_scale,m_dpi;
		public scalebar(double scale,double dpi)
		{
			m_scale=scale;
			m_dpi  =dpi;
		}
		public int ScaleBarWidth 
		{
			get 
			{
				double dpm=m_dpi/0.0256;
				double pix=m_scale/dpm;
				double bl=pix*(200*(m_dpi/96))/5.0;

				if(bl>1000000)		bl=Math.Round((int)(bl/100000)*100000.0,0);
				else if(bl>100000)	bl=Math.Round((int)(bl/10000)*10000.0,0);
				else if(bl>10000)	bl=Math.Round((int)(bl/5000)*5000.0,0);
				else if(bl>1000)	bl=Math.Round((int)(bl/500)*500.0,0);
				else if(bl>100)		bl=Math.Round((int)(bl/100)*100.0,0);
				else if(bl>10)		bl=Math.Round((int)(bl/10)*10.0,0);
				
				int bm_bl=(int)(bl/pix);
				int dist=(int)Math.Round(bl*5,0);
				return bm_bl*5;
			}
		}

		public bool Create(ref Bitmap bm,int a,int y)
		{
			double dpm=m_dpi/0.0256;
			double pix=m_scale/dpm;
			double bl=pix*(200*(m_dpi/96))/5.0;
			float fac=(float)m_dpi/96;

			if(bl>1000000)		bl=Math.Round((int)(bl/100000)*100000.0,0);
			else if(bl>100000)	bl=Math.Round((int)(bl/10000)*10000.0,0);
			else if(bl>10000)	bl=Math.Round((int)(bl/5000)*5000.0,0);
			else if(bl>1000)	bl=Math.Round((int)(bl/500)*500.0,0);
			else if(bl>100)		bl=Math.Round((int)(bl/100)*100.0,0);
			else if(bl>10)		bl=Math.Round((int)(bl/10)*10.0,0);

			int bm_bl=(int)(bl/pix);
			Font font=new Font("Verdana",7*fac,System.Drawing.FontStyle.Bold);
			int dist=(int)Math.Round(bl*5,0);
			
			System.Drawing.Graphics gr=System.Drawing.Graphics.FromImage(bm);
			// Hintergrund und Rahmen zeichnen
			SolidBrush brush=new SolidBrush(Color.FromArgb(155,149,149));
			
			gr.FillRectangle(brush,a+0,	     15*fac+y,bm_bl,5*fac);
			gr.FillRectangle(brush,a+2*bm_bl,15*fac+y,bm_bl,5*fac);
			gr.FillRectangle(brush,a+4*bm_bl,15*fac+y,bm_bl,5*fac);
			brush.Color=Color.FromArgb(255,255,255);
			gr.FillRectangle(brush,a+1*bm_bl,15*fac+y,bm_bl,5*fac);
			gr.FillRectangle(brush,a+3*bm_bl,15*fac+y,bm_bl,5*fac);

			Pen pen=new Pen(Color.FromArgb(0,0,0));
			gr.DrawRectangle(pen,a+0,14*fac+y,bm_bl*5-1,5*fac);
			gr.DrawLine(pen,a,		 12*fac+y,a,		 19*fac+y);
			gr.DrawLine(pen,a+bm_bl*5-1,12*fac+y,a+bm_bl*5-1,19*fac+y);
			gr.DrawLine(pen,a+bm_bl,  14*fac+y,a+bm_bl,  19*fac+y);
			gr.DrawLine(pen,a+bm_bl*2,14*fac+y,a+bm_bl*2,19*fac+y);
			gr.DrawLine(pen,a+bm_bl*3,14*fac+y,a+bm_bl*3,19*fac+y);
			gr.DrawLine(pen,a+bm_bl*4,14*fac+y,a+bm_bl*4,19*fac+y);
			
			string text=Math.Round(m_scale,0).ToString()  ,t="";
			int counter=1;
			// Tausenderpunkte
			for(int i=text.Length-1;i>0;i--)
			{
				t=text[i]+t;
				if((counter++ % 3)==0) t="."+t;
			}
			t=text[0]+t;
			text="M 1:"+t;
			brush.Color=Color.FromArgb(0,0,0);
			drawString(gr,brush,font,text,(float)(a+(bm_bl*5-gr.MeasureString(text,font).Width)/2),(float)y);
			drawString(gr,brush,font,"0",(float)(a-4),(float)y);
			
			if(dist>1000) 
			{
				float x=(float)dist/(float)1000;
				text=x.ToString()+" km";
			} 
			else 
			{
				text=dist.ToString()+" m";
			}
			drawString(gr,brush,font,text,(float)(a+bm_bl*5-5*fac),(float)y);
			gr.Dispose();
			gr=null;
			font.Dispose();
			font=null;
			return true;
		}
		public Bitmap Create()
		{
			double dpm=m_dpi/0.0256;
			double pix=m_scale/dpm;
			double bl=pix*200/5.0;

			if(bl>1000000)		bl=Math.Round((int)(bl/100000)*100000.0,0);
			else if(bl>100000)	bl=Math.Round((int)(bl/10000)*10000.0,0);
			else if(bl>10000)	bl=Math.Round((int)(bl/5000)*5000.0,0);
			else if(bl>1000)	bl=Math.Round((int)(bl/500)*500.0,0);
			else if(bl>100)		bl=Math.Round((int)(bl/100)*100.0,0);
			else if(bl>10)		bl=Math.Round((int)(bl/10)*10.0,0);

			int bm_bl=(int)(bl/pix);
			Font font=new Font("Verdana",7,System.Drawing.FontStyle.Bold);
			int a=4,dist=(int)Math.Round(bl*5,0);
			Bitmap bm = new Bitmap(bm_bl*5+a+50,34);
			System.Drawing.Graphics gr=System.Drawing.Graphics.FromImage(bm);
			// Hintergrund und Rahmen zeichnen
			SolidBrush brush=new SolidBrush(Color.FromArgb(155,149,149));
			
			gr.FillRectangle(brush,a+0,	     15,bm_bl,5);
			gr.FillRectangle(brush,a+2*bm_bl,15,bm_bl,5);
			gr.FillRectangle(brush,a+4*bm_bl,15,bm_bl,5);

			Pen pen=new Pen(Color.FromArgb(0,0,0));
			gr.DrawRectangle(pen,a+0,14,bm_bl*5-1,5);
			gr.DrawLine(pen,a,			12,a,		   19);
			gr.DrawLine(pen,a+bm_bl*5-1,12,a+bm_bl*5-1,19);
			gr.DrawLine(pen,a+bm_bl,  14,a+bm_bl,  19);
			gr.DrawLine(pen,a+bm_bl*2,14,a+bm_bl*2,19);
			gr.DrawLine(pen,a+bm_bl*3,14,a+bm_bl*3,19);
			gr.DrawLine(pen,a+bm_bl*4,14,a+bm_bl*4,19);
			
			string text=Math.Round(m_scale,0).ToString()  ,t="";
			int counter=1;
			// Tausenderpunkte
			for(int i=text.Length-1;i>0;i--)
			{
				t=text[i]+t;
				if((counter++ % 3)==0) t="."+t;
			}
			t=text[0]+t;
			text="M 1:"+t;
			brush.Color=Color.FromArgb(0,0,0);
			drawString(gr,brush,font,text,(float)(bm_bl*5+a-gr.MeasureString(text,font).Width)/2,(float)0);
			drawString(gr,brush,font,"0",(float)0,(float)0);
			
			if(dist>1000) 
			{
				float x=dist/1000;
				text=x.ToString()+" km";
			} 
			else 
			{
				text=dist.ToString()+" m";
			}
			drawString(gr,brush,font,text,(float)bm_bl*5-1,(float)0);
			gr.Dispose();
			gr=null;
			font.Dispose();
			font=null;
			return bm;
		}

		private void drawString(System.Drawing.Graphics gr,SolidBrush brush,
			Font font,string text,float x,float y) 
		{
			brush.Color=Color.White;
			gr.DrawString(text,font,brush,x-1,y);
			gr.DrawString(text,font,brush,x+1,y);
			gr.DrawString(text,font,brush,x,  y+1);
			gr.DrawString(text,font,brush,x,  y-1);

			brush.Color=Color.Black;
			gr.DrawString(text,font,brush,x,y);

		}
	}
}
