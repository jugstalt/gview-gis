OGC API
=======

gView Server publishes every map service as an *OGC API* dataset as well:

* *OGC API - Features* (Part 1 Core, Part 2 CRS by Reference, Part 3 Filtering/Queryables, Part 4 Editing)
* *OGC API - Maps* (Part 1: map images of the service and of single layers)

Clients like QGIS, GDAL/OGR, ArcGIS Pro, OpenLayers or Leaflet can use the services without
WMS/WFS or GeoServices REST.


URLs
----

| Resource | URL |
|---|---|
| All services (link list) | `{server}/ogcapi` |
| Landing page | `{server}/ogcapi/{folder}/{service}` |
| Landing page (service in the root folder) | `{server}/ogcapi/_/{service}` |
| API definition (OpenAPI 3.0) | `.../api` (`?f=html` shows Swagger UI) |
| Conformance classes | `.../conformance` |
| Collections | `.../collections` |
| Collection | `.../collections/{collectionId}` |
| Queryables | `.../collections/{collectionId}/queryables` |
| Features | `.../collections/{collectionId}/items` |
| Feature | `.../collections/{collectionId}/items/{featureId}` |
| Map image of the service | `.../map` |
| Map image of a layer | `.../collections/{collectionId}/map` |

`{server}` is the configured `onlineresource-url`. Every resource is available as JSON
(`?f=json`, GeoJSON for features) and HTML (`?f=html`). Without `f`, browsers get HTML.

Collections
-----------

* Every visible feature and raster layer of the map becomes a collection. The collection id is
  `c{layerId}` (as in WMS/WFS), the title is the layer name in the table of contents.
* Feature layers have items and a map, raster layers only a map.
* Group layers and layers inside *checkbox* group layers are not collections.
* The feature id is the object id (OID) of the feature.
* The properties are the visible fields of the layer (without OID and geometry).

Query parameters for items
--------------------------

| Parameter | Description |
|---|---|
| `limit` | Number of features per page. Default 10, maximum = `MaxRecordCount` of the service |
| `offset` | Paging. Clients follow the `next` link of the response |
| `bbox` | `minx,miny,maxx,maxy` in the axis order of `bbox-crs` |
| `bbox-crs` | CRS of the `bbox`, default CRS84 (lon/lat) |
| `crs` | CRS of the geometries in the response, default CRS84 (lon/lat) |
| `datetime` | Accepted, but ignored (layers have no temporal property yet) |
| `{field}={value}` | Filter by a queryable field. `*` is a wildcard for text fields, e.g. `NAME=Gra*`. Dates as `yyyy-MM-dd`, booleans as `true`/`false` |
| `filter` | CQL2-Text filter (see below) |
| `filter-lang` | Only `cql2-text` (default) |

Queryables are all visible text and number fields of the layer. Date and boolean fields are
queryables only if the SQL dialect of the data source is known (PostGIS, MSSQL, SpatiaLite,
GeoPackage, FDB). Unknown query parameters result in `400 Bad Request`.

CQL2 filter
-----------

`filter` filters features with a CQL2-Text expression (Part 3 Filtering), e.g.

```
filter=GEMEINDE LIKE 'Klagen%' OR (EINWOHNER > 10000 AND BEZIRK IN ('Villach', 'Spittal'))
```

Supported:

* comparisons `= <> < > <= >=` (also between two fields), `AND`, `OR`, `NOT`, parentheses
* `IS [NOT] NULL`, `[NOT] LIKE` (`%`, `_`, escape with `\`), `[NOT] BETWEEN`, `[NOT] IN (...)`
* literals: text `'...'`, numbers, `TRUE`/`FALSE`, `DATE('2024-05-17')`, `TIMESTAMP('2024-05-17T10:30:00Z')`
* `CASEI(...)` for case-insensitive comparisons, e.g. `CASEI(NAME) LIKE CASEI('graz%')`

Timestamps are compared in UTC. Comparisons with `DATE(...)` compare the date only.
Spatial and temporal operators (`S_INTERSECTS`, `T_AFTER`, ...), functions and `cql2-json`
are not supported yet.

Map images (OGC API - Maps)
---------------------------

`.../map` returns a map image of the service with the layers visible by default,
`.../collections/{collectionId}/map` only that one layer. Scale limits of the layers apply as in WMS.

| Parameter | Description |
|---|---|
| `bbox` | Map extent `minx,miny,maxx,maxy` in the axis order of `bbox-crs`. Default: extent of the service or layer |
| `bbox-crs` | CRS of the `bbox`, default CRS84 (lon/lat) |
| `crs` | CRS of the image, default: the CRS of the map |
| `width`, `height` | Image size in pixels. A missing value is calculated from the aspect ratio. Without both: max. 1024 x 768 |
| `scale-denominator` | Scale (instead of `width`/`height`, projected CRS only) |
| `mm-per-pixel` | Display resolution, default 0.28 (about 90 dpi) |
| `transparent` | `true`/`false`, default `false` |
| `bgcolor` | Background color `0xRRGGBB`, default white |
| `collections` | Only for `.../map`: the layers shown, e.g. `collections=c1,c4` |
| `f` | `png` (default) or `jpg`, or via the `Accept` header |

The image size is limited by `MaxImageWidth`/`MaxImageHeight` of the service. The response
contains the headers `Content-Crs` and `Content-Bbox`. Supported CRS are the CRS of the map,
the EPSG codes configured for the *WMS export* and CRS84.

Editing features (Part 4)
-------------------------

Feature layers that are editable in the *editor module* of the map (gView Carto) can be edited
with OGC API. The same rules as for the GeoServices FeatureServer (ArcGIS Pro) apply:

| Method | URL | Edit statement | Response |
|---|---|---|---|
| `POST` | `.../collections/{collectionId}/items` | INSERT | `201` + `Location` of the new feature |
| `PUT` | `.../collections/{collectionId}/items/{featureId}` | UPDATE | `204`, missing properties are set to `null` |
| `PATCH` | `.../collections/{collectionId}/items/{featureId}` | UPDATE | `204`, only the properties/geometry in the body are changed |
| `DELETE` | `.../collections/{collectionId}/items/{featureId}` | DELETE | `204` |

* The body is a GeoJSON feature (`Content-Type: application/geo+json`).
* The CRS of the geometry can be set with the `Content-Crs` header
  (e.g. `<http://www.opengis.net/def/crs/EPSG/0/31256>`), default is CRS84.
* Unknown and not editable properties are ignored. A wrong geometry type results in `400`.
* `OPTIONS` on `items` or `items/{featureId}` returns the allowed methods in the `Allow` header
  (depending on the edit statements of the layer and the `edit` right of the user). QGIS uses it
  to make the layer editable.
* Every change runs in a transaction (if the data source supports it).

Coordinate reference systems
----------------------------

Supported per collection: CRS84, the storage CRS of the data and the EPSG codes configured
for the *WFS export* of the map in gView Carto. CRS are given as URI, e.g.
`http://www.opengis.net/def/crs/EPSG/0/31256`. Coordinates are written in the axis order
of the CRS authority: CRS84 is lon/lat, EPSG:4326 is lat/lon.

Access rights
-------------

The interface is called `ogcapi`. Users need the interface `_ogcapi` (or `_all`)
in the access rules of a service/folder, and additionally:

* metadata (landing page, collections, ...): the interface only
* features (`items`): `query`
* map images (`map`): `map`
* editing (`POST`, `PUT`, `PATCH`, `DELETE`): `edit`

Users can authenticate with HTTP Basic authentication (gView token users), a bearer token
or `?token=...`. Requests without the required rights get `401` (with `WWW-Authenticate`)
or `403`.

QGIS
----

*Layer > Add Layer > Add WFS / OGC API - Features Layer*, create a new connection with the
landing page url, e.g. `https://my.server.com/gview-server/ogcapi/_/myservice`, and select
*Version: OGC API - Features*. For protected services, add a *Basic* authentication.
Editable collections can be edited directly in QGIS (*Toggle Editing*).
