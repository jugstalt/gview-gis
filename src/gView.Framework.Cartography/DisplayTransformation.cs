using gView.Framework.Core.Carto;
using gView.Framework.Core.Geometry;
using gView.Framework.Geometry;
using System;

namespace gView.Framework.Cartography;

internal class DisplayTransformation : IDisplayRotation
{
    private bool _useDisplayRotation = false;
    private double _cos = 1.0, _sin = 0.0, _rotation = 0.0;

    private readonly IDisplay _display;

    public DisplayTransformation(IDisplay display)
    {
        _display = display;
    }

    public bool UseDisplayRotation
    {
        get { return _useDisplayRotation; }
    }

    public double DisplayRotation
    {
        set
        {
            _rotation = value;
            _cos = Math.Cos(_rotation * Math.PI / 180.0);
            _sin = Math.Sin(_rotation * Math.PI / 180.0);

            _useDisplayRotation = _rotation != 0.0;
        }
        get { return _rotation; }
    }

    public void Rotate(ref double x, ref double y)
    {
        if (_display == null || _useDisplayRotation == false)
        {
            return;
        }

        x -= _display.ImageWidth / 2.0;
        y -= _display.ImageHeight / 2.0;

        double x_ = x, y_ = y;

        x = x_ * _cos + y_ * _sin;
        y = -x_ * _sin + y_ * _cos;

        x += _display.ImageWidth / 2.0;
        y += _display.ImageHeight / 2.0;
    }

    public void RotateInverse(ref double x, ref double y)
    {
        if (_display == null || _useDisplayRotation == false)
        {
            return;
        }

        x -= _display.ImageWidth / 2.0;
        y -= _display.ImageHeight / 2.0;

        double x_ = x, y_ = y;

        x = x_ * _cos - y_ * _sin;
        y = x_ * _sin + y_ * _cos;

        x += _display.ImageWidth / 2.0;
        y += _display.ImageHeight / 2.0;
    }

    public IEnvelope RotatedBounds()
    {
        if (_display == null)
        {
            return new Envelope();
        }

        if (_useDisplayRotation == false)
        {
            return _display.Envelope;
        }

        IEnvelope oBounds = _display.Envelope;
        Envelope bounds = new Envelope(_display.Envelope);
        bounds.TranslateTo(0.0, 0.0);

        IPointCollection pColl = bounds.ToPointCollection(0);
        for (int i = 0; i < pColl.PointCount; i++)
        {
            IPoint point = pColl[i];
            double x = point.X * _cos + point.Y * _sin;
            double y = -point.X * _sin + point.Y * _cos;
            point.X = x;
            point.Y = y;
        }

        bounds = new Envelope(pColl.RecalcedEnvelope);
        bounds.TranslateTo(oBounds.Center.X, oBounds.Center.Y);
        return bounds;
    }
}
