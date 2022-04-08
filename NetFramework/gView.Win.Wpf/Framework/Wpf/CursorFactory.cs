using System.Windows.Input;

namespace gView.Desktop.Wpf
{
    static public class CursorFactory
    {
        public static Cursor ToWpfCursor(object cursor)
        {
            if (cursor is Cursor)
            {
                return (Cursor)cursor;
            }


            if (System.Windows.Forms.Cursors.AppStarting.Equals(cursor))
            {
                return Cursors.AppStarting;
            }

            if (System.Windows.Forms.Cursors.Arrow.Equals(cursor))
            {
                return Cursors.Arrow;
            }

            if (System.Windows.Forms.Cursors.Cross.Equals(cursor))
            {
                return Cursors.Cross;
            }

            if (System.Windows.Forms.Cursors.Default.Equals(cursor))
            {
                return Cursors.Arrow;
            }

            if (System.Windows.Forms.Cursors.Hand.Equals(cursor))
            {
                return Cursors.Hand;
            }

            if (System.Windows.Forms.Cursors.Help.Equals(cursor))
            {
                return Cursors.Help;
            }

            if (System.Windows.Forms.Cursors.IBeam.Equals(cursor))
            {
                return Cursors.IBeam;
            }

            if (System.Windows.Forms.Cursors.No.Equals(cursor))
            {
                return Cursors.No;
            }

            if (System.Windows.Forms.Cursors.SizeAll.Equals(cursor))
            {
                return Cursors.SizeAll;
            }

            if (System.Windows.Forms.Cursors.SizeNESW.Equals(cursor))
            {
                return Cursors.SizeNESW;
            }

            if (System.Windows.Forms.Cursors.SizeNS.Equals(cursor))
            {
                return Cursors.SizeNS;
            }

            if (System.Windows.Forms.Cursors.SizeNWSE.Equals(cursor))
            {
                return Cursors.SizeNWSE;
            }

            if (System.Windows.Forms.Cursors.SizeWE.Equals(cursor))
            {
                return Cursors.SizeWE;
            }

            if (System.Windows.Forms.Cursors.UpArrow.Equals(cursor))
            {
                return Cursors.UpArrow;
            }

            if (System.Windows.Forms.Cursors.WaitCursor.Equals(cursor))
            {
                return Cursors.Wait;
            }

            return Cursors.Arrow;
        }

        public static System.Windows.Forms.Cursor ToFormsCursor(object cursor)
        {
            if (cursor is System.Windows.Forms.Cursor)
            {
                return (System.Windows.Forms.Cursor)cursor;
            }

            if (cursor == Cursors.AppStarting)
            {
                return System.Windows.Forms.Cursors.AppStarting;
            }

            if (cursor == Cursors.Arrow)
            {
                return System.Windows.Forms.Cursors.Arrow;
            }

            if (cursor == Cursors.Cross)
            {
                return System.Windows.Forms.Cursors.Cross;
            }

            if (cursor == Cursors.Arrow)
            {
                return System.Windows.Forms.Cursors.Default;
            }

            if (cursor == Cursors.Hand)
            {
                return System.Windows.Forms.Cursors.Hand;
            }

            if (cursor == Cursors.Help)
            {
                return System.Windows.Forms.Cursors.Help;
            }

            if (cursor == Cursors.IBeam)
            {
                return System.Windows.Forms.Cursors.IBeam;
            }

            if (cursor == Cursors.No)
            {
                return System.Windows.Forms.Cursors.No;
            }

            if (cursor == Cursors.SizeAll)
            {
                return System.Windows.Forms.Cursors.SizeAll;
            }

            if (cursor == Cursors.SizeNESW)
            {
                return System.Windows.Forms.Cursors.SizeNESW;
            }

            if (cursor == Cursors.SizeNS)
            {
                return System.Windows.Forms.Cursors.SizeNS;
            }

            if (cursor == Cursors.SizeNWSE)
            {
                return System.Windows.Forms.Cursors.SizeNWSE;
            }

            if (cursor == Cursors.SizeWE)
            {
                return System.Windows.Forms.Cursors.SizeWE;
            }

            if (cursor == Cursors.UpArrow)
            {
                return System.Windows.Forms.Cursors.UpArrow;
            }

            if (cursor == Cursors.Wait)
            {
                return System.Windows.Forms.Cursors.WaitCursor;
            }

            return System.Windows.Forms.Cursors.Default;
        }
    }
}
