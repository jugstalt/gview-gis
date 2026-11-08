---
status: accepted
---

# Vector tile cache

gView's tile cache produces raster tiles (PNG/JPG) for the grid in the tile service metadata. That grid is any EPSG code, the metadata scales, and an upper-left or lower-left origin. Vector tile caches from other vendors (Mapbox, MapLibre) only exist in WebMercator with the standard zoom levels. gView adds vector tiles as one more **format of the same tile cache**, not as a separate product. They use the same grid as the raster tiles, so they work in national projections and scales too. When the metadata grid is the standard WebMercator grid, the tiles are fully Mapbox conform and every client can read them.

## Decisions

- **Format:** Mapbox Vector Tiles (MVT 2.1, `.pbf`, gzipped, extent 4096) for every projection. MVT itself does not depend on the projection, only the grid does. One file per tile contains one MVT layer per **map layer** (not per feature class). This is because filters and scale ranges belong to the map layer. The layer name is the TOC path, normalized to `[a-z0-9_]`, with a numeric suffix if two names collide (`MvtLayerNames`).
- **Content:**
  - Only feature layers visible at the tile's scale are included. Raster layers are skipped.
  - Geometries are projected to the tile CRS, then clipped with a buffer of 64/4096, simplified with a tolerance of about 1 tile pixel, and quantized.
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
4. Labels as GL `symbol` layers. The font question is open: OpenLayers uses web fonts, MapLibre needs glyph PBFs.
5. Sprites for picture and font markers and pattern fills.
6. Vector tiles in the WMTS capabilities, a PBF reader in the `VectorTileCache` data source, and integration in the Leaflet viewer.

## Consequences

- A tile is rendered by querying features, not by drawing. Large polygons are expensive per tile. This is mitigated by clipping to the buffered tile and by caching. A stack overflow in SpatialNET's union-find on large polygons was fixed in 0.10.0-preview.2.
- Clients of a custom projection need the grid (`gview:grid`) and a proj4 definition. Mapbox and MapLibre clients only work with Mapbox conform WebMercator caches.
- The test viewer uses the full builds `ol.js` and `olms.js`: CDN module builds bundle every OpenLayers file separately and break `instanceof` between `ol` and `ol-mapbox-style`.
- Tests: `src/tests/gView.Framework.Cartography.VectorTiles.Tests` covers layer names and grid math (WebMercator conformity, custom grid against the raster tile formula). The MVT encoding itself is tested in SpatialNET.
