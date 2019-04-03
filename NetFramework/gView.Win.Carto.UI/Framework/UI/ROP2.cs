//
// RubberbandRects.cs - A class to generate a rubberband rectangle in a .NET
// application through calls to the Win32 GDI API.
// created on 9/5/2003 at 4:46 PM by cthomas
// 

using System;
using System.Drawing;

namespace gView.Framework.UI
{
	internal enum PenStyles
	{
		PS_SOLID		= 0,
		PS_DASH			= 1,
		PS_DOT			= 2,
		PS_DASHDOT		= 3,
		PS_DASHDOTDOT	= 4
	}
	internal class ROP2
	{
		// These values come from the larger set of defines in WinGDI.h,
		// but are all that are needed for this application.  If this class
		// is expanded for more generic rectangle drawing, they should be
		// replaced by enums from those sets of defones.
		private int NULL_BRUSH = 5;
		private int R2_XORPEN = 7;
		private PenStyles penStyle;
		private int Col = 0, width=1;

		// Default contructor - sets member fields
        public ROP2()
		{
			penStyle = PenStyles.PS_DOT;
		}
		
		// penStyles property get/set.
		public PenStyles PenStyle
		{
			get { return penStyle; }
			set { penStyle = value; }
		}

        public Color Color
        {
            set
            {
                Col = RGB(value.R, value.G, value.B);
            }
        }
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
		public void DrawXORRectangle( System.Drawing.Graphics grp,
		                              int X1, int Y1, int X2, int Y2 )
		{
			// Extract the Win32 HDC from the Graphics object supplied.
			IntPtr hdc = grp.GetHdc();
			
			// Create a pen with a dotted style to draw the border of the
			// rectangle.
			IntPtr gdiPen = CreatePen( penStyle,
			              width, Col );
			
			// Set the ROP cdrawint mode to XOR.
			SetROP2( hdc, R2_XORPEN );
			
			// Select the pen into the device context.
			IntPtr oldPen = SelectObject( hdc, gdiPen );
			
			// Create a stock NULL_BRUSH brush and select it into the device
			// context so that the rectangle isn't filled.
			IntPtr oldBrush = SelectObject( hdc,
			                     GetStockObject( NULL_BRUSH ) );
			
			// Now XOR the hollow rectangle on the Graphics object with
			// a dotted outline.
			Rectangle( hdc, X1, Y1, X2, Y2 );
			
			// Put the old stuff back where it was.
			SelectObject( hdc, oldBrush ); // no need to delete a stock object
			SelectObject( hdc, oldPen );
			DeleteObject( gdiPen );		// but we do need to delete the pen
			
			// Return the device context to Windows.
			grp.ReleaseHdc( hdc );
		}

		// Use Interop to call the corresponding Win32 GDI functions
		[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
		private static extern int SetROP2(
		        IntPtr hdc,		// Handle to a Win32 device context
		        int enDrawMode	// Drawing mode
		        );
		[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
		private static extern IntPtr CreatePen(
		        PenStyles enPenStyle,	// Pen style from enum PenStyles
		        int nWidth,				// Width of pen
		        int crColor				// Color of pen
		        );
		[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
		private static extern bool DeleteObject(
		        IntPtr hObject	// Win32 GDI handle to object to delete
		        );
		[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
		private static extern IntPtr SelectObject(
		        IntPtr hdc,		// Win32 GDI device context
		        IntPtr hObject	// Win32 GDI handle to object to select
		        );
		[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
		private static extern void Rectangle(
		        IntPtr hdc,			// Handle to a Win32 device context
		        int X1,				// x-coordinate of top left corner
		        int Y1,				// y-cordinate of top left corner
		        int X2,				// x-coordinate of bottom right corner
		        int Y2				// y-coordinate of bottm right corner
		        );
		[System.Runtime.InteropServices.DllImportAttribute( "gdi32.dll" )]
		private static extern IntPtr GetStockObject( 
		        int brStyle	// Selected from the WinGDI.h BrushStyles enum
		        );


		// C# version of Win32 RGB macro
		private static int RGB( int R, int G, int B )
		{
			return ( R | (G<<8) | (B<<16) );
		}
	}
}
