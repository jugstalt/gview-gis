using System.Collections.Generic;

namespace gView.Framework.Symbology
{
    public class AnnotationPolygonCollection : List<IAnnotationPolygonCollision>, IAnnotationPolygonCollision
    {
        #region IAnnotationPolygonCollision Member

        public bool CheckCollision(IAnnotationPolygonCollision poly)
        {
            foreach (IAnnotationPolygonCollision child in this)
            {
                if (child.CheckCollision(poly))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(float x, float y)
        {
            foreach (IAnnotationPolygonCollision child in this)
            {
                if (child.Contains(x, y))
                {
                    return true;
                }
            }
            return false;
        }

        public AnnotationPolygonEnvelope Envelope
        {
            get
            {
                if (this.Count == 0)
                {
                    return new AnnotationPolygonEnvelope(0, 0, 0, 0);
                }

                AnnotationPolygonEnvelope env = this[0].Envelope;
                for (int i = 1; i < this.Count; i++)
                {
                    env.Append(this[i].Envelope);
                }
                return env;
            }
        }

        #endregion
    }
}
