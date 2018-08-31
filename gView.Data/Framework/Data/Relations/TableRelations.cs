using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.IO;
using gView.Framework.UI;

namespace gView.Framework.Data.Relations
{
    public class TableRelations : List<ITableRelation>, ITableRelations
    {
        private IMapDocument _mapDocument;

        public TableRelations(IMapDocument mapDocument)
        {
            _mapDocument = mapDocument;
        }

        #region Members

        public ITableRelation this[string relationName]
        {
            get
            {
                foreach (ITableRelation relation in this)
                {
                    if (relation.RelationName == relationName)
                        return relation;
                }
                return null;
            }
        }

        public IEnumerable<ITableRelation> GetRelations(IDatasetElement element)
        {
            List<ITableRelation> relations=new List<ITableRelation>();
            foreach (ITableRelation tableRelation in this)
            {
                if (tableRelation.LeftTable == element || tableRelation.RightTable == element)
                    relations.Add(tableRelation);
            }
            return relations;
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            ITableRelation relation;
            while ((relation = stream.Load("Relation", null, new TableRelation(_mapDocument)) as ITableRelation) != null)
            {
                this.Add(relation);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (ITableRelation relation in this)
            {
                stream.Save("Relation", relation);
            }
        }

        #endregion
    }
}
