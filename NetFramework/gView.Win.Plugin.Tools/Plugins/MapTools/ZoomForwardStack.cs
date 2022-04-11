using gView.Framework.Geometry;
using System.Collections;

namespace gView.Plugins.MapTools
{
    internal class ZoomForwardStack
    {
        static System.Collections.Stack stack = new Stack(20);

        static public void Push(IEnvelope envelope)
        {
            if (envelope == null)
            {
                return;
            }

            stack.Push(envelope);
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
