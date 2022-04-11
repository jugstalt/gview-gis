using gView.Framework.Geometry;
using System;
using System.Collections;

namespace gView.Plugins.MapTools
{
    internal class ZoomStack
    {
        static System.Collections.Stack stack = new Stack(20);

        static public void Push(IEnvelope envelope)
        {
            if (envelope == null)
            {
                return;
            }

            if (stack.Count > 0)
            {
                IEnvelope last = (IEnvelope)stack.Peek();
                //MessageBox.Show(last.minx.ToString() + "==" + envelope.minx.ToString() + "\n" + last.miny.ToString() + "==" + envelope.miny.ToString() + "\n" + last.maxx.ToString() + "==" + envelope.maxx.ToString() + "\n" + last.maxy.ToString() + "==" + envelope.maxy.ToString());
                if (Math.Abs(last.minx - envelope.minx) < 1e-10 &&
                    Math.Abs(last.miny - envelope.miny) < 1e-10 &&
                    Math.Abs(last.maxx - envelope.maxx) < 1e-10 &&
                    Math.Abs(last.maxy - envelope.maxy) < 1e-10)
                {
                    return;
                }
            }
            stack.Push(envelope);
            //MessageBox.Show(stack.Count.ToString());
        }
        static public IEnvelope Pop()
        {
            if (stack.Count == 0)
            {
                return null;
            }

            return (IEnvelope)stack.Pop();
        }

        static public int Count
        {
            get { return stack.Count; }
        }
    }
}
