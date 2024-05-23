using gView.Framework.Core.Geometry;
using gView.Framework.Data;
using System.Threading.Tasks;

namespace gView.DataSources.Shape
{
    internal class CreateSpatialIndexTree
    {
        private SHPFile _file;
        private DualTree _tree;
        private IEnvelope _bounds;

        public CreateSpatialIndexTree(SHPFile file, DualTree tree, IEnvelope bounds)
        {
            _file = file;
            _tree = tree;
            _bounds = bounds;
        }

        public Task Create()
        {
            _tree.CreateTree(_bounds);

            for (uint i = 0; i < _file.Entities; i++)
            {
                IEnvelope env = _file.ReadEnvelope(i);
                //if (env == null)
                //    continue;
                SHPObject obj = new SHPObject((int)i, env);
                //bool inserted=_tree.SHPTreeAddShapeId(obj);
                bool inserted = _tree.AddShape(obj);
                inserted = false;
            }
            _tree.FinishIt();

            _tree.writeIDXIndex(_file.IDX_Filename);

            return Task.CompletedTask;
        }
    }
}
