using gView.Framework.Core.IO;
using gView.Framework.Core.Common;
using System.Collections.Generic;

namespace gView.Framework.Core.Data
{
    public class FeatureLayerJoins : List<IFeatureLayerJoin>, IPersistable, IClone
    {
        #region Members

        public IFeatureLayerJoin this[string joinName]
        {
            get
            {
                foreach (IFeatureLayerJoin join in this)
                {
                    if (join.JoinName == joinName)
                    {
                        return join;
                    }
                }
                return null;
            }
        }

        new public void Add(IFeatureLayerJoin join)
        {
            int counter = 1;
            string joinName = join.JoinName;
            while (this[join.JoinName] != null)
            {
                join.JoinName = joinName + "_" + counter++;
            }

            base.Add(join);
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            IFeatureLayerJoin join;
            while ((join = stream.Load("Join", null) as IFeatureLayerJoin) != null)
            {
                Add(join);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (IFeatureLayerJoin join in this)
            {
                stream.Save("Join", join);
            }
        }

        #endregion

        #region IClone Member

        public object Clone()
        {
            FeatureLayerJoins joins = new FeatureLayerJoins();

            foreach (IFeatureLayerJoin join in this)
            {
                joins.Add((IFeatureLayerJoin)join.Clone());
            }

            return joins;
        }

        #endregion
    }
}