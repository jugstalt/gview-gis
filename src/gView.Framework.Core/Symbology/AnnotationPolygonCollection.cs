using System;
using System.Collections.Generic;

namespace gView.Framework.Core.Symbology
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
                if (Count == 0)
                {
                    return new AnnotationPolygonEnvelope(0, 0, 0, 0);
                }

                AnnotationPolygonEnvelope env = this[0].Envelope;
                for (int i = 1; i < Count; i++)
                {
                    env.Append(this[i].Envelope);
                }
                return env;
            }
        }

        public IAnnotationPolygonCollision WithSpacing(SymbolSpacingType type, float spacingX, float spacingY)
        {
            if (type == SymbolSpacingType.None) return this;

            var envelope = this.Envelope;
            float centerX = (envelope.MinX + envelope.MaxX) * 0.5f;
            float centerY = (envelope.MinY + envelope.MaxY) * 0.5f;
            float width = Math.Abs(envelope.MaxX - envelope.MinX);
            float height = Math.Abs(envelope.MaxY - envelope.MinY);

            var (minWidth, minHeight) = type switch
            {
                SymbolSpacingType.BoundingBox => (spacingX, spacingY),
                SymbolSpacingType.Margin => (width + 2f * spacingX, height + 2f * spacingY),
                _ => (0, 0)
            };

            if (width < minWidth || height < minHeight)
            {
                width = Math.Max(minWidth, width);
                height = Math.Max(minHeight, height);

                var x1 = centerX - width / 2f;
                var y1 = centerY - height / 2f;

                return new AnnotationPolygon(x1, y1, width, height);
            }

            return this;
        }

        #endregion
    }
}
