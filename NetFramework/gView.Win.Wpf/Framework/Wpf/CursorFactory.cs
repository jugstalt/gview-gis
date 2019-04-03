using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace gView.Desktop.Wpf
{
    static public class CursorFactory
    {
        public static Cursor ToWpfCursor(object cursor)
        {
            if (cursor is Cursor)
                return (Cursor)cursor;

            if (cursor == System.Windows.Forms.Cursors.AppStarting)
                return Cursors.AppStarting;
            if (cursor == System.Windows.Forms.Cursors.Arrow)
                return Cursors.Arrow;
            if (cursor == System.Windows.Forms.Cursors.Cross)
                return Cursors.Cross;
            if (cursor == System.Windows.Forms.Cursors.Default)
                return Cursors.Arrow;
            if (cursor == System.Windows.Forms.Cursors.Hand)
                return Cursors.Hand;
            if (cursor == System.Windows.Forms.Cursors.Help)
                return Cursors.Help;
            if (cursor == System.Windows.Forms.Cursors.IBeam)
                return Cursors.IBeam;
            if (cursor == System.Windows.Forms.Cursors.No)
                return Cursors.No;

            if (cursor == System.Windows.Forms.Cursors.SizeAll)
                return Cursors.SizeAll;
            if (cursor == System.Windows.Forms.Cursors.SizeNESW)
                return Cursors.SizeNESW;
            if (cursor == System.Windows.Forms.Cursors.SizeNS)
                return Cursors.SizeNS;
            if (cursor == System.Windows.Forms.Cursors.SizeNWSE)
                return Cursors.SizeNWSE;
            if (cursor == System.Windows.Forms.Cursors.SizeWE)
                return Cursors.SizeWE;
            if (cursor == System.Windows.Forms.Cursors.UpArrow)
                return Cursors.UpArrow;

            if (cursor == System.Windows.Forms.Cursors.WaitCursor)
                return Cursors.Wait;

            return Cursors.Arrow;
        }

        public static System.Windows.Forms.Cursor ToFormsCursor(object cursor)
        {
            if (cursor is System.Windows.Forms.Cursor)
                return (System.Windows.Forms.Cursor)cursor;

            if (cursor == Cursors.AppStarting)
                return System.Windows.Forms.Cursors.AppStarting;
            if (cursor == Cursors.Arrow)
                return System.Windows.Forms.Cursors.Arrow;
            if (cursor == Cursors.Cross)
                return System.Windows.Forms.Cursors.Cross;
            if (cursor == Cursors.Arrow)
                return System.Windows.Forms.Cursors.Default;
            if (cursor == Cursors.Hand)
                return System.Windows.Forms.Cursors.Hand;
            if (cursor == Cursors.Help)
                return System.Windows.Forms.Cursors.Help;
            if (cursor == Cursors.IBeam)
                return System.Windows.Forms.Cursors.IBeam;
            if (cursor == Cursors.No)
                return System.Windows.Forms.Cursors.No;

            if (cursor == Cursors.SizeAll)
                return System.Windows.Forms.Cursors.SizeAll;
            if (cursor == Cursors.SizeNESW)
                return System.Windows.Forms.Cursors.SizeNESW;
            if (cursor == Cursors.SizeNS)
                return System.Windows.Forms.Cursors.SizeNS;
            if (cursor == Cursors.SizeNWSE)
                return System.Windows.Forms.Cursors.SizeNWSE;
            if (cursor == Cursors.SizeWE)
                return System.Windows.Forms.Cursors.SizeWE;
            if (cursor == Cursors.UpArrow)
                return System.Windows.Forms.Cursors.UpArrow;

            if (cursor == Cursors.Wait)
                return System.Windows.Forms.Cursors.WaitCursor;

            return System.Windows.Forms.Cursors.Default;
        }
    }
}
