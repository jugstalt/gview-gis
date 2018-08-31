using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gView.Framework.UI;
using gView.Framework.Carto;

namespace gView.Framework.Data.Relations
{
    public class TableRelation : ITableRelation 
    {
        private IMapDocument _mapDocument;

        public TableRelation(IMapDocument mapDocument)
        {
            _mapDocument = mapDocument;
            this.LogicalOperator = "=";
        }

        #region ITableRelation Member

        public string RelationName
        {
            get;
            set;
        }

        public IDatasetElement LeftTable
        {
            get;
            set;
        }

        public IDatasetElement RightTable
        {
            get;
            set;
        }

        public string LeftTableField
        {
            get;
            set;
        }

        public string RightTableField
        {
            get;
            set;
        }

        public string LogicalOperator
        {
            get;
            set;
        }

        public ICursor GetLeftRows(string leftFields, object rightValue)
        {
            try
            {
                IQueryFilter filter = GetLeftFilter(leftFields, rightValue);

                return ((ITableClass)this.LeftTable.Class).Search(filter);
            }
            catch
            {
                return null;
            }
        }

        public IQueryFilter GetLeftFilter(string leftFields, object rightValue)
        {
            QueryFilter filter = new QueryFilter();
            filter.SubFields = leftFields;

            IField field = ((ITableClass)this.LeftTable.Class).FindField(this.LeftTableField);
            filter.WhereClause = Field.WhereClauseFieldName(this.LeftTableField) + " " + InverseOperator(this.LogicalOperator) + " " + ValueString(field, rightValue);

            return filter;
        }

        public ICursor GetRightRows(string rightFields, object leftValue)
        {
            try
            {
                IQueryFilter filter = GetRightFilter(rightFields, leftValue);

                return ((ITableClass)this.RightTable.Class).Search(filter);
            }
            catch
            {
                return null;
            }
        }

        public IQueryFilter GetRightFilter(string rightFields, object leftValue)
        {
            QueryFilter filter = new QueryFilter();
            filter.SubFields = rightFields;

            IField field = ((ITableClass)this.RightTable.Class).FindField(this.RightTableField);
            filter.WhereClause = Field.WhereClauseFieldName(this.RightTableField) + " " + this.LogicalOperator + " " + ValueString(field, leftValue);

            return filter;
        }

        #endregion

        #region IPersistable Member

        public void Load(IO.IPersistStream stream)
        {
            this.RelationName = (string)stream.Load("RelationName");

            this.LeftTable = DatasetElementById((string)stream.Load("LeftMap"), (int)stream.Load("LeftTableId",(int)-1));
            this.LeftTableField = (string)stream.Load("LeftTableField");

            this.RightTable = DatasetElementById((string)stream.Load("RightMap"), (int)stream.Load("RightTableId", (int)-1));
            this.RightTableField = (string)stream.Load("RightTableField");

            this.LogicalOperator = (string)stream.Load("LogicalOperator", "=");
        }

        public void Save(IO.IPersistStream stream)
        {
            stream.Save("RelationName", this.RelationName);
            
            IMap leftMap = MapByDatasetElement(this.LeftTable);
            if (leftMap != null)
            {
                stream.Save("LeftMap", leftMap.Name);
                stream.Save("LeftTableId", this.LeftTable.ID);
                stream.Save("LeftTableField", this.LeftTableField);
            }
            IMap rightMap = MapByDatasetElement(this.RightTable);
            if (rightMap != null)
            {
                stream.Save("RightMap", rightMap.Name);
                stream.Save("RightTableId", this.RightTable.ID);
                stream.Save("RightTableField", this.RightTableField);
            }

            stream.Save("LogicalOperator", this.LogicalOperator);
        }
        
        #endregion

        #region Helper

        private IMap MapByDatasetElement(IDatasetElement element)
        {
            foreach (IMap map in _mapDocument.Maps)
            {
                if (map[element] != null)
                    return map;
            }
            return null;
        }

        private IDatasetElement DatasetElementById(string mapName, int id)
        {
            foreach (IMap map in _mapDocument.Maps)
            {
                if (map.Name == mapName)
                {
                    foreach (IDatasetElement element in map.MapElements)
                    {
                        if (element.ID == id)
                            return element;
                    }
                }
            }
            return null;
        }

        private string ObjectString(object obj)
        {
            if (obj is double)
                return ((double)obj).ToString(gView.Framework.system.Numbers.Nhi);
            if(obj is float)
                return ((float)obj).ToString(gView.Framework.system.Numbers.Nhi);

            if (obj == null)
                return "null";

            return obj.ToString();
        }

        private string ValueString(IField field, object value)
        {
            if (value == null) return "null";

            switch (field.type)
            {
                case FieldType.biginteger:
                case FieldType.Double:
                case FieldType.Float:
                case FieldType.ID:
                case FieldType.smallinteger:
                case FieldType.integer:
                    return ObjectString(value);
                default:
                    return "'" + ObjectString(value) + "'";
            }
        }

        private string InverseOperator(string logicalOperator)
        {
            switch (logicalOperator)
            {
                case "<":
                    return ">";
                case ">":
                    return "<";
                case "<=":
                    return ">=";
                case ">=":
                    return "<=";
            }

            return logicalOperator;
        }

        #endregion
    }
}
