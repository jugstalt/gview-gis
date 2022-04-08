using System.Collections.Generic;

//[assembly: InternalsVisibleTo("gView.OGC, PublicKey=0024000004800000940000000602000000240000525341310004000001000100916d0be3f662c2d3589fbe93479f3215e23fd195db9a20e77f42dc1d2942bd48cad3ea36b797f57880e6c31af0c238d2e445898c8ecce990aacbb70ae05a10aff73ab65c7db86366697f934b780238ed8fd1b2e28ba679a97e060b53fce66118e129b91d24f392d4dd3d482fa4173e61f18c74cda9f35721a97e77afbbc96dd2")]


namespace gView.Framework.Geometry
{
    public class Hole : Ring, IHole
    {
        public Hole()
            : base()
        {
        }
        public Hole(List<IPoint> points)
            : base(points)
        {
        }

        internal Hole(IRing ring)
            : base(ring)
        {
        }

        public override object Clone()
        {
            return new Hole(m_points);
        }
    }
}
