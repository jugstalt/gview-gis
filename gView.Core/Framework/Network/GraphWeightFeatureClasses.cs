using System.Collections.Generic;

namespace gView.Framework.Network
{
    public class GraphWeightFeatureClasses : List<IGraphWeightFeatureClass>
    {
        public void Remove(int fcId)
        {
            GraphWeightFeatureClasses tmp = new GraphWeightFeatureClasses();
            foreach (IGraphWeightFeatureClass gwfc in this)
            {
                if (gwfc.FcId == fcId)
                {
                    tmp.Add(gwfc);
                }
            }

            foreach (IGraphWeightFeatureClass gwfc in tmp)
            {
                this.Remove(gwfc);
            }
        }

        public new IGraphWeightFeatureClass this[int fcId]
        {
            get
            {
                foreach (IGraphWeightFeatureClass gwfc in this)
                {
                    if (gwfc.FcId == fcId)
                    {
                        return gwfc;
                    }
                }

                return null;
            }
            set
            {
                if (this[fcId] != null)
                {
                    Remove(fcId);
                }

                base.Add(value);
            }
        }

        new public void Add(IGraphWeightFeatureClass gwfc)
        {
            if (gwfc == null)
            {
                return;
            }

            this[gwfc.FcId] = gwfc;
        }
    }
}
