using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Carto;
using gView.Framework.IO;
using gView.Framework.system;

namespace gView.Framework.Carto
{
    public class GraphicsContainer : IGraphicsContainer, IPersistable
    {
        public event EventHandler SelectionChanged = null;

        private GraphicElementList _elements = new GraphicElementList();
        private GraphicElementList _selectedelements = new GraphicElementList();
        private GrabberMode _editMode = GrabberMode.Pointer;

        public GraphicsContainer()
        {
            _selectedelements.SelectionChanged += new EventHandler(selectedelements_SelectionChanged);
        }

        void selectedelements_SelectionChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null) SelectionChanged(this, new EventArgs());
        }

        #region IGraphicsContainer Members

        public IGraphicElementList Elements
        {
            get { return _elements; }
        }

        public IGraphicElementList SelectedElements
        {
            get
            {
                return _selectedelements;
            }
        }

        public GrabberMode EditMode
        {
            get
            {
                return _editMode;
            }
            set
            {
                _editMode = value;
            }
        }

        #endregion

        #region IPersistable Member

        public void Load(IPersistStream stream)
        {
            _elements.Clear();
            _selectedelements.Clear();

            IGraphicElement grElement;
            while ((grElement = (IGraphicElement)stream.Load("GraphicElement", null)) != null)
            {
                _elements.Add(grElement);
            }
        }

        public void Save(IPersistStream stream)
        {
            foreach (IGraphicElement grElement in _elements)
            {
                if (PlugInManager.IsPlugin(grElement) &&
                    grElement is IPersistable)
                {
                    stream.Save("GraphicElement", grElement);
                }
            }
        }

        #endregion

        private class GraphicElementList : IGraphicElementList
        {
            public event EventHandler SelectionChanged = null;
            private List<IGraphicElement> _elements;

            public GraphicElementList()
            {
                _elements = new List<IGraphicElement>();
            }
            private GraphicElementList(List<IGraphicElement> elements)
            {
                _elements = elements;
            }

            #region IGraphicElementList Member

            public void Add(IGraphicElement element) 
            {
                _elements.Add(element);
                if (SelectionChanged != null) SelectionChanged(this, new EventArgs());
            }

            public void Clear()
            {
                _elements.Clear();
                if (SelectionChanged != null) SelectionChanged(this, new EventArgs());
            }

            public void Remove(IGraphicElement element)
            {
                _elements.Remove(element);
                if (SelectionChanged != null) SelectionChanged(this, new EventArgs());
            }

            public void Insert(int i, IGraphicElement element)
            {
                _elements.Insert(i, element);
                if (SelectionChanged != null) SelectionChanged(this, new EventArgs());

            }

            public bool Contains(IGraphicElement element)
            {
                return _elements.Contains(element);
            }

            public int Count
            {
                get { return _elements.Count; }
            }

            public IGraphicElement this[int i]
            {
                get { return _elements[i]; }
            }

            public IGraphicElementList Clone()
            {
                return new GraphicElementList(ListOperations<IGraphicElement>.Clone(_elements));
            }

            public IGraphicElementList Swap()
            {
                return new GraphicElementList(ListOperations<IGraphicElement>.Swap(_elements));
            }

            #endregion

            #region IEnumerable<IGraphicElement> Member

            public IEnumerator<IGraphicElement> GetEnumerator()
            {
                return _elements.GetEnumerator();
            }

            #endregion

            #region IEnumerable Member

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _elements.GetEnumerator();
            }

            #endregion
        }
    }
}
