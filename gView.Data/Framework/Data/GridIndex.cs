using System;
using System.Collections.Generic;
using System.Text;
using gView.Framework.Geometry;

namespace gView.Framework.Data
{
    public class GridIndex<T> where T : class
    {
        private IEnvelope _bounds;
        private int _cellsX, _cellsY;
        private double _cX, _cY;
        private Dictionary<int, T> _gridCellContainer = new Dictionary<int, T>();

        public GridIndex(IEnvelope bounds, int cells)
            : this(bounds, cells, cells)
        {
        }
        public GridIndex(IEnvelope bounds, int cellsX, int cellsY)
        {
            if (bounds == null || cellsX <= 0 || cellsY <= 0)
                throw new ArgumentException();

            _bounds = new Envelope(bounds);
            ((Envelope)_bounds).Raise(120);

            _cellsX = cellsX;
            _cellsY = cellsY;

            _cX = _bounds.Width / cellsX;
            _cY = _bounds.Height / cellsY;
        }

        #region Members
        public int XIndex(IPoint p)
        {
            if (p == null)
                return -1;

            int i = (int)Math.Floor((p.X - _bounds.minx) / _cX);
            if (i < 0 || i >= _cellsX)
                return -1;
            return i;
        }
        public int YIndex(IPoint p)
        {
            if (p == null)
                return -1;

            int i = (int)Math.Floor((p.Y - _bounds.miny) / _cY);
            if (i < 0 || i >= _cellsY)
                return -1;
            return i;
        }
        public int XYIndex(IPoint p)
        {
            int ix = XIndex(p);
            int iy = YIndex(p);
            if (ix < 0 || iy < 0)
                return -1;

            return iy * _cellsX + ix;
        }

        public List<int> XYIndices(IEnvelope env)
        {
            List<int> ret = new List<int>();

            int i = XYIndex(new Point(env.minx, env.miny));
            ret.Add(i);

            i = XYIndex(new Point(env.minx, env.maxy));
            if (!ret.Contains(i))
                ret.Add(i);
            i = XYIndex(new Point(env.maxx, env.maxy));
            if (!ret.Contains(i))
                ret.Add(i);
            i = XYIndex(new Point(env.maxx, env.miny));
            if (!ret.Contains(i))
                ret.Add(i);

            return ret;
        }
        public T this[int index]
        {
            get
            {
                if (_gridCellContainer.ContainsKey(index))
                    return _gridCellContainer[index];
                return null;
            }
            set
            {
                if (_gridCellContainer.ContainsKey(index))
                    _gridCellContainer[index] = value;
                else
                    _gridCellContainer.Add(index, value);
            }
        }

        public T[] AllCells
        {
            get
            {
                List<T> ret = new List<T>();
                foreach (int key in _gridCellContainer.Keys)
                {
                    T t = _gridCellContainer[key];
                    if (t == null)
                        continue;

                    ret.Add(t);
                }
                return ret.ToArray();
            }
        }
        #endregion
    }

    public class GridArray<T> where T : class
    {
        private List<GridIndex<T>> _grids = null;
        private T _outside;

        public GridArray(IEnvelope bounds, int[] cellsX, int[] cellsY)
        {
            if (cellsX != null && cellsY != null && cellsX.Length == cellsY.Length)
            {
                _grids = new List<GridIndex<T>>();

                for (int i = 0; i < cellsX.Length; i++)
                {
                    _grids.Add(new GridIndex<T>(bounds, cellsX[i], cellsY[i]));
                }
                _outside = Activator.CreateInstance(typeof(T)) as T;
            }
        }

        #region Members
        public T this[IEnvelope env]
        {
            get
            {
                foreach (GridIndex<T> grid in _grids)
                {
                    List<int> gridIndices = grid.XYIndices(env);
                    if (gridIndices.Count == 1)
                    {
                        T ret = grid[gridIndices[0]];
                        if (ret == null)
                        {
                            ret = Activator.CreateInstance(typeof(T)) as T;
                            grid[gridIndices[0]] = ret;
                        }

                        return ret;
                    }
                }
                return _outside;
            }
        }

        public List<T> Collect(IEnvelope env)
        {
            List<T> list = new List<T>();
            list.Add(_outside);

            foreach (GridIndex<T> grid in _grids)
            {
                foreach (int index in grid.XYIndices(env))
                {
                    T t= grid[index];
                    if (t != null)
                        list.Add(t);
                }
            }
            return list;
        }
        #endregion
    }
}
