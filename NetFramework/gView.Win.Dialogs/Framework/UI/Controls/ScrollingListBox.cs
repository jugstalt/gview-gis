using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace gView.Framework.UI.Controls
{
    public partial class ScrollingListBox : System.Windows.Forms.ListBox
    {
        public ScrollingListBox()
        {
            InitializeComponent();

            this.CalcFontScaleFactor();
        }

        public float FontScaleFactor { get; set; }

        public int HorizontalScrollPos
        {
            get
            {
                if (_hWnd != (IntPtr)0)
                {
                    _hsi.fMask = SIF_ALL;
                    _hsi.cbSize = Marshal.SizeOf(_hsi);
                    GetScrollInfo(_hWnd, SB_HORZ, ref _hsi);
                }
                return _hsi.nPos;
            }
            set
            {
                _hsi.nPos = value;
                _hsi.fMask = SIF_POS;

                if (_hWnd != (IntPtr)0)
                {
                    SetScrollInfo(_hWnd, SB_HORZ, ref _hsi, true);   
                }
            }
        }
        public int VerticalScrollPos
        {
            get
            {
                if (_hWnd != (IntPtr)0)
                {
                    _vsi.fMask = SIF_ALL;
                    _vsi.cbSize = Marshal.SizeOf(_hsi);
                    GetScrollInfo(_hWnd, SB_VERT, ref _vsi);
                }
                return _vsi.nPos;
            }
            set
            {
                _vsi.nPos = value;
                _vsi.fMask = SIF_POS;

                if (_hWnd != (IntPtr)0)
                {
                    SetScrollInfo(_hWnd, SB_VERT, ref _vsi, true); 
                }
            }
        }

        public void CalcFontScaleFactor()
        {
            try
            {
                using (Graphics graphics = this.CreateGraphics())
                {
                    this.FontScaleFactor = graphics.DpiX / 96f;
                }

                this.ItemHeight = (int)(this.ItemHeight * this.FontScaleFactor);
            }
            catch { this.FontScaleFactor = 1f; }
        }


        #region Scrolling
        
        ScrollInfoStruct _vsi=new ScrollInfoStruct();
        ScrollInfoStruct _hsi = new ScrollInfoStruct();
        IntPtr _hWnd = (IntPtr)0;
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == WM_HSCROLL)
            {
                if (msg.WParam.ToInt32() == SB_ENDSCROLL)
                {
                    _hsi.fMask = SIF_ALL;
                    _hsi.cbSize = Marshal.SizeOf(_hsi);
                    GetScrollInfo(_hWnd = msg.HWnd, SB_HORZ, ref _hsi);
                }
            }
            else if (msg.Msg == WM_VSCROLL)
            {
                if (msg.WParam.ToInt32() == SB_ENDSCROLL)
                {
                    _vsi.fMask = SIF_ALL;
                    _vsi.cbSize = Marshal.SizeOf(_vsi);
                    GetScrollInfo(_hWnd = msg.HWnd, SB_VERT, ref _vsi);
                }
            }

            try
            {
                base.WndProc(ref msg);
            }
            catch(Exception ex) 
            { 
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetScrollInfo(
            IntPtr hWnd, int n, ref ScrollInfoStruct lpScrollInfo);

        [DllImport("user32.dll")]
        static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref ScrollInfoStruct
            lpScrollInfo, bool fRedraw);

        [DllImport("user32")]
        static extern bool InvalidateRect(IntPtr hwnd, IntPtr lpRect, bool bErase);

        private const int WM_HSCROLL = 0x0114;
        private const int WM_VSCROLL = 0x0115;

        private const int SB_LINELEFT = 0;
        private const int SB_LINERIGHT = 1;
        private const int SB_PAGELEFT = 2;
        private const int SB_PAGERIGHT = 3;
        private const int SB_THUMBPOSITION = 4;
        private const int SB_THUMBTRACK = 5;
        private const int SB_LEFT = 6;
        private const int SB_RIGHT = 7;
        private const int SB_ENDSCROLL = 8;

        private const int SB_HORZ = 0;
        private const int SB_VERT = 1;
        private const int SB_CTL = 2;
        private const int SB_BOTH = 3;

        private const int SIF_TRACKPOS = 0x10;
        private const int SIF_RANGE = 0x1;
        private const int SIF_POS = 0x4;
        private const int SIF_PAGE = 0x2;
        private const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

        private struct ScrollInfoStruct
        {
            public int cbSize;
            public int fMask;
            public int nMin;
            public int nMax;
            public int nPage;
            public int nPos;
            public int nTrackPos;
        }
        #endregion
    }
}
