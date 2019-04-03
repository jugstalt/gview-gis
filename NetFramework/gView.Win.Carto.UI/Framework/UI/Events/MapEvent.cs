using System;
using gView.Framework.Carto;

namespace gView.Framework.UI.Events
{
	/// <summary>
	/// 
	/// </summary>
	public class MapEvent
	{
		protected IMap m_map;
		public bool refreshMap=false;
		public DrawPhase drawPhase=DrawPhase.All;
        public object UserData = null;

		public MapEvent() {}
		public MapEvent(IMap map)
		{
			m_map=map;
		}

		public IMap Map { get { return m_map; } }
	}

	public class MapEventRubberband : MapEvent 
	{
		protected double m_minx,m_miny,m_maxx,m_maxy;

		public MapEventRubberband(IMap map,double minx,double miny,double maxx,double maxy)
		{
			m_map=map;
			m_minx=minx;
			m_miny=miny;
			m_maxx=maxx;
			m_maxy=maxy;
		}

		public double minX { get { return m_minx; } }
		public double minY { get { return m_miny; } }
		public double maxX { get { return m_maxx; } }
		public double maxY { get { return m_maxy; } }
	}

	public class MapEventPan : MapEvent 
	{
		protected double m_dx,m_dy;

		public MapEventPan(IMap map,double dx,double dy) 
		{
			m_map=map;
			m_dx=dx; 
			m_dy=dy;
		}

		public double dX { get { return m_dx; } }
		public double dY { get { return m_dy; } }
	}

	public class MapEventClick : MapEvent 
	{
		protected double m_x,m_y;

		public MapEventClick(IMap map,double x,double y) 
		{
			m_map=map;
			m_x=x; 
			m_y=y;
		}

		public double x { get { return m_x; } }
		public double y { get { return m_y; } }
	}
}
