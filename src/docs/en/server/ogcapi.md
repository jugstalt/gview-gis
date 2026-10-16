OGC API - Features
==================

gView Server publishes every map service as an *OGC API - Features* dataset as well
(Part 1 Core, Part 2 CRS by Reference, Part 3 Queryables). Clients like QGIS, GDAL/OGR,
ArcGIS Pro, OpenLayers or Leaflet can read the features without WMS/WFS or GeoServices REST.

> Currently read access only. Editing (Part 4) and map images (OGC API - Maps) will follow.

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

`{server}` is the configured `onlineresource-url`. Every resource is available as JSON
(`?f=json`, GeoJSON for features) and HTML (`?f=html`). Without `f`, browsers get HTML.

Collections
-----------

* Every visible feature layer of the map becomes a collection. The collection id is
  `c{layerId}` (as in WMS/WFS), the title is the layer name in the table of contents.
* Group layers, raster layers and layers inside *checkbox* group layers are not collections.
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

Users can authenticate with HTTP Basic authentication (gView token users), a bearer token
or `?token=...`. Requests without the required rights get `401` (with `WWW-Authenticate`)
or `403`.

QGIS
----

*Layer > Add Layer > Add WFS / OGC API - Features Layer*, create a new connection with the
landing page url, e.g. `https://my.server.com/gview-server/ogcapi/_/myservice`, and select
*Version: OGC API - Features*. For protected services, add a *Basic* authentication.
