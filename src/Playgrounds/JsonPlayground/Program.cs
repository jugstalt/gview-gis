using JsonPlayground;

gView.GraphicsEngine.Current.Engine = new gView.GraphicsEngine.Skia3x.SkiaGraphicsEngine(96.0f);
gView.GraphicsEngine.Current.Encoder = new gView.GraphicsEngine.Skia3x.SkiaBitmapEncoding();

JsonGeometryPlayground.Do();
JsonRendererPolyground.Do();
MapServerSettingsPlayground.Do();
GeoJsonPolyground.Do();