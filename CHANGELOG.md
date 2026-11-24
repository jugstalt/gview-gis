# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

## 9.26.4306

## Added

- gView.Pipe plugin `gView.Pipe.Plugins.Gis` ([ADR 0004](docs/adr/0004-gview-as-pipe-plugin.md)):
  nodes for the workflow app gView.Pipe that work with the gView data sources. gView.Pipe loads
  it from a gView app folder (`Pipe:ExternalPlugins`).
  - Credential type gView Datasource: PostGIS, SQL Server geometry, SDE, shape files,
    GeoPackage, gView FDB or any dataset plugin GUID; `Allow schema changes` switch.
  - Read Features: features as GeoJSON items, with where clause, match field, spatial filter
    (a GeoJSON geometry or a bounding box), field list, order by, offset, limit and target SRS;
    output as features, distinct values or a count. The feature class is a choice list of the
    data source.
  - Write Features: insert, update, upsert and delete by object id or a key field; geometries
    are projected into the feature class. Write fields and write geometry limit what is
    written (e.g. only one field of an update), field mapping renames properties, unknown
    properties are ignored or an error, and with `On error` = skip item the items that can't be
    written go to an Error output. Mode Update where sets fixed values on all features of a where
    clause without items, e.g. a default colour before the computed ones.
  - Feature Class: list, describe, create (from a described schema, with additional and
    excluded fields), create if not exists, truncate and delete feature classes.
  - Copy Feature Class: copies a feature class with its schema and features inside gView, from
    one data source to the same or another (create, append, replace; where and spatial filter),
    for volumes too large for items.
  - Geometry, Spatial Aggregate and Spatial Join: buffer, centroid, hulls, simplify, make valid,
    densify, area/length; dissolve with group by and aggregates, union, Voronoi, Delaunay, line
    merge, polygonize; intersect, clip, difference, cut and spatial join (intersects, within,
    contains, nearest); offset curve, smooth, point at distance; locate points along lines;
    Voronoi cells cut to a clip geometry. They work on the GeoJSON items with SpatialNET only,
    without a data source.
  - Network Trace and the credential type gView Network (the folder of a network built with
    gView): Feeders labels every line with the source and branch it hangs on (e.g. transformer
    and connector) in one run, Multiple for lines reached from more than one branch; Trace runs
    once per input item. One item per line feature without geometry, object id as `id`.
    Output fields (source and branch id, all ids as text, kind, classes as object id modulo Classes, Multiple and
    Open class) and Open lines (switched off lines at a separation point, kind Open).
  - Deploy build: the plugin is part of the server and web apps ZIPs and Docker images.
- Network searches: new operation `feeders` (gView.Cmd `-u feeders -branches …`, server REST
  `networkserver/feeders?branches=…`): every edge gets its source, its branch (the first edge
  at the source, e.g. the connector) and its cost from the source; `openEdges` / `-open-edges`
  adds the edges no branch went along at reached nodes as Open.
- PlugInManager: `PluginDirectory` sets the folder scanned for plugins (default: folder of the
  entry assembly); inside an isolated load context plugins are loaded into that context.
  `gView.Pipe.*.dll` are never scanned.

- Vector tile cache (Mapbox Vector Tiles, MVT 2.1) for the grid of the tile service metadata
  ([ADR 0003](docs/adr/0003-vector-tile-cache.md)): any EPSG code and the metadata scales, not
  only WebMercator. New metadata options `supportMvt`, `mvtLayerAttributes` (extra fields per
  layer, `*` for all) and `mvtSizeWarningKb`.
  - Tiles: one gzipped `.pbf` per tile with one MVT layer per map layer, clipped, simplified
    and quantized per scale with SpatialNET.IO; the OID becomes the feature id, raster layers
    are skipped.
  - Seeding: `RenderTileCacheCommand` / DataExplorer "Render Tile Cache" with image format
    `pbf`, exploded and compact (`.pbfbundle`).
  - Serving: `vtc/[{folder}/]{name}/{exploded|compact}/{ul|ll}/{epsg}/{level}/{row}/{col}.pbf`,
    rendered on the fly and cached if the metadata allows it, and
    `vtc/[{folder}/]{name}/{z}/{x}/{y}.pbf` for Mapbox conform WebMercator caches.
  - Styling: `vtc/[{folder}/]{name}/style.json` (MapLibre/Mapbox GL style v8 derived from the
    Simple, ValueMap, ManyValueMap, Quantity, ScaleDependent and FeatureGroup renderers, sizes
    following the reference scale) and `tilejson.json`, both with the tile grid as `gview:grid`.
  - Labels: Simple, Advanced and ScaleDependent label renderers become GL `symbol` layers. The
    label text is computed by gView (field, expression, script) and stored in an extra MVT layer
    `{layer}__labels`, on one label point per feature (polygons, points) or along the line.
    Font family and style, size (reference scale), color, halo (glowing text), alignment and
    offset are taken over; blockout labels are approximated by a halo.
    Large polygons are labelled in every tile their part covers to at least
    `mvtLabelTileCoverage` (metadata, per layer `mvtLayerLabelTileCoverage`; default 0 =
    one label per polygon).
  - Sprites: `vtc/[{folder}/]{name}/sprite.json|png` and `sprite@2x.json|png`, rendered by gView
    from the renderers' symbols. Picture, svg, font and non-circle markers become `icon-image`
    (scaled per zoom with the reference scale), hatches and other fills become `fill-pattern`.
  - WMTS: the capabilities list a layer `{service} EPSG:{epsg} vector` per EPSG code with the
    format `application/vnd.mapbox-vector-tile` and a RESTful template on the `vtc` route
    (all levels, no seeded cache needed).
  - Glyphs for MapLibre/Mapbox clients: `vtc-glyphs/{fontstack}/{start}-{end}.pbf` (signed
    distance fields rendered from the installed fonts with gView's graphics engine, cached in
    `{TileCachePath}/_glyphs`); styles with labels refer to it as `glyphs`.
  - Leaflet clients: the separate project
    [leaflet-gl-vectortiles](https://github.com/jugstalt/leaflet-gl-vectortiles) draws gView vector
    tiles of any grid into Leaflet maps of the same projection (e.g. Proj4Leaflet maps in GK M34).
  - Test viewer for OpenLayers: `src/Playgrounds/VectorTilesViewer/index.html`.
- Networks (SpatialNET): utility and street networks built from the point and line layers of a map,
  searched in gView.Cmd, Carto and gView Server (new project `gView.Framework.NetworkModel`).
  - Config: `NetworkConfig` (JSON) with the node and edge layers, type facets (e.g. Ebene,
    Objektart, Bauart; values from fields or templates), weights, tolerance, shard polygons and
    the network directory (default `{map}.network` next to the mxl). Kept in the map as
    `NetworkModule`; Carto edits it on the module page (tabs General, Layers, Rules, Build, JSON
    import/export, pattern pickers per facet) and on a Network page in the layer settings.
  - Build: gView.Cmd `Network -u build` (or `-u config-from-mxl` for a config from a map), from
    the Carto module page and the search tool (status and live log), and from the server admin
    page (background job per service, status, reports). One binary file per shard plus
    `nodes.idx` and `network.json`, written into `{dir}.building` and swapped in atomically.
    Duplicate points of one layer are left out; points of different layers at one place stop the
    build (`NodeConflicts = Error`) or are merged by TOC priority (`PriorityByToc`, the others
    become aliases, `aliases.idx`, `merged.json`). Node and edge ids follow from the features and
    stay the same between builds.
  - Searches (`NetworkQueryService`, shared by all three): `trace`, `reach`, `path`, `supply`,
    `info` and `validate`, with type filters, blocked nodes, weights and a maximum cost. Nodes are
    given as `Layer#OID`, id or type pattern (e.g. `[*,Trafo,*]`); merged points stand for their
    node. Results as CSV or GeoJSON with the real parts of the line features (measures).
    `FileNetworkGraph` loads shards on demand and keeps them in an LRU cache;
    `NetworkGraphCache` opens the network again after a rebuild.
  - Rules: neighbour count rules in the config (and `network.json`), checked by
    `validate` with a summary per rule and the violations (also of merged points); never at build.
  - gView Server: NetworkServer as its own resource
    `geoservices/rest/services/[{folder}/]{id}/networkserver` and `/{op}` (HTML forms without
    parameters), listed as service type when the network is built and the user has the new right
    `network`, linked from the MapServer page. Parameters as in the CLI plus `outSR`, `geometry`
    and `attributes=true` (fields of the original features, `outFields`, needs the right `query`).
    Networks live in `{services-folder}/networks/{folder}/{service}.network` and are removed
    with the service. Limits in appsettings `Network`: `MaxResultNodes` (50000), `TimeoutSeconds`
    (120), `MaxParallel` (2), `MaxLoadedShards` (512), `IdleMinutes` (30).
  - Carto: "Network Search" tool: click a node, choose the search, the result is drawn with the
    real line parts and can become the selection or a GeoJSON file in `{network}/results`;
    validate uses the rules of the map without a rebuild and lists the violations (click zooms),
    the report is a side panel.
- Networks: line ends that only arise when the network is built (helper nodes, e.g. where a
  Leitung hangs on an Anlagen Verbinder at a station) can be picked and used as start nodes, and a
  branch can be switched off to see which customers lose their supply.
  - Build: the network directory gets `spatial.idx` (every node, line end and merged point at its
    location) and `edges.idx` (every edge id); networks built before have to be built again.
  - References: `@x,y[~radius]` means the nearest node (also a line end, default radius 10 x the
    build tolerance), edges are given as `Layer#OID` (every part of the line), `Layer#OID/part`
    or id. Line ends are labelled with their lines, e.g. `end of Verbinder#12, Leitung#3456`.
  - Type patterns can be negated with a leading `!`, e.g. the filter `![*,Anlagen Verbinder,*]`
    keeps a trace from a line end on its branch instead of running back over the station.
  - Searches (`NetworkQueryService`, gView.Cmd, NetworkServer, Carto): `disable` switches edges off
    for one search; new operation `outage` switches edges off and reports the affected customers
    (supplied before, not after; only the connected part of the network is computed, found
    without the directions of the edges so a box finds its station also in networks with
    directed lines, while the supply itself follows the directions), the
    customers still supplied through a mesh and the dead edges; new operation `nearest` lists the
    nodes nearest to `x`/`y`. Supply and outage default to the new network config settings
    `supplySources` and `customers` (Carto: network module, tab General, card "Supply").
  - NetworkServer: `inSR` for `x`/`y` and the `@x,y` references, `snap=true` sets the line ends
    of the result exactly onto the nodes (a valid network for power flow software; a shift over
    the build tolerance is reported), forms for `outage` and `nearest`; edges have a `kind` and
    GeoJSON edges `from_node`/`to_node`.
  - Carto network search: a click picks the nearest node of the network (real node, line end or
    merged point) instead of only the points of the node layers; outage picks the line part
    under the click, draws the switched off edge red and the dead edges orange; GeoJSON export
    can snap the line ends onto the nodes. Ctrl-click adds a pick or removes it again: several
    edges to switch off (outage), several starts (trace, reach), sources and sinks (supply).
    Map clicks in Carto now carry the Ctrl key (`MapClickEventArgs.CtrlKey`); a Ctrl-click without
    dragging is a click again (the Ctrl box of the Leaflet map swallowed it), Ctrl-drag still draws a box.
    The pin buttons show how many are picked ("1 edge", "3 nodes"); the report lists the picked items
    (label and reference, ✕ removes one, ⌖ zooms to it, also to typed `Layer#OID` and `@x,y`) with a
    copy button for the references, opens after Run and copies the result (text plus the nodes tab
    separated, e.g. for Excel; validate only the text). Networks are built only from the network module
    page; the Build button of the tool is gone. A running search can be cancelled (Run becomes Cancel)
    and shows where it stands (e.g. `trace: 28,672 node(s) found`, `supply: 120/840 source(s)`).
  - Searches (`NetworkQueryService.Run`) report their progress and stop on cancellation, also on the
    server when its timeout is reached (before, a search kept running in the background); gView.Cmd
    prints the progress lines.
- Carto: the map zooms deeper than the resolutions of the coordinate system, down to the setting
  `MapControl:MaxScale` (default 1:10); the resolutions beyond the last one are extrapolated and
  background tiles are upscaled beyond their native zoom.
- Carto: when the window gets narrow, the ribbon boxes collapse into menu buttons (File first, then
  Map) instead of wrapping out of sight.
- Carto: "Clear Selection" tool for the whole map and for the selected layer.
- Carto: the Value Map, Many Value Map and Quantity renderers change all class symbols at once
  (menu `More ...`, the Quantity renderer now has one too, `Remove All` moved into it):
  - `Recolor ...`: new colours in list order from a palette (Viridis, Blues, Greens, Reds,
    YlOrRd, RdYlGn, RdBu, Set1, Set3, Paired), a gradient between two colours or random; reverse,
    fill and/or outline for polygons, the transparency of each symbol is kept.
  - `Set Symbol Properties ...`: sets checked properties (colour, width, dash style, fill
    opacity, size, smoothing) to the same value for all classes.
  - "All other values" is left out unless included; classes with composite symbols are skipped.
- Carto: classes of these renderers can be selected (checkbox, Shift for a range, All / None /
  Invert, Remove selected); Recolor, Set Symbol Properties and paste then work on the selection
  only.
- Carto: copy and paste of symbols: Copy / Paste buttons next to Random and Undo wherever a symbol
  is edited (also across layers of the same geometry type), `Paste Symbol` and `Paste Symbol, keep
  colors` in the `More ...` menu of the class renderers and `Apply to ...` in the symbol card of a
  class. "Keep colors" takes the style from the pasted symbol and keeps the colour of each class.

## Changed

- Geometry: SpatialNET packages updated to 0.10.0-preview.6.
- Polygon labels (`SimpleLabelRenderer`, `ChartLabelRenderer`): all polygons now take the SpatialNET path
  that was only used for polygons with more than 12000 vertices before (the limit where Clipper and the ring
  raster became too slow or crashed). The label point is found first (`FastPolygonLabel`) and the polygon is only
  clipped if that point is not visible; for a collision the alternative points come lazily
  (`FastAlternativePolygonLabelsLazy`, at most 32, 20 pixels apart; `ISmartLabelPoint.AlternativeLabelPointsLazy`),
  and the classic raster is lazy too. Benchmarks per feature: polygons cut by the map edge or completely visible
  need about 1/10 to 1/40 of the time, large polygons (100000 vertices) 0.5 ms instead of 4-15 ms; a dense map
  with many label collisions draws its labels 15-30% faster. The alternative point positions differ from before and
  fewer alternatives are tried, so about 10% fewer labels are placed in very dense maps. The limit is the setting
  `GeometryEngineOptions.LabelSpatialNetMinVertices` (environment variable
  `GVIEW_LABEL_SPATIALNET_MIN_VERTICES`, default 0 = all polygons; `int.MaxValue` = classic path only,
  12000 = the old behaviour). The alternative points can be tuned with
  `GeometryEngineOptions.LabelAlternativeSeparationPixels` (`GVIEW_LABEL_ALTERNATIVE_SEPARATION_PIXELS`, default 20)
  and `GeometryEngineOptions.LabelMaxAlternatives` (`GVIEW_LABEL_MAX_ALTERNATIVES`, default 32): a smaller distance
  places more labels in dense maps but needs more label engine checks, a larger one is faster.
- Tile service metadata: `renderTilesOnTheFly` can be set in the metadata editor (YAML).

## Fixed

- Tile service: tiles rendered on the fly (vector and raster) failed with "file used by another process" /
  "access denied" when clients requested the same tile several times at once. A tile is now rendered once per
  process for concurrent requests (`TileCacheFile`), written via a temp file without overwriting, a locked cache
  file counts as not cached, and a failed cache write is only logged: the tile is delivered anyway.
- Vector tiles: layers whose scale range ends exactly at a cache scale were in some tiles and missing
  in others (scale computed per tile differed in the last digits); all tiles of a level now use the
  level's scale.
- Geometry: `PointCollection`/`MultiPoint` can be enumerated as `IEnumerable<IPoint>` (the
  generic enumerator threw `NotImplementedException`).
- Carto: a touch pinch zoom past the last resolution no longer resets the map to the world extent
  (max. zoom was off by one, the zoom is now clamped in the coordinate system).
- gView Server: services added by publishing get the full path of their mxl, so their `.settings`
  are written next to it instead of into the working directory.
- Network module: keeps its loaded config when the server calls `OnCreate` after `Load`.

## Security

gView Server hardening for internet-facing installations, see
[Security gView.Server](https://docs.webgiscloud.com/gview/en/setup/security-server.html).
**Breaking changes are marked (⚠).**

- First admin: a first login no longer creates the admin (anyone could take over a fresh
  installation). The admin comes from `GVIEW_ADMIN_USER`/`GVIEW_ADMIN_PASSWORD` or the login page
  asks for a one-time setup code that the server writes to the log (`AdminSetupService`). ⚠ A fresh
  installation needs one of the two.
- Passwords: PBKDF2-SHA256 instead of a single SHA256 (`SecureCrypto.HashPassword`); old hashes
  keep working and are replaced on the next successful login. Constant time comparisons.
- Login lockout per user and per client ip for the manage login, token login, `generateToken`
  and Basic auth (`LoginThrottleService`, `security:login`: 10 failures in 5 minutes lock for
  15 minutes); also for wrong setup codes.
- Unauthenticated side effects removed: `Manage/Collect` needs an admin, `Manage/Services` reloads
  only after the auth check, the legacy `RemoveMap`/`GetMetadata`/`SetMetadata` urls use the
  request credentials instead of always running anonymously. Usernames from requests can no
  longer leave the login folder (`ToLoginFile`).
- New `security` section in `mapserver.json` (`SecurityOptions`):
  - Reverse proxies: `X-Forwarded-For`/`-Proto` only from `security:known-proxies`/`known-networks`
    (default loopback and the private networks) via ASP.NET `ForwardedHeaders`; the own
    `XForwardedMiddleware` is gone. ⚠ A proxy on a public address must be entered.
  - HSTS for https requests and with `force-https` (also `security:force-https`), optional
    `https-redirect`.
  - Admin networks: `/manage`, publishing in `/browseservices` and the legacy publishing urls only
    answer to `security:manage:allowed-networks` (default loopback and the private networks,
    `"*"` = everywhere, with OIDC only if configured), others get a 404. Startup warning if the
    admin networks are restricted but no proxies are configured. ⚠ Admin access from outside
    needs an entry.
  - CORS only for the services (`security:cors:allowed-origins`), never for admin and login pages.
- Cookies and CSRF: auth cookie `SameSite=Lax`; state changing requests to Manage and BrowseServices
  sent with cookies need an antiforgery token (sent by `site.js` for every jQuery request, Carto
  publishing with an `Authorization` header is not affected).
- Security headers: `nosniff` and `Referrer-Policy` everywhere, `X-Frame-Options: DENY` and a CSP
  on the admin and login pages.
- Sessions: every token from a login carries a security stamp (`LoginStamp`, hash of `<user>.lgn`
  and `<user>.stamp`); a password change or **Sign out everywhere** (new, sidebar) ends all
  sessions and tokens of the user. Admin sessions last 8 hours instead of 24
  (`security:manage:session-hours`) and are renewed while active. ⚠ Tokens issued before this
  version are invalid: users log in again, clients request a new token.
- Url tokens: work again (`UrlTokenAuthenticationMiddleware` never set the user), are stored
  hashed (`sha256$`, plain text tokens are hashed on first use), listed as `token-name~(hidden)`
  and shown only once after create/recycle. A wrong url token is an *Invalid Token (498)*.
- Logs: tokens, passwords, secrets, url tokens and `Authorization` values are masked in the gView
  service logs (`MaskSecrets`); security events (failed/refused logins, lockouts, refused admin
  requests, admin setup, sign out everywhere, configuration warnings) are logged with the
  category `gView.Server.Security`.
- Limits:
  - WFS `GetFeature` returns at most the `MaxRecordCount` of the service, also without
    `MAXFEATURES` ⚠; `Features2GML` wrote one feature more than the limit.
  - Vector tile glyphs are cached per resolved installed font instead of per requested font
    stack, so any number of font stacks cannot fill the disk.
  - Request bodies up to 50 MB for Kestrel, IIS and forms (`security:max-request-body-mb`).
  - Optional rate limit per client ip for the services (`security:rate-limit`, off by default).
  - Map images (WMS, GeoServices export/legend, ArcXML, GeoJson service, OGC API Maps) are never
    larger than 16384 pixels, even if the service or `mapserver-defaults:maxImageWidth/Height`
    allow more (warning at service load); larger requests are scaled down as before
    (`ImageSizeLimits`). Width/height ≤ 0 are a protocol error, also for WMS `GetFeatureInfo` and
    GeoServices `identify`.
  - Dpi of map requests limited by `mapserver-defaults:maxDpi` (default 600) ⚠, larger values are
    clamped (the image size stays).
  - Capabilities report the real image limits: WMS 1.3.0 `MaxWidth`/`MaxHeight` (was always
    8000), GeoServices and GeoJson service (`maxImageWidth`/`maxImageHeight` were swapped).
- SQL injection through public service parameters closed (also in 8.26.4001, details there):
  where clauses from GeoServices (`where`, `layerDefs`, `definitionExpression`), ArcXML and the
  GeoJsonService are checked against a SQL token allowlist (`SqlWhereClauseValidator`, more
  functions via `security:sql:allowed-functions`); order by, field lists and renderer fields from
  requests only accept fields of the table; WFS filter literals are escaped and parsed; request
  and layer filters are always combined as `(a) AND (b)`. ⚠ `layerDefs` and
  `definitionExpression` with other functions, subqueries or unknown fields are rejected.
- WFS: the KVP `FILTER` parameter is parsed as filter XML instead of being loaded as url or file
  path (also in 8.26.4001).
- WMS `GetFeatureInfo` with `INFO_FORMAT=xsl/{name}.xsl`: only a file name of `a-z 0-9 _ . -`
  in `{etc}/wms/getfeatureinfo/xsl` is accepted, other values (`..`, paths, urls) are an
  `InvalidFormat` error (`WMSParameterDescriptor.GetInfoFormatXslPath`); no more loading of
  arbitrary XML/XSL files on the server. The stylesheet is loaded without DTDs, `xsl:include`/
  `xsl:import`, `document()` and scripts.
- XML input (ArcXML, WFS POST, `SLD_BODY`, SLD, OGC filter/GML) and responses of remote services
  (WMS/WFS/ArcIMS/gView clients) are parsed without DTD processing and without resolving external
  resources (`XmlDocumentExtensions.LoadXmlSafe`, `SafeXml`): no entity expansion
  ("billion laughs"), no XXE.
- Docker images (`gview-server-base`, `gview-server`, `gview-webapps`) run as the non-root user
  `app` (UID 1654); `/app/_config`, `/etc/gview` and `/etc/gview-repository` belong to this user. ⚠ Mounted volumes must
  be writable for UID 1654. The outdated .NET 6 `src/gView.Server/Dockerfile` was removed.
- Error messages: the services no longer pass internals (exception texts with paths, connection
  data, SQL, database messages, stack traces) to clients. `MapServerException` texts (written for
  clients, incl. auth and SQL check errors) are still returned; every other error is answered with
  `internal server error (id: xxxxxxxx)` and the full exception is logged with the same id
  (`ExceptionExtensions`, `ServiceRequest.ErrorId`). ⚠ Clients that showed the error texts now
  show the generic text.
  - GeoServices: no stack trace for `NullReferenceException`, unknown errors are code 500 (was
    999 "unknown error" or 400 without logging), `layerDefs` parser details are no longer
    returned, FeatureServer `applyEdits`/`addFeatures`/... report database errors with the id
    only, `generateToken` errors stay readable.
  - Map images: render exceptions were drawn into the image with message and stack trace; the
    server now draws the generic text and logs the exceptions (`ServiceMap`), Carto still shows
    the details.
  - ArcXML (`<ERROR>` with stack trace), WMS/WMTS/OGC controller, WMTS and tile service tiles
    (error text in the tile), vector tiles, MapServer and VTC controllers, `MapServerInstance`,
    token login page, OGC API (also `OgcApiException` 500 with database messages) and GeoJson
    service (now with id and stack trace in the log).
  - ArcXML and MapServer `<ERROR>` texts are XML escaped.
- Build: new nuke target `Audit` (runs before `Compile`): all projects of server, webapps and
  cmd (read from `project.assets.json`) are checked with
  `dotnet list package --vulnerable --include-transitive`; vulnerable packages from
  `--audit-fail-level` (default `high`) on fail the build, lower ones are warnings, native
  packages (SkiaSharp, HarfBuzzSharp, SQL client SNI) are listed with their versions. Hotfix
  despite a new advisory: `--skip Audit`.

## 9.26.4301

## Added

- Geometry: spatial filters (intersects, contains, within) of data sources without native
  spatial filtering (Shape, GeoJSON, FDB fallback, OGC, event tables, feature cache) can run
  on SpatialNET predicates ([ADR 0002](docs/adr/0002-spatialnet-as-geometry-engine.md)).
  The envelope relations, including the map envelope filter of map drawing and labelling,
  always stay on gView's box test. Opt-in with the environment
  variable `GVIEW_SPATIALNET=relation` (or `all`) or
  `GeometryEngineOptions.UseSpatialNetForSpatialRelation`; off by default. Results follow
  DE-9IM: touching geometries intersect, and a filter polygon whose hole lies inside a
  feature does not contain it.
- Geometry: `Buffer` of points, multipoints, polylines and polygons can run on SpatialNET
  (opt-in with `GVIEW_SPATIALNET=buffer` or `GeometryEngineOptions.UseSpatialNetForBuffer`;
  off by default). Multipoints and polylines are buffered in one pass, and buffers are exact
  in geographic coordinates: the classic buffers round to 0.001 map units (Clipper), so in
  degrees small buffer distances vanish.
- Geometry: merging polygons (`Algorithm.MergePolygons`, `Algorithm.FastMergePolygon`,
  `Polygon.Union` with a polygon) can run on SpatialNET (opt-in with `GVIEW_SPATIALNET=merge`
  or `GeometryEngineOptions.UseSpatialNetForMerge`; off by default): one union of all
  polygons instead of pairwise merging, exact in geographic coordinates.
- Geometry: clipping polygons to an envelope (map highlighting, raster footprints, polygon
  labels, `Clip.PerformClip`, `Polygon.Clip`) can run on SpatialNET (opt-in with
  `GVIEW_SPATIALNET=clip` or `GeometryEngineOptions.UseSpatialNetForClip`; off by default),
  exact in geographic coordinates. Polylines keep gView's own envelope clip.

- GeoServices REST: query operation supports `outStatistics` and `groupByFieldsForStatistics`
  (MapServer and FeatureServer) with `count`, `sum`, `min`, `max`, `avg`, `stddev` and `var`
  (sample formula), several group-by fields, `orderByFields` on group-by fields / statistic
  names, `resultOffset` / `resultRecordCount` on the aggregated rows. Field names are checked
  against the table fields; incompatible parameters (`returnIdsOnly`, `returnCountOnly`,
  `returnDistinctValues`, `returnExtentOnly`) are rejected.
  - New interface `IStatisticsTableClass`: FDB SQLite / PostgreSql / SqlServer, PostGIS,
    SQL Server Spatial and SDE aggregate with a SQL `GROUP BY` (plain attribute filters).
    All other data sources, and spatial / object id filters, are aggregated in memory
    (`StatisticsAggregator`).
  - New map service property `MaxStatisticsRecordScan` (default 1,000,000): max. number of
    records an in-memory aggregation may read.
- GeoServices REST: layers of MapServer and FeatureServer report `supportsStatistics` and
  `supportsAdvancedQueries` (and the same flags in `advancedQueryCapabilities`) for feature
  layers and tables. `advancedQueryCapabilities` is now also part of MapServer layers.
- Carto: map settings offer REST capabilities (Statistics, Pagination, Order By, Distinct,
  Query Extent, Dynamic Legends, Dynamic Layers) per map service and per layer, each
  Auto / On / Off. A layer setting overrides the service setting; capabilities that are
  switched off are also rejected by the query operation.
- New test project `gView.Framework.Data.Tests`.
- GeoServices REST: `dynamicLayers` of the export operation accept more renderers.
  - `uniqueValue` (`ValueMapRenderer`, several fields via `ManyValueMapRenderer`) and
    `classBreaks` (`QuantityRenderer`), both with `defaultSymbol` and legend labels.
  - Symbols: `esriSFS` hatch styles (`HatchSymbol`) and `esriSFSNull`, `esriSLSNull`,
    `esriPMS` with `imageData` (png / jpeg / gif, max. 512 KB; `url` is not supported).
  - `labelingInfo`: `minScale` / `maxScale` (`ScaleDependentLabelRenderer`) and `labelPlacement`
    for points / lines (text alignment).
  - Layer drawingInfo (`classBreaks` for `QuantityRenderer`, hatch styles) is written as well.
  - Unsupported renderer types (`heatmap`, `dotDensity`, ...) no longer hide the layer: the
    renderer of the layer is used.
  - New tests in `gView.Interoperability.GeoServices.Tests`.

## Changed

- Buffer queries (`BufferQueryFilter`) merge all buffers once at the end instead of uniting
  the growing result with every single buffer.

## Fixed

- GeoServices REST: `uniqueValue` drawingInfo no longer contains the internal
  "all other values" key as a value.
- Geometry: `Polygon.Difference` and `Polygon.SymDifference` compute the difference and the
  symmetric difference. Before, both computed the intersection. They now run on SpatialNET
  ([ADR 0002](docs/adr/0002-spatialnet-as-geometry-engine.md)); `SymDifference` with a
  non-polygon throws `NotSupportedException`.
- Geometry: `Polyline.Intersect`, `Difference`, `SymDifference` and `Union` are implemented
  (on SpatialNET) instead of throwing "not implemented". The in-place variants throw
  `NotSupportedException` when the result is not a polyline (e.g. polyline ∩ point); the
  variants with an `out` result return it as it is.
- Geometry: `Polygon.Intersect` with a polygon, polyline or points computes the intersection
  (on SpatialNET). Before, it returned `null` (`out` variant) or changed nothing. With a
  polyline or points, the in-place variant throws `NotSupportedException`. With an envelope
  it clips as before.

## 8.26.4001

## Added

- FDB SQLite: native SpatiaLite / GeoPackage geometry storage. A SQLite FDB file is then
  also a valid SpatiaLite / GeoPackage (opens directly in QGIS / GDAL): `FDB_SHAPE` geometry
  blob + SQLite R-Tree instead of the gView BinaryTree. The New-Dataset dialog offers
  GeoPackage (default) / SpatiaLite / Classic, `CreateDatasetCommand -geometry_storage`
  accepts `SpatiaLite | GeoPackage`, and DataExplorer shows the storage type plus a
  "Rebuild spatial index" tool. The never-released WKB storage type was removed.
- New project `gView.DataSources.GeoPackage`: GeoPackage datasource without `mod_spatialite`
  (GPB geometry + gView-maintained R-Tree).
- FDB: fast `ITableClass2.ExecuteCount` (SQL `COUNT`) for SpatiaLite / GeoPackage / PostGIS /
  SqlServer storage, used by `returnCountOnly` queries.
- GeoServices REST: layer fields report `nullable` (from the database schema, new
  `IField.IsNullable`) and `editable` (layer allows INSERT/UPDATE in the editor module), for
  MapServer and FeatureServer layers.
- GeoServices REST: insert/update accept ISO 8601 date strings (`2024-05-17T10:30:15Z`,
  `…+02:00`) in addition to epoch milliseconds and the existing text formats.
- New test projects `gView.DataSources.PostGIS.Tests` and
  `gView.Interoperability.GeoServices.Tests`.
- gView Server: OGC API - Features (read access) for every map service, new project
  `gView.Interoperability.OgcApi` (docs: `src/docs/*/server/ogcapi.md`).
  - Part 1 Core, Part 2 CRS by Reference, Part 3 Queryables + queryables as query parameters,
    JSON/GeoJSON and HTML, OpenAPI 3.0 definition per service (`/api`, Swagger UI with `?f=html`).
  - URLs `/ogcapi/{folder}/{service}/...` (`_` for the root folder), `/ogcapi` lists all
    services. Feature layers are collections `c{layerId}`, the feature id is the OID.
  - New access rule interface `_ogcapi` (+ `query` for features). Missing rights return real
    `401` (with `WWW-Authenticate: Basic`) / `403` status codes.
  - New marker interface `ISupportsQueryPaging` for feature classes that page natively
    (PostGIS, SpatiaLite, GeoPackage, MSSql, FDB, MongoDb); other sources (e.g. Shape) are
    paged by skipping records.
- New test project `gView.Interoperability.OgcApi.Tests`.
- OGC API - Features: CQL2-Text filter (`filter`, Part 3 Filtering): comparisons (also
  property-property), `AND`/`OR`/`NOT`, `IS NULL`, `LIKE`, `BETWEEN`, `IN`, `CASEI`,
  `DATE(...)`/`TIMESTAMP(...)` and boolean literals. Property filter query parameters use the
  same translation and now also support date and boolean fields.
- OGC API - Maps (Part 1): `/map` (service, `collections=` selects layers) and
  `/collections/{id}/map` with `bbox`/`bbox-crs`, `crs`, `width`/`height`, `scale-denominator`,
  `mm-per-pixel`, `transparent`, `bgcolor`, PNG/JPEG and `Content-Crs`/`Content-Bbox` headers.
  Raster layers are now collections too (map only). New access type `map` for the `_ogcapi` interface.
- OGC API - Features Part 4 (editing): `POST` items (`201` + `Location`), `PUT` (replace),
  `PATCH` (JSON merge patch) and `DELETE` item, `Content-Crs` for the body geometry, `OPTIONS`
  with an `Allow` header from the layer's edit statements and the user's `edit` right (used by
  QGIS). Edits run in an edit session (transaction) if the data source supports it.
- New `FeatureEditService` (`gView.Interoperability.Extensions`): the edit rules of GeoServices
  REST FeatureServer (edit statements, insert/update validation, projection into the feature
  class CRS, geometry cleaning, edit sessions), now shared by GeoServices REST and OGC API.
- New test project `gView.Interoperability.Extensions.Tests`.
- New `MapImageRenderer` (`gView.Interoperability.Extensions`): the map image rendering of
  WMS GetMap, now shared by WMS and OGC API - Maps.
- New interface `ISupportsSqlDialect` (`SqlDialect` PostgreSql / SqlServer / SQLite) on PostGIS,
  MSSql, SpatiaLite, GeoPackage and FDB feature classes, used to write dialect specific date and
  boolean literals.

## Fixed

- PostGIS / GeoServices REST: inserts dropped all attributes of tables with upper-case columns.
  Insert/Update now resolve field names from the table schema (case-insensitive fallback)
  instead of rewriting them per `UseDatasetNameCase`.
- PostGIS / GeoServices REST: date columns.
  - `time`, `timetz`, `datetimeoffset`, `DateOnly` columns are read as date fields (`real` as
    float), all date/time values are returned as epoch milliseconds.
  - Writing dates no longer fails with Npgsql's strict `DateTime.Kind` checks: values are
    adapted to `timestamp` / `timestamptz` / `date` / `time` / `timetz` columns.
  - New PostGIS date fields are created as `timestamp` (was `time`); pgFDB add/alter field
    used the invalid type `datetime`.
- PostGIS: `nullable` is read with `CommandBehavior.KeyInfo` (Npgsql only fills `AllowDBNull`
  then); OGC DB schema reader tolerates `DBNull` in `IsIdentity` and no longer adds a second
  ID field.
- OGC DB `CreateFeatureClass`: `classNameLower/Upper` only changes the table name (columns
  keep their case), `fieldNamesUpper/Lower` is applied; `CREATE TABLE` uses the quoted
  `DbTableName`, `AddGeometryColumn` gets schema and table separately.
- GeoServices REST (QGIS): FeatureServer spatial queries without `inSR` use the map's
  display SRef (returned zero features before); MapServer `fullExtent` is reprojected into
  the display SRef; ExportMap returns `extent.spatialReference` and no longer throws without
  a `layers` parameter.
- FDB SQLite: GeoPackage spatial queries were truncated under `LIMIT`;
  `RepairNativeSpatialIndex` now refills the GeoPackage R-Tree.
- SpatiaLite import: polygons repaired by `ST_MakeValid` keep only their polygon parts (were
  dropped as `GEOMETRYCOLLECTION`).
- FDB import: null geometry checks for PostgreSQL / SQLite / SqlServer.
- DataExplorer: a newly created object appears without a manual refresh.
- Carto: in the Scale Dependent (Label) Renderer and Feature Group Renderer property pages,
  changes made through a modal dialog (e.g. symbol, rotation, label expression) were not
  applied. The renderer items are now only rebuilt when the renderer instance changes, not on
  every re-render. [Issue #45](https://github.com/jugstalt/gview-gis/issues/45)

## Changed

- `gView.Cmd`: exits with `2` on exception and `1` when the command fails (was always `0`);
  exceptions are printed in red.

## Security

- gView.Server: SQL injection through public service parameters closed.
  - Where clauses from requests (GeoServices `where`, `layerDefs` for export/identify,
    `dynamicLayers[].definitionExpression`, ArcXML `where` of GET_FEATURES and GET_IMAGE layer
    queries, GeoJsonService `where`) are checked against an allowlist of SQL tokens
    (`SqlWhereClauseValidator`, replaces the blocklist in `CheckWhereClauseForSqlInjection`):
    fields of the layer's table, string/number/date literals, `NULL`, `TRUE`/`FALSE`, comparison
    operators, `AND OR NOT IN LIKE ESCAPE IS BETWEEN`, parentheses, `+ - * /` and the functions
    `UPPER LOWER CHAR_LENGTH TRIM CAST EXTRACT CURRENT_DATE CURRENT_TIMESTAMP`. Subqueries,
    `UNION`, `;`, comments, variables and other functions are rejected with status 400; the
    message names the rejected token, not the SQL. `1=1` (Esri clients) stays allowed.
  - Additional functions can be allowed in `mapserver.json`:
    `"security": { "sql": { "allowed-functions": [ "ROUND" ] } }`.
  - **Behavior change:** `layerDefs` and `definitionExpression` were not checked before.
    Expressions with other functions, subqueries or unknown fields are now rejected.
  - Order by (GeoServices `orderByFields`, WFS `SortBy`): only `field [ASC|DESC]` with fields of
    the table.
  - Field lists (GeoServices `outFields` and `returnDistinctValues`, ArcXML `subfields`, fields
    used by renderers from GeoServices `dynamicLayers`, ArcXML GET_IMAGE and WMS `SLD_BODY`):
    only fields of the table (field functions like `SHAPE.STArea()` only if the table defines
    that field). The old check could be bypassed with tabs instead of spaces.
  - WFS filter: string literals are quoted with doubled quotes, numbers are parsed, dates and
    booleans validated, property names checked against the feature type. `PropertyIsBetween` and
    `PropertyIsNull` are supported now, `Not` no longer loses the negation.
  - GeoJsonService: string parameters of the attribute filter are quoted with doubled quotes.
  - Request filters and the service's layer filter are always combined as `(a) AND (b)`, so an
    `OR` in the request can no longer bypass the layer filter.
- WFS: the KVP `FILTER` parameter was loaded with `XmlDocument.Load(string)`, i.e. as url or
  file path (server side requests, local files). It is now parsed as filter XML without DTD
  processing and external resources; `<Filter>` with and without the ogc namespace is accepted.
- New test project `gView.Framework.OGC.Tests`.

## 8.26.3602

## Added

- New datasource `gView.DataSources.SpatiaLite` (`SpatiaLiteDataset`, plugin
  `975bcd88-bee4-43ed-a82c-ba10e0b45500`): opens and **edits** stand-alone SpatiaLite
  (`geometry_columns` + SpatiaLite blob) and GeoPackage (`gpkg_*` metadata, handled in
  mod_spatialite "amphibious" mode) files - `*.sqlite` / `*.db` / `*.sqlite3` / `*.gpkg`.
  Built on the shared `OgcSpatialDataset` pipeline like the PostGIS provider, so
  query / insert / update / delete all go through the common code path; single-part
  geometries are promoted (`CastToMultiPolygon` / `-LineString` / `-Point`) into strictly
  typed multi-geometry columns on write. Schema management too: `Create()` makes a new
  empty database (`*.gpkg` &rarr; `gpkgCreateBaseTables()`, otherwise
  `InitSpatialMetaData()`), and `CreateFeatureClass()` / `DeleteFeatureClass()` add and
  remove layers in both formats (GeoPackage via the `gpkgAddGeometryColumn` /
  `gpkgAddGeometryTriggers` / `gpkgAddSpatialIndex` helpers plus the `gpkg_contents` /
  `gpkg_geometry_columns` rows; SpatiaLite via `AddGeometryColumn` / `CreateSpatialIndex`).
  GeoPackage specifics: the geometry column is
  read through `CastAutomagic()` (raw GPB blobs are invisible to `MbrIntersects` /
  `Extent` even in amphibious mode), written through `AsGPB()` so the stored blob stays
  spec-compliant, and the connection sets `PRAGMA trusted_schema=ON` so the GPKG
  RTree / feature-count triggers don't fail edits with "unsafe use of ST_IsEmpty()".
  Awkward field names survive the round-trip: `DbParameterName` sanitises the bind-token
  (OSM-style `mtb:scale:uphill` no longer breaks inserts with `near ":scale": syntax
  error`), and `SelectCommand` builds its column list from `QuerySubFields` instead of a
  space-split so a field name containing a space (`some name`) still reads back.
  - The spatial SQL comes from the native `mod_spatialite` extension, loaded per
    connection via `SpatiaLiteNative`. It is located, in order, from the
    `GVIEW_MOD_SPATIALITE` environment variable, `runtimes/<rid>/native/` next to the
    output, a local QGIS / OSGeo4W install, or `PATH`; on Windows its dependency DLLs
    (geos, proj, …) are pre-loaded from its own folder. The extension itself is **not**
    committed to the repo (see `src/gView.DataSources.SpatiaLite/runtimes/README.md`).
  - Blazor DataExplorer: browse to a SpatiaLite / GeoPackage file and expand it to its
    feature classes (`SpatiaLiteExplorerObject` / `SpatiaLiteFeatureClassExplorerObject`).
    "Create new" makes a fresh database in the current folder (`.gpkg` name &rarr;
    GeoPackage, otherwise SpatiaLite), and the file node is a valid paste target for the
    "Copy / Paste feature class" tool (runs `CopyFeatureClassCommand` in the background).
    `SpatiaLiteDataset` also implements `IFileFeatureDatabase` (with an `Open(connstr)` /
    `GetDataset(connstr)` that accept a file path or a `Data Source=…` string, as the
    CopyFeatureClass parameter builders call them), so pasting a feature class onto a
    plain folder offers "SpatiaLite / GeoPackage" as a target format and prompts for the
    new file name. `IFileFeatureDatabase` gains a defaulted `IsFolderBased` (`true` for
    the existing directory-of-files databases, `false` here) that the Paste tool uses to
    decide whether it needs to ask for a file name. `CreateFeatureClass` no longer emits
    a spurious `UNIQUE constraint failed: spatial_ref_sys.srid` when the SRID is already
    registered.
  - "Create new" ribbon entries are now ordered by `Priority`.
- New test project `gView.DataSources.SpatiaLite.Tests` (needs a real `mod_spatialite`:
  `GVIEW_MOD_SPATIALITE`, QGIS/OSGeo4W, or `libsqlite3-mod-spatialite`).

- GeoServices REST FeatureServer: implemented the `applyEdits` operation so third-party clients
  (QGIS in particular, ArcGIS Pro optionally) can edit through the FeatureService.
  - Layer level `.../FeatureServer/{layerId}/applyEdits` with `adds` / `updates` / `deletes`
    (the operation QGIS uses to commit a layer's changes in one request). `deletes` accepts
    `1,2,3`, `[1,2,3]` and `[{ "objectId": 1 }]`. Honours `rollbackOnFailure` (per-phase:
    adds and updates each run as one transactional database call; deletes are applied
    per objectId).
  - Service level `.../FeatureServer/applyEdits` with an `edits` array of per-layer
    adds/updates/deletes, returning the ArcGIS-style result array (used by ArcGIS Pro).
  - New DTOs `JsonFeatureServerApplyEditsRequestDTO`,
    `JsonFeatureServerApplyEditsServiceRequestDTO`,
    `JsonFeatureServerApplyEditsServiceResultDTO`; new controller actions
    `GeoServicesRestController.FeatureServerApplyEdits` /
    `FeatureServerApplyEditsService` and the matching routes in `Startup.cs`;
    `GeoServicesRestInterperter` methods `ApplyEdits` / `ApplyEditsService` /
    `ApplyEditsToLayer` sharing the existing add/update/delete logic via new
    `GetFeatureClass` / `GetFeatures` / `CheckEditableStatement` overloads.
  - Editable feature layers now advertise an ArcGIS-style `capabilities` string
    (`Query,Create,Update,Delete,Editing`), `supportsRollbackOnFailureParameter` and an
    `Apply Edits` service method; the FeatureService reports `Editing` in its capabilities.
  - `addFeatures` / `applyEdits` now return the generated `objectId` in `addResults`
    (see `## Fixed`), so QGIS keeps the correct server ids without a layer refresh.

## Changed

- `OgcSpatialDataset` (`gView.Framework.OGC.DB`): the query/edit paths now open their
  ADO connections through a single `OpenConnectionAsync()` seam with a
  `OnConnectionOpenedAsync(DbConnection)` provider hook (default: no-op), instead of
  ~15 inline `ProviderFactory.CreateConnection()` calls. Behaviour for the existing
  PostGIS / MS SQL Spatial providers is unchanged; SpatiaLite uses the hook to load
  `mod_spatialite` (and enable GeoPackage amphibious mode) on every connection.

- GeoServices REST Query (`returnIdsOnly=true`): the response now carries an
  `exceededTransferLimit` property, matching current ArcGIS Server. Without `resultRecordCount`
  all object ids are still returned and `exceededTransferLimit` is `false`; with
  `resultRecordCount` (and `resultOffset`) the response is a page and `exceededTransferLimit` is
  `true` when the page is full. New `JsonObjectIdResponseDTO.ExceededTransferLimit`.

## Fixed

- GeoServices REST FeatureServer `applyEdits` `rollbackOnFailure=true` (the default) now really
  rolls back across adds, updates **and** deletes: a single failing edit leaves the layer
  unchanged, matching ArcGIS Server. New opt-in `ISupportsFeatureEditSession` /
  `IFeatureEditSession` (`gView.Framework.Core.FDB`): a provider runs the whole batch on one
  connection + transaction; the interpreter commits it once (a thrown error disposes the
  session and rolls everything back). Service-level `applyEdits` groups its `edits` by target
  dataset and uses one session per dataset. Wired for gView FDB (SQLite / PostgreSQL / SQL
  Server) and `OgcSpatialDataset` (PostGIS / MS SQL Spatial Geometry+Geography / SDE /
  SpatiaLite); the edit methods gained an `...Internal(...)` overload that optionally runs on a
  caller supplied connection + transaction (`ownConnection` / `ownTransaction` flags, per-feature
  bodies unchanged). Providers without session support (Shape, MongoDB, GML) fall back to the
  previous per-phase behaviour.
- GeoServices REST FeatureServer `addFeatures` / `applyEdits`: `addResults` now carry the
  database generated `objectId`. `IFeatureUpdater.Insert(fClass, features, returnIds = false)`
  gained an opt-in `returnIds` flag &mdash; when `true` the new row id is written back onto
  each `IFeature.OID` via `RETURNING` (PostgreSQL / PostGIS / FDB-PostgreSQL),
  `SCOPE_IDENTITY()` (SQL Server Geometry/Geography, FDB-SQL Server) or
  `last_insert_rowid()` (SQLite/SpatiaLite/GeoPackage, FDB-SQLite), and for SDE the id
  assigned before the insert is kept. `false` (the default, used by every existing caller
  such as FdbImport / CopyFeatureClass) keeps the plain insert statement unchanged. A new
  `OgcSpatialDataset.InsertReturnRowIdStatement` hook carries the per-provider SQL and the
  shared `DbCommand.ExecuteInsertAndApplyId` / `ExecuteInsertAndApplyIds` extensions
  (`gView.Framework.Db.Extensions`) run it and write the ids back; the FeatureServer add /
  applyEdits path passes `returnIds: true`. Providers that do not support it (Shape, MongoDB,
  GML) ignore the flag and `objectId` is then omitted from the response.
- Query `returnCountOnly` / GeoJSON `CountOnly`: the count is no longer capped at the
  service `MaxRecordCount`. For feature classes without a fast `ExecuteCount` (FDB, Shape,
  &hellip;) the fallback iterated the cursor with the normal page limit applied, so
  `where=1=1` reported e.g. exactly `1000`. Count queries now run unlimited (from record 1)
  and the cursor loop only counts, without materialising feature DTOs.

## 8.26.3601

## Added

- New `AdvancedLabelRenderer` (`gView.Framework.Cartography.Rendering`): a `SimpleLabelRenderer`
  subclass with an opt-in `ColorExpression` that resolves a per-feature font colour at render
  time, using the same conditional-script (`@@start` / `@@if` / `@@endif` / `@@end`) machinery
  already used for the label text. An empty expression - or one that doesn't resolve to a
  colour for a given feature - falls back to the text symbol's own colour. Adds the (empty by
  default) `SimpleLabelRenderer.HandleAdditionalLabelRendererBehavior` extension point,
  mirroring `ModifyEvaluatedLabel`; the common `SimpleLabelRenderer` path stays unchanged, with
  no per-`Draw()` colour work when a renderer doesn't use it. Shows up as "Advanced Text
  Renderer" in the label-renderer list.
- `ArgbColor.FromString` / `TryFromString`: new colour notations - bare `R,G,B` and `R,G,B,A`
  (0-255 per channel), `rgb:R,G,B`, and `cmyk(C,M,Y,K)` (0-100 per channel).
- MxlUtil ConvertAprx: recovers per-branch text colour from ArcGIS Pro's rich-text
  `<CLR red=.. green=.. blue=..>` tags. When each If/ElseIf/Else branch (VB or Python) or an
  unconditional expression cleanly wraps its output in a `<CLR>` tag, a parallel colour script
  is built alongside the text script and the layer gets an `AdvancedLabelRenderer` instead of a
  `SimpleLabelRenderer`. Replace/append chains still strip `<CLR>` to plain text as before. The
  "does not evaluate `<CLR>` tags" warning no longer fires when the colour is actually
  recovered.
- Blazor Carto: `AdvancedLabelRenderer` property page - `ColorExpression` is editable wherever
  label renderers are configured (embeds the existing `SimpleLabelRenderer` page and adds a
  colour-expression card).
- Blazor Carto: reworked the label-expression editor dialog (`LabelExpressionDialog`, used for
  both the label expression and the new colour expression).
  - Large monospace, resizable editor instead of a 5-line field; Tab inserts spaces.
  - Field buttons and `@@start` / `@@if( )` / `@@endif` / `@@end` / `@@replace( )` snippet
    buttons insert at the cursor; the field list is filterable.
  - Live "Script mode" detection with a structure check for `@@start` on its own first line, a
    missing `@@end`, and unbalanced `@@if(...)` / `@@endif`.
  - Always-visible "Syntax reference" column (no expander to open); for colour expressions an
    extra column lists every format `ArgbColor.TryFromString()` accepts.
  - Dialog widened to the full content width.

## 8.26.3402

## Added

- New `SvgMarkerSymbol` (`gView.Framework.Symbology`): renders point markers from SVG markup
  (a file path, or a `resource:<name>` embedded map resource) instead of a font glyph, so the
  symbol no longer depends on a font being installed on the rendering server. Rasterized via
  the new `IGraphicsEngine.RasterizeSvg` (Skia2x/Skia3x, backed by `Svg.Skia`) and cached per
  symbol instance; the cache only rebuilds when the requested pixel size changes materially
  (~15%), and rasterizes at a size-dependent oversampling factor so rotated symbols stay crisp
  instead of blurring at small on-screen sizes.
  - Thread-safe: `gView.Server` shares layers/renderers/symbols by reference across
    concurrently handled requests against the same map (`RequireClone() => false`), so the
    bitmap cache is an immutable, atomically-swapped snapshot rather than mutated in place -
    avoiding a use-after-dispose race under concurrent rendering.
  - `GdiGraphicsEngine.RasterizeSvg` (no SVG renderer available under GDI+) now returns a
    generic placeholder marker instead of throwing, so a map using the GDI+ engine still
    renders, just without the actual SVG artwork.
- Blazor Carto: new `ResourcePickerDialog` for picking a map resource (used by
  `SvgMarkerSymbol`/`RasterMarkerSymbol`'s `Filename` property) - shows a thumbnail preview per
  resource and filters the list to the extensions relevant for the property being edited (new
  `PropertyDescriptionAttribute.FileExtensions`), instead of listing every map resource
  regardless of type.
- gView.DataExplorer: new `MxlUtil` entry under `Tools` - runs `gView.Cmd.exe --command MxlUtil`
  in-process. A dialog lets you pick one of the available mxl utilities (`MxlDatasets`,
  `MxlToFdb`, `PublishService`, `ConvertAprx`) and fill in that utility's specific parameters;
  the equivalent command line is shown before execution.
- Font provisioning for gView Server: a new `fonts` section in `_config/mapserver.json`
  (`"fonts": { "directories": [ ... ], "install-to-system": false }`) points the server at one
  or more directories of font files (`*.ttf`, `*.otf`, `*.ttc`). At startup those fonts are
  loaded straight into the graphics engine and resolved by family name during rendering - no
  OS font installation, no elevated rights, identical behaviour on Windows and Linux and in
  containers. New `IGraphicsEngine.RegisterFontDirectory` (implemented for Skia2x/Skia3x via an
  own `SKTypeface` registry with weight/slant matching, and for GDI+ via `PrivateFontCollection`);
  the registered families also show up in `GetInstalledFontNames()`. Optional
  `install-to-system` additionally copies the files into the per-user OS font store
  (`~/.local/share/fonts` + `fc-cache` on Linux, per-user `Fonts` + `HKCU` on Windows) on a
  best-effort basis. New font files require a server restart. See
  `src/docs/design/font-provisioning.md`.

## Fixed

- Blazor Carto: `ResourcesPickerPropertyEditor`'s resource-picker dialog was titled
  "Color Gradient" (copy-paste leftover from `ColorGradientPropertyEditor`) instead of
  describing what it actually does.

- MxlUtil ConvertAprx: converted `CIMCharacterMarker` point symbols could render visibly offset
  from their feature's location.
  - `anchorPoint`/`anchorPointUnits`/`offsetX`/`offsetY` were not read from the CIM at all;
    `AprxMapConverter.ConvertCharacterMarker` now maps them onto the resulting
    `TrueTypeMarkerSymbol`'s `HorizontalOffset`/`VerticalOffset`.
  - Added `GetGlyphCenteringCorrectionFraction`, which renders the marker's glyph offscreen
    through the current graphics engine and measures its actual ink bounds, then corrects for
    it. Many ArcGIS Pro marker/dingbat fonts carry bogus ascent/descent metadata that has
    nothing to do with where the glyph is drawn, which previously threw off
    `TrueTypeMarkerSymbol`'s line-metrics-based centering (`StringAlignment.Center`) even when
    the CIM symbol had no anchor point/offset at all.
- MxlUtil ConvertAprx: stopped forcing every converted map to reference scale 1:1000 and map
  units of meters. ArcGIS Pro only ties symbol/text sizes to ground distance when the author
  explicitly sets a reference scale (most maps never do); forcing one made converted symbols
  resize differently than in ArcGIS Pro as soon as the map was viewed at another scale. `CimMap`
  now reads `referenceScale` from the CIM (falling back to disabled, matching ArcGIS Pro's
  default), and map units are derived from the map's resolved spatial reference instead of being
  hardcoded.

## 8.26.3401

## Changed

- `gView.Server.exe` offline commands (`--publish`, `--remove`, `--catalog`, `--get-metadata`,
  `--set-metadata`): `PlugInManager.Init()` now runs with `InitSilent = true`, so it no longer
  prints one "added ..." line per plugin type to the console.

## Fixed

- `gView.Server.exe --publish`: fixed a `"No maps found in document"` error on every publish. An
  `.mxl` file on disk has a `<MapServer>` root wrapping `<MapDocument>`, but
  `MapServiceDeploymentManager.AddMap` expects a bare `<MapDocument>` fragment as its `mapXml`
  argument - the same unwrapping `BrowseServicesController.AddService` already does for HTTP
  uploads. Added `MxlFile.ExtractMapDocumentXmlAsync` and used it in `PublishCommand` before
  calling `AddMap`.

## 8.26.3303

## Added

- `gView.Deploy`: more command-line parameters, so a profile can be deployed fully
  non-interactively. New: `--product`, `-y`/`--yes`, `--download`/`--skip-download`,
  `--confirm`/`--no-confirm`, `-h`/`--help`, `-v`/`--version latest` (resolves to the newest
  locally available version), and one `--<property-name>` flag per deploy-profile property
  (`--repository-path`, `--server-url`, `--admin-username`, `--admin-password`,
  `--carto-username`, `--carto-password`, `--target-installation-path`).
- `gView.Server.exe` offline/CLI commands: services can now be managed without starting the HTTP
  server (no port is bound). The process reads the full server configuration (DI, plugins,
  `ServicesPath`) exactly like a normal server start, then exits with `0` on success or a non-zero
  code (message on `stderr`) on failure:
  - `--publish --mxl <mxl-file> --service <folder/servicename>`: validates the MXL, renames its
    first map to `folder/servicename`, and writes `.mxl`/`.meta` to `ServicesPath`.
  - `--remove --service <folder/servicename>`: deletes `.mxl`/`.svc`/`.meta` of the service from
    `ServicesPath`.
  - `--catalog [--format text|xml|json]`: lists all registered services (root level plus one
    folder level); defaults to text output.
  - `--get-metadata --service <folder/servicename> [--out <path>]`: prints the service's `.meta`
    XML, or writes it to a file with `--out`. An empty result (no service/no metadata) is not an
    error.
  - `--set-metadata --service <folder/servicename> --metadata <xml-file>`: writes the given XML as
    the service's `.meta` and reloads the service.
- `MapServiceDeploymentManager`: `GetMetadata`/`SetMetadata` now have identity-based overloads (in
  addition to the existing user/password ones), and their shared file-I/O logic was factored out
  into private helpers so both the HTTP and the new CLI commands go through the same code path.

## 8.26.3302

## Added

- MxlUtil ConvertAprx: ArcGIS Pro VBScript-like label expressions are now translated into gView
  label expressions instead of always falling back to a warning. Handles:
  - plain string literals and `&`/`+` concatenation of literals, `[Field]` references, and
    `vbNewLine`/`vbCrLf`/`vbTab`
  - `round(...)` calls, mapped onto gView's existing `[Field:F2]`-style format placeholders
  - ArcGIS Pro rich-text formatting tags (`<CLR>`, `<BOL>`, `<ITA>`, `<UND>`, ...), stripped since
    gView label symbols can't render per-run text formatting
  - `Function ... If/ElseIf/[Else] ... End Function` branching, including `and (... or ...)`
    groups and `[Field] = "Value"` / `<> "Value"` comparisons, reduced to gView's
    `@@start/@@if/@@endif/@@end` conditional-line mini-script. Every conditional reduction is
    exhaustively verified against the original VB semantics before being accepted, so an
    unsupported shape always falls back to the previous warning instead of risking wrong output.
  - chained VB `Replace(...)` calls, mapped onto the existing `@@replace(search,replacement)`
    mini-script command
- Testing: added an xUnit test project layout (`src/tests/<Project>.Tests`) and test coverage for
  `SimpleScriptInterpreter`, `AprxLabelExpressionParser`, `AprxMapConverter` and `AprxReader`.

## Fixed

- `SimpleScriptInterpreter`: nested `@@if(...)/@@endif` now uses a proper condition stack instead
  of a single flag, so an inner `@@endif` no longer incorrectly re-enables content that's still
  inside an outer, false `@@if`.
- MxlUtil ConvertAprx: a label's fallback to its first `fieldNames` entry (used when no
  `expression` is set) is now bracketed as `[Field]` before the "is this a plain field reference?"
  check, so a plain field name is no longer misrouted into the expression parser and flagged as a
  "complex" expression.

## 8.26.3102

## Fixed

- PostGIS/MSSqlSpatial/Sde: the generated-feature-id fallback introduced in 8.26.3101 could
  incorrectly kick in for `SdeFeatureClass` and `MSSqlSpatial.Featureclass` even when a perfectly
  valid, numeric id field was found, because they resolve their id field via their own logic
  instead of `OgcSpatialFeatureclass.ReadSchema()` and never updated `HasIntegerIdField`
  accordingly. Both now set the flag correctly, so real database ids are used again where
  available.

## 8.26.3101 (Corrupt, update to 8.26.3102!)

## Fixed

- PostGIS: Feature classes whose id/primary-key column is not numeric (e.g. `varchar`/GUID `gid`,
  or a real primary key of type `oid`) could silently load zero features, without any visible error.
  - Primary-key detection now also recognizes `oid` columns, not just `integer`/`bigint`.
  - If no numeric id column can be found at all, features now load with a generated feature id
    (always negative, so it can never be confused with a real database id) instead of disappearing.
  - Errors that occur while reading rows are no longer swallowed silently.

## 8.26.2205

Using .NET 10

## Added

- WebApps: Usability
  - Carto:
    - Context Menus on TOC Items (Layers/Map)
    - Layer Source (Layer settings): Layer and dataset can changed inside app
    - ...

- Server: GeoServices an serve and edit SDE Layers with Z- and M-Values

- Reading WKB from DB: Support auf Curves. Curves can be read (converted internally to simple lines)

- Labeling Engine:
  - Feature Priority: Features (Geometry of geo-object) can take a priority
    in labeling to avoid label to overlap this features.
  - Some bugfixing in labeling engine: in edge cases an overlapping of 
    labels was possible

- Graphics Engine: Skia upgrade to SkiaSharp 3.x

## 7.x

Using .NET 9
If you run *gView Server* on Microsoft Internet Information Server (IIS) note the following:

If the gView Server ApplicationPool is not running under a system account such as ``LocalSystem``, you must set the ``Load User Profile`` option to ``True`` for this ApplicationPool 
under ``Advanced Settings…`` in the ``Process Model`` section. Otherwise, the application may encounter issues accessing the ``*.pfx`` certificate. 
In that case, the application will terminate with an error message upon startup.

## Unreleased
## Added
## Fixed

## 7.25.4901
## Added

- Toc Ordering in GeoServices Rest Services is the same as in gView.Carto now.

## 7.25.4601
## Added

- WebApps: GaphicsEnginge is configurable via `graphics-engine` in `gview-webapps.config` in the same way as in `mapserver.config`.
  - `gdiplus`
  - `skia` (default) 
  
  ``` javascript
  "graphics": {
			"rendering": "skia"
  }
  ```
## Fixed

## 7.25.4401
## Added

- ``GeoServices Rest``: Implemented `/queryLegends` endpoint to retrieve 
  optimized legends for layers in a map service (only symobls within the current map extent).

## 7.25.3601
## Added

- ``mapserver.config``: Added support for ``CriticalErrorLevel``
  [Online Documentation](https://docs.gviewonline.com/en/setup/config-server.html)

## Fixed

- Bug: Publish services from gView.Carto to gView.Server failed (400: Badrequest)

## 7.25.3301
## Added

- Introduced additional System-Rotation-Types: (geographic - 0 => north, arithmetic - 0 => east)

## 7.25.3201

## Fixed

- Manage Map Datasets Dialog Width: [Issue #40](https://github.com/jugstalt/gview-gis/issues/40)]

## 7.25.1901

### Added

```
**************************************************************
*                                                            *
*                    !! Upgrade to NET 9 !!                  *
*                                                            *
**************************************************************
```

-   Installation of dotnet-9.x-hosting-bundle required on windows servers.

-   **Label Renderer:** Expression supports formatting for `number` and `date` types.
    `[FIELD:0.00]` or `[FIELD:yyyy-MM-dd]` will format the field value.
    [Online Documentation](https://docs.gviewonline.com/en/webapps/carto/labeling.html#simple-text-renderer)

-   **Layer Query Definintion:** supports `Order By`.
    You can now define the order of the features during the rendering by using the `orderBy` property in the layer properties dialog (`Filter`)
    [Online Documentation](https://docs.gviewonline.com/en/webapps/carto/layersettings.html)

-   **Projection:** Parallel coordinate transformations on the fly when using `proj-engine=ManageProj4Parallel`
    [Online Documentation](https://docs.gviewonline.com/en/setup/config-server.html)

-   **Projection:** Intoduced gridshifts for transforming to WGS84.
    (still in beta)

