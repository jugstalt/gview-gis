using gView.Framework.IO;
using gView.Framework.system;
using System;

namespace gView.Framework.Symbology
{
    public class SymbolRotation : IPersistable, IClone
    {
        private RotationType _rotType = RotationType.aritmetic;
        private RotationUnit _rotUnit = RotationUnit.deg;
        private string _rotationFieldName = "";

        public RotationType RotationType
        {
            get { return _rotType; }
            set { _rotType = value; }
        }
        public RotationUnit RotationUnit
        {
            get { return _rotUnit; }
            set { _rotUnit = value; }
        }
        public string RotationFieldName
        {
            get { return _rotationFieldName; }
            set { _rotationFieldName = value; }
        }

        public double Convert2DEGAritmetic(double rotation)
        {
            //if (_rotType == RotationType.aritmetic && _rotUnit == RotationUnit.deg) 
            //    return -rotation;

            switch (_rotUnit)
            {
                case RotationUnit.rad:
                    rotation *= 180.0 / Math.PI;
                    break;
                case RotationUnit.gon:
                    rotation /= 0.9;
                    break;
            }

            switch (_rotType)
            {
                case RotationType.aritmetic:
                    rotation = 90 - rotation;
                    break;
                case RotationType.geographic:
                    //rotation = 90 - rotation;
                    break;
            }

            if (rotation < 0.0)
            {
                rotation += 360.0;
            }

            return rotation;
        }

        #region IPersistable Members

        public string PersistID
        {
            get { return ""; }
        }

        public void Load(IPersistStream stream)
        {
            _rotationFieldName = (string)stream.Load("RotationFieldname", "");
            _rotType = (RotationType)stream.Load("RotationType", RotationType.aritmetic);
            _rotUnit = (RotationUnit)stream.Load("RotationUnit", RotationUnit.deg);
        }

        public void Save(IPersistStream stream)
        {
            if (_rotationFieldName == "")
            {
                return;
            }

            stream.Save("RotationFieldname", _rotationFieldName);
            stream.Save("RotationType", (int)_rotType);
            stream.Save("RotationUnit", (int)_rotUnit);
        }

        #endregion

        #region IClone Members

        public object Clone()
        {
            SymbolRotation rot = new SymbolRotation();
            rot._rotationFieldName = _rotationFieldName;
            rot._rotType = _rotType;
            rot._rotUnit = _rotUnit;
            return rot;
        }

        #endregion
    }
}
