# Leaflet.gViewVectorTiles

A Leaflet layer for **vector tiles (Mapbox Vector Tiles, MVT) styled with GL styles**:

- the vector tiles of **gView Server** in **any projection, any scales and any tile size**, styled with the GL style
  that gView generates from the map's renderers;
- **classic WebMercator vector tiles** (z/x/y) with their GL style, e.g. basemap.at, OpenMapTiles or ESRI vector
  tile services, in Leaflet maps in WebMercator.

Leaflet's usual vector tile plugins expect the WebMercator grid (z/x/y). gView publishes vector tile caches for
the grid in the tile service metadata, e.g. EPSG:31256 with scales 1:1,000,000 … 1:500 and 512 px tiles. This
plugin reads that grid (`gview:grid` in the style) and draws the tiles into any Leaflet map of the same
projection.

- **One file, no dependencies** besides Leaflet 1.9. Protobuf and MVT are decoded in the plugin.
- **Any map zoom levels:** the map's zoom levels don't have to match the cache scales. The plugin draws the tiles
  of the nearest cache level at the map's resolution, and while a level is loading it draws coarser tiles.
- **Style:** fills, lines (width, dashes), circles, sprites (icons, fill patterns) and labels (font, size, halo,
  anchor, offset, along lines), with collision handling ("declutter"). Sizes interpolated over the zoom follow the
  map's resolution.
- **Classic sources:** `tiles` URLs, TileJSON (`url`) and ESRI VectorTileServer sources, several sources per style,
  relative URLs, legacy GL functions (`stops`), `{field}` tokens, legacy filters, SDF icons tinted with
  `icon-color`, repeated line labels (`symbol-spacing`), `text-transform`, `text-letter-spacing`,
  `text-max-width` wrapping. 404/204 tiles count as empty.
- **Web worker:** tiles are loaded, decoded, filtered and rendered into images in a web worker (`OffscreenCanvas`),
  so dense maps (e.g. basemap.at in a city: hundreds of style layers, 600–900 KB tiles) don't block the page.
  The main thread places labels and puts the images together. The worker is created from the plugin's own code
  (blob URL), the plugin stays one file. Without `Worker`/`OffscreenCanvas` (or with a Content Security Policy
  without `worker-src blob:`) and for styles with several sources, everything runs on the main thread.
- **Fast panning and smooth zooming:** with one source, each tile is rendered once per zoom step into an image and
  reused. After zooming, the images of the previous step are shown scaled while the new ones render in small
  portions per frame (nearest to the center first). Tile requests are prioritized by distance to the center,
  limited in number and aborted when no longer visible. Labels are placed on every redraw, with collision handling
  across tiles (grid index, cached text widths).

## Usage

```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/leaflet.css">
<script src="https://cdn.jsdelivr.net/npm/leaflet@1.9.4/dist/leaflet.js"></script>
<!-- only for maps in another projection than WebMercator -->
<script src="https://cdn.jsdelivr.net/npm/proj4@2.12.1/dist/proj4.js"></script>
<script src="https://cdn.jsdelivr.net/npm/proj4leaflet@1.0.2/src/proj4leaflet.js"></script>

<script src="leaflet.gview-vectortiles.js"></script>
```

Into an existing map (your own CRS, e.g. GK M34):

```js
const crs = new L.Proj.CRS('EPSG:31256', '+proj=tmerc +lat_0=0 +lon_0=16.3333333333333 +k=1 +x_0=0 +y_0=-5000000 +ellps=bessel +towgs84=577.326,90.129,463.919,5.137,1.474,5.297,2.4232 +units=m +no_defs', {
  origin: [-1000000, 1000000],
  resolutions: [500, 250, 100, 50, 20, 10, 5, 2.5, 1, 0.5]
});
const map = L.map('map', { crs });

L.gViewVectorTiles('https://myserver/gview/vtc/myfolder/myservice').addTo(map);
```

Classic vector tiles (the map in WebMercator, Leaflet's default CRS):

```js
const map = L.map('map').setView([48.2082, 16.3738], 13);
L.gViewVectorTiles('https://mapsneu.wien.gv.at/basemapvectorneu/root.json', {
  attribution: '<a href="https://basemap.at">basemap.at</a>'
}).addTo(map);
```

A URL ending in `.json` is a style URL; anything else is the `vtc` base of a gView service.

The service URL is the `vtc` base of the gView service: `{gView Server}/vtc/[{folder}/]{service}`. The plugin
loads `{url}/style.json?epsg={EPSG of the map}`. The tile URLs, sprites and the tile grid come from there.

A map whose zoom levels are exactly the cache levels:

```js
const style = await (await fetch(url + '/style.json')).json();
const grid = style.metadata['gview:grid'];
const crs = L.gViewVectorTiles.createCrs(grid, proj4Definition);   // L.Proj.CRS from the grid (+3 overzoom levels)
const map = L.map('map', { crs, maxZoom: crs.options.resolutions.length - 1 });
L.gViewVectorTiles(url, { epsg: grid.epsg }).addTo(map);
```

## Options

| Option | Default | |
|---|---|---|
| `epsg` | EPSG code of the map's `crs.code` | the projection of the cache to load |
| `style` | `{url}/style.json?epsg=…` | another style URL |
| `declutter` | `true` | hide labels and icons overlapping earlier ones |
| `debug` | `false` | draw the tile borders and coordinates |
| `maxCachedTiles` | `384` | decoded tiles kept in memory |
| `maxConcurrentRequests` | `8` | tile requests at the same time |
| `renderBudget` | `12` | milliseconds per frame for rendering tile images (main thread rendering) |
| `worker` | `true` | render in a web worker where the browser supports it |
| `maxConcurrentRenders` | `4` | tile images rendered in the worker at the same time |
| `fetchOptions` | `{}` | options for `fetch`, e.g. `{ credentials: 'include' }` or headers for authorization |
| `pane` | `'overlayPane'` | the Leaflet pane |
| `attribution` | `null` | |

Events: `load` (style loaded), `error` (`e.error`). Methods: `getStyle()`, `getGrid()`.

## Requirements and limits

- A Proj4Leaflet CRS only knows the zoom levels it has resolutions for: set the map's `maxZoom` to its last
  level, otherwise zooming beyond it gives no scale (NaN) in Leaflet.

- The map's projection must be the cache's projection (gView caches) or WebMercator (classic sources). Reprojecting
  tiles on the client is not supported, so classic WebMercator tiles can't be shown in a GK M34 map.
- Canvas 2D, not WebGL: complex styles (hundreds of layers, very detailed tiles) render well, in the worker without
  blocking the page, but a new zoom step takes longer to fill in than in MapLibre. Labels are placed on the main
  thread on every redraw. Expressions: `get`, `has`, comparisons, `!`, `all`,
  `any`, `in`, `to-string`, `to-number`, `concat`, `geometry-type`, `interpolate`, `match`, `case`, `step`, and
  legacy functions/filters. Not supported: `line-offset`, `*-translate`, `fill-extrusion`, `heatmap`, `raster`
  layers.
- Labels with `symbol-placement: line` follow the line glyph by glyph (kept upright, not where the line bends more
  than `text-max-angle`). Labels repeat per tile for lines crossing tile borders, as in other vector tile clients.
- Fonts are CSS fonts: `text-font` "Arial Bold" becomes `bold … "Arial"`. The glyph PBFs of the style are only
  for MapLibre.

## Test viewer

`src/Playgrounds/VectorTilesViewer/leaflet.html` in the gView repository loads a service and builds the map's CRS
from the cache grid. Serve the `src` folder (e.g. `python -m http.server --directory src`) and open
`/Playgrounds/VectorTilesViewer/leaflet.html`, because the page loads the plugin from `../../Clients/`.
