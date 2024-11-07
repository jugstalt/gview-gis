using gView.GraphicsEngine.Abstraction;
using System.Collections.Generic;
using System.Linq;

namespace gView.GraphicsEngine
{
    public class BrushCollection : IBrushCollection
    {
        public IBrush[] _brushes;

        public BrushCollection(IEnumerable<IBrush> brushes)
        {
            _brushes = brushes != null ? brushes.ToArray() : new IBrush[0];
        }

        public IEnumerable<IBrush> Brushes => _brushes;

        public void Dispose()
        {
            foreach (var brush in _brushes)
            {
                brush.Dispose();
            }
            _brushes = new IBrush[0];
        }
    }
}
