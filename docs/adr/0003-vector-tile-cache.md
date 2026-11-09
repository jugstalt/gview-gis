---
status: accepted
---

# Vector tile cache

gView's tile cache produces raster tiles (PNG/JPG) for the grid in the tile service metadata. That grid is any EPSG code, the metadata scales, and an upper-left or lower-left origin. Vector tile caches from other vendors (Mapbox, MapLibre) only exist in WebMercator with the standard zoom levels. gView adds vector tiles as one more **format of the same tile cache**, not as a separate product. They use the same grid as the raster tiles, so they work in national projections and scales too. When the metadata grid is the standard WebMercator grid, the tiles are fully Mapbox conform and every client can read them.

## Decisions

- **Format:** Mapbox Vector Tiles (MVT 2.1, `.pbf`, gzipped, extent 4096) for every projection. MVT itself does not depend on the projection, only the grid does. One file per tile contains one MVT layer per **map layer** (not per feature class). This is because filters and scale ranges belong to the map layer. The layer name is the TOC path, normalized to `[a-z0-9_]`, with a numeric suffix if two names collide (`MvtLayerNames`).
- **Content:**
  - Only feature layers visible at the tile's scale are included. Raster layers are skipped.
  - Geometries are projected to the tile CRS, then clipped with a buffer, simplified with a tolerance of about 1 tile pixel, and quantized.
  - The buffer is 64/4096, or more at scales where half of the widest line or symbol (reference scale included) reaches further. Clipping adds artificial polygon edges on the buffer border. Clients cut each tile at its border, so these edges stay invisible only if thick outlines drawn along them don't reach into the tile. Clients show a level's tiles scaled between the neighbor levels (OpenLayers picks the nearest level), while the style interpolates widths over the zoom. So the buffer covers the widths half way to the finer level (wider lines, magnified tile) and to the coarser level (shrunk tile). The buffer is at most half a tile.
  - The last scale is not simplified and acts as the overzoom level.
  - Attributes are the fields the renderers need (`PrepareQueryFilter`) plus the fields configured in `mvtLayerAttributes`.
  - The OID becomes the MVT feature id.
  - Tiles larger than `mvtSizeWarningKb` are logged.
- **Metadata:** `supportMvt` sits next to `supportPng`/`supportJpg`, and `TileImageFormat.pbf` sits next to `png`/`jpg`. A service can have a raster cache and a vector cache side by side.
- **Storage:** Tiles use the paths of the raster cache with the extension `.pbf`. In the compact layout the bundles are named `.pbfbundle`/`.pbfbundlx`, so they never collide with raster bundles. An empty tile is stored as a zero-length file, or as a missing bundle index entry.
- **Serving:** The new `vtc/...` route (`VtcController` → `VectorTileRequest`) serves the tiles. It does not reuse the WMTS route, because `style.json`, TileJSON and `z/x/y` need their own URLs anyway.
  - **On the fly:** Missing tiles are rendered when `RenderTilesOnTheFly` is set, and written to the exploded cache. Compact bundles are only written by seeding.
  - **`z/x/y`:** It is only offered when EPSG:3857 uses the standard origin and a scale is exactly a WebMercator zoom (relative to the tile's ground size, so 512px tiles work too). `z` is then the real zoom level, not the scale index.
  - **Seeding:** It goes through the existing `tiles` interpreter with the extension `.pbf`.
- **Styling:** `style.json` is a MapLibre/Mapbox GL style (v8) derived from the renderers as far as GL can express them (`GlStyleBuilder`).
  - **Zoom:** The style's zoom is the real zoom for a Mapbox conform cache. Otherwise it is the scale index, which is how OpenLayers numbers the levels of a custom tile grid.
  - **Grid:** The style and the TileJSON carry the grid as `gview:grid`, so clients can build the tile grid of a custom projection.
  - **Renderers and symbols:** Phase 1 supports Simple, ValueMap, ManyValueMap, Quantity, ScaleDependent and FeatureGroup, with fill, line and circle symbols.
  - **Approximations:** Whatever GL can't draw is approximated (hatch → fill color, picture or font marker → circle) and listed in `gview:warnings`.
  - **Sizes:** Sizes follow the reference scale and meter units per level with `interpolate`.
- **Geometry engine:** Encoding, decoding, clipping, simplification, quantization and grid math live in **SpatialNET.IO**, written without additional packages, directly from the MVT spec. It is not behind a `GVIEW_SPATIALNET` switch, because there is no classic implementation to fall back to.
  - **Exception to ADR 0002:** `gView.Framework.Cartography.VectorTiles` references SpatialNET.IO directly, because it is a tile and IO format, not a geometry operation of gView's API. All other projects keep using gView's API. The SpatialNET.IO version is pinned to the same preview as `gView.Framework.Geometry`.

## Phases

1. SpatialNET.IO: encoder, decoder, tile transform, grid (done, 0.10.0-preview.2).
2. gView: metadata, tile builder, seeding, serving (done).
3. Style, TileJSON, OpenLayers test viewer `src/Playgrounds/VectorTilesViewer` (done).
4. Labels as GL `symbol` layers (done for OpenLayers). The label text is computed by gView (`SimpleLabelRenderer.EvaluateLabelText`, so expressions and scripts work) and written to an extra MVT layer `{layer}__labels`. Points and polygons get one label point per feature, written only into the tile that contains it, so labels don't repeat per tile. Lines carry their geometry for line placement. Large polygons would otherwise only be labelled where their single label point happens to be visible. So a polygon part covering at least `mvtLabelTileCoverage` of a tile (overridable per layer; default 0 = off, one label per polygon, until a representative service shows a good value) gets its own label point in that tile. Being relative to the tile, this adapts to the scale by itself, and small polygons crossing a tile border keep one label. `text-font` is a GL font stack ("Arial Bold"): OpenLayers maps it to CSS fonts. For MapLibre there is a glyph endpoint `vtc-glyphs/{fontstack}/{start}-{end}.pbf` (done). gView renders the glyphs from the installed fonts with its graphics engine as signed distance fields: 24 px, buffer 3, radius 8, cutoff 0.25 (fontnik/TinySDF), with no font files or external glyph server. Metrics are measured from the rendered ink (the bottom of "H" = baseline), so they don't depend on how an engine positions text. The first installed family of the stack is used. Results are cached in `{TileCachePath}/_glyphs`.
5. Sprites (done). `sprite.json`/`sprite.png` (and `@2x`) are rendered by gView from the symbols the style refers to. `GlStyleBuilder` collects them while it builds the style, so sheet and style always match. Simple circle markers and simple fills stay plain GL (`circle`, `fill-color`). Everything else becomes `icon-image` (base size = symbol size, `icon-size` scaled per zoom) or a 32 px `fill-pattern` without outline. Complex line symbols (dotted, capped) are still not supported.
6. Vector tiles in the WMTS capabilities (done): one extra layer per EPSG code (`… EPSG:{epsg} vector`), format `application/vnd.mapbox-vector-tile`, a RESTful template on the `vtc` route (`{TileMatrix}` = scale index), and a matrix set with all levels, because vector tiles don't need a seeded cache directory. The WMTS route itself still serves raster tiles only.
7. Reading gView vector tiles of a custom grid (deferred). The existing `gView.DataSources.VectorTileCache` already reads MVT, but only in the WebMercator grid (`mapbox-vector-tile`, via GeoJSON in WGS84). It stays as it is. If needed, a **separate data source project** reads tiles via SpatialNET.IO: the grid comes from `gview:grid` in the TileJSON (standard WebMercator otherwise), and geometries are decoded directly in the tile CRS. Open points there:
   - The SpatialNET → gView geometry conversion is `internal` in `gView.Framework.Geometry`. The preferred way is a small public "MVT feature → gView geometry" method there, instead of another exception to ADR 0002.
   - It is untested whether `gView.Framework.Vtc` understands the expression syntax of the generated styles.
   - It is deferred because gView clients can use the map service directly; it pays off only for sharing vector tiles between servers or offline.
8. Leaflet plugin (done): `src/Clients/Leaflet.gViewVectorTiles`, a standalone file to take from the repository, so Leaflet clients such as a WebGIS in GK M34 can use gView's vector tiles (Leaflet only had WebMercator plugins). It doesn't use VectorGrid, because VectorGrid ties the tile grid to the map's zoom levels.
   - **Rendering:** its own canvas layer decodes MVT itself. It reads the grid from `gview:grid` and draws the nearest cache level at the map's resolution, so the map's CRS only needs the same projection, not the cache scales. Coarser tiles are drawn while a level loads.
   - **Style:** it evaluates the GL style subset gView generates: filters, `interpolate`, fill, line and circle, sprite icons and patterns, and labels with declutter, drawn straight along lines.
   - **Viewer:** the test viewer `src/Playgrounds/VectorTilesViewer/leaflet.html` builds the map CRS from the grid (`L.gViewVectorTiles.createCrs`).

## Consequences

- A tile is rendered by querying features, not by drawing. Large polygons are expensive per tile. This is mitigated by clipping to the buffered tile and by caching. A stack overflow in SpatialNET's union-find on large polygons was fixed in 0.10.0-preview.2.
- Clients of a custom projection need the grid (`gview:grid`) and a proj4 definition. Mapbox and MapLibre clients only work with Mapbox conform WebMercator caches.
- The test viewer uses the full builds `ol.js` and `olms.js`: CDN module builds bundle every OpenLayers file separately and break `instanceof` between `ol` and `ol-mapbox-style`.
- Tests: `src/tests/gView.Framework.Cartography.VectorTiles.Tests` covers layer names and grid math (WebMercator conformity, custom grid against the raster tile formula). The MVT encoding itself is tested in SpatialNET.
