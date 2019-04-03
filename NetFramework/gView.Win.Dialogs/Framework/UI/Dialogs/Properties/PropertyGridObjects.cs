using System;
using System.ComponentModel;
using gView.Framework;
using gView.Framework.Geometry;
using gView.Framework.system;

namespace gView.Framework.UI.Dialogs.Properties
{
	internal class SpatialReferenceProperties : IProperties 
	{
		private SpatialReference _sRef;

		public SpatialReferenceProperties(SpatialReference sRef) 
		{
			_sRef=sRef;
		}

		public override string ToString()
		{
			return "Spatial Reference Properties";
		}

		[Category("Spatial reference name")]
		[Browsable(true)]
		public string Name 
		{
			get { return _sRef.Name; }
			set 
			{
				if(_sRef is SpatialReference) 
				{
					((SpatialReference)_sRef).Name=value;
				}
			}
		}
		[Category("Spatial reference name")]
		[Browsable(true)]
		public string Description 
		{
			get { return _sRef.Description; }
			set 
			{
				if(_sRef is SpatialReference) 
				{
					((SpatialReference)_sRef).Description=value;
				}
			}
		}
		[Category("Spatial reference Parameters")]
		[Browsable(true)]
		public string [] Paramters 
		{
			get { return _sRef.Parameters; }
			set 
			{
				if(_sRef is SpatialReference) 
				{
					((SpatialReference)_sRef).Parameters=value;
				}
			}
		}
		#region IProperties Member

		[Browsable(false)]
		public object SelectedObject 
		{
			get { return _sRef; }
		}

		#endregion
	}

	internal class GeodeticDatumProperties : IProperties 
	{
		GeodeticDatum _datum;
		
		public GeodeticDatumProperties(GeodeticDatum datum) 
		{
			if(datum!=null) 
				_datum=datum;
			else 
				_datum=new GeodeticDatum();
		}

		[Category("1. Name")]
		[Browsable(true)]
		public string Name 
		{
			get { return _datum.Name; }
			set { _datum.Name=value; }
		}
		[Category("2. Translation")]
		[Browsable(true)]
		public double X_Axis 
		{
			get { return _datum.X_Axis; }
			set { _datum.X_Axis=value; }
		}
		[Category("2. Translation")]
		[Browsable(true)]
		public double Y_Axis 
		{
			get { return _datum.Y_Axis; }
			set { _datum.Y_Axis=value; }
		}
		[Category("2. Translation")]
		[Browsable(true)]
		public double Z_Axis 
		{
			get { return _datum.Z_Axis; }
			set { _datum.Z_Axis=value; }
		}

		[Category("3. Rotation")]
		[Browsable(true)]
		public double X_Rotation 
		{
			get { return _datum.X_Rotation; }
			set { _datum.X_Rotation=value; }
		}
		[Category("3. Rotation")]
		[Browsable(true)]
		public double Y_Rotation 
		{
			get { return _datum.Y_Rotation; }
			set { _datum.Y_Rotation=value; }
		}
		[Category("3. Rotation")]
		[Browsable(true)]
		public double Z_Rotation 
		{
			get { return _datum.Z_Rotation; }
			set { _datum.Z_Rotation=value; }
		}

		[Category("4. Scale Difference")]
		[Browsable(true)]
		public double Scale_Difference 
		{
			get { return _datum.Scale_Diff; }
			set { _datum.Scale_Diff=value; }
		}
		#region IProperties Member

		[Browsable(false)]
		public object SelectedObject
		{
			get
			{
				return _datum;
			}
		}

		#endregion
	}
}
