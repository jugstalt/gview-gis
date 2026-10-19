OGC API - Examples
==================

This page shows how to call the OGC API of a gView Server map service with `curl`.
See [OGC API](../src/docs/en/server/ogcapi.md) for the reference of all resources and parameters.

The example service
-------------------

All examples use a fictional water supply service of a city, published from gView Carto
into the folder `city` with the name `waterworks`:

* Server: `https://maps.example.com/gview-server`
* Landing page: `https://maps.example.com/gview-server/ogcapi/city/waterworks`
* Map CRS: ETRS89 / UTM zone 33N (`EPSG:25833`), the data is stored in the same CRS
* WMS/WFS export CRS (configured in gView Carto): `EPSG:25833`, `EPSG:3857`, `EPSG:4326`

| Collection | Layer | Type | Editable |
|---|---|---|---|
| `c0` | Hydrants | point | INSERT, UPDATE, DELETE |
| `c1` | Water mains | line | - |
| `c2` | Pressure zones | polygon | - |
| `c3` | Orthophoto | raster (map only) | - |

The hydrants (`c0`) have these fields:

| Field | Type | Example |
|---|---|---|
| `HYD_ID` | text | `H-1043` |
| `HYD_TYPE` | text | `Overground`, `Underground` |
| `DISTRICT` | text | `Lend`, `Gries`, `Geidorf` |
| `DIAMETER` | integer | `80`, `100`, `150` |
| `FLOW_RATE` | number (l/s) | `13.3` |
| `LAST_INSPECTION` | date | `2024-05-17` |
| `ACTIVE` | boolean | `true` |

To keep the examples short, they use these shell variables:

```bash
OGC=https://maps.example.com/gview-server/ogcapi/city/waterworks
CRS84=http://www.opengis.net/def/crs/OGC/1.3/CRS84
UTM33=http://www.opengis.net/def/crs/EPSG/0/25833
```

> Coordinates are always written in the axis order of the CRS: CRS84 is longitude/latitude,
> `EPSG:4326` is latitude/longitude, `EPSG:25833` and `EPSG:3857` are easting/northing.

Exploring the service
---------------------

```bash
# all services of the server (links to the landing pages)
curl "https://maps.example.com/gview-server/ogcapi?f=json"

# landing page, conformance classes and the OpenAPI definition
curl "$OGC?f=json"
curl "$OGC/conformance?f=json"
curl "$OGC/api?f=json"

# the collections (layers) with their extent and supported CRS
curl "$OGC/collections?f=json"
curl "$OGC/collections/c0?f=json"

# the fields that can be used in filters
curl "$OGC/collections/c0/queryables?f=json"
```

Every resource is also available as HTML, e.g. open `$OGC/collections?f=html` in a browser.
`$OGC/api?f=html` shows the API in Swagger UI, where all requests can be tried out.

Maps
----

### The whole service

Without parameters, the map shows the layers that are visible by default in the
full extent of the service, in the map CRS (`EPSG:25833`), max. 1024 x 768 pixels:

```bash
curl -o waterworks.png "$OGC/map"
```

The response headers tell the CRS and the extent of the image:

```
Content-Type: image/png
Content-Crs: <http://www.opengis.net/def/crs/EPSG/0/25833>
Content-Bbox: 526000,5205000,541000,5220000
```

### Choosing the layers

`collections` sets the layers shown on the map (the order of the map, not of the parameter):

```bash
# orthophoto with the water mains and hydrants on top
curl -o mains.png "$OGC/map?collections=c3,c1,c0"

# only the pressure zones, as a transparent overlay for another base map
curl -o zones.png "$OGC/map?collections=c2&transparent=true"

# a single layer: the map resource of the collection
curl -o hydrants.png "$OGC/collections/c0/map"
```

### Extent, size and CRS

```bash
# the inner city of Graz, extent in CRS84 (lon/lat), 800 pixels wide
# (the height is calculated from the aspect ratio of the bbox)
curl -o center.png "$OGC/map?bbox=15.425,47.062,15.452,47.078&width=800"

# the same extent in UTM 33N, image 1200 x 800 pixels
curl -o center_utm.png "$OGC/map?bbox=532300,5212600,534400,5214400&bbox-crs=$UTM33&width=1200&height=800"

# a map in Web Mercator (e.g. as an overlay for OpenStreetMap)
curl -o center_3857.png "$OGC/map?bbox=15.425,47.062,15.452,47.078&crs=http://www.opengis.net/def/crs/EPSG/0/3857&transparent=true"

# scale 1:5000 instead of width/height (projected CRS only)
curl -o scale5000.png "$OGC/map?bbox=532300,5212600,534400,5214400&bbox-crs=$UTM33&scale-denominator=5000"

# print resolution (0.0847 mm/pixel = 300 dpi) and a light grey background
curl -o print.png "$OGC/map?bbox=532300,5212600,534400,5214400&bbox-crs=$UTM33&width=3000&mm-per-pixel=0.0847&bgcolor=0xF0F0F0"

# JPEG instead of PNG (f=jpg or the Accept header)
curl -o ortho.jpg "$OGC/collections/c3/map?bbox=15.425,47.062,15.452,47.078&f=jpg"
curl -o ortho.jpg -H "Accept: image/jpeg" "$OGC/collections/c3/map?bbox=15.425,47.062,15.452,47.078"
```

Scale limits of the layers apply as in WMS: the hydrants may not be drawn on an overview map.

### In a web map

The map resource can be used as an image layer in any web map client, e.g. with
[OpenLayers](https://openlayers.org/):

```javascript
import ImageLayer from 'ol/layer/Image.js';
import ImageStatic from 'ol/source/ImageStatic.js';

// a static image of the pressure zones for a fixed extent (EPSG:3857)
const extent = [1717000, 5949500, 1720000, 5952500];
const zones = new ImageLayer({
  source: new ImageStatic({
    url: 'https://maps.example.com/gview-server/ogcapi/city/waterworks/map'
       + '?collections=c2&transparent=true'
       + '&crs=http://www.opengis.net/def/crs/EPSG/0/3857&bbox-crs=http://www.opengis.net/def/crs/EPSG/0/3857'
       + '&bbox=' + extent.join(',') + '&width=1200',
    imageExtent: extent
  })
});
```

Querying features
-----------------

### Paging

```bash
# the first 10 hydrants (GeoJSON, CRS84)
curl "$OGC/collections/c0/items"

# 100 per page, the second page
curl "$OGC/collections/c0/items?limit=100&offset=100"
```

The response contains the features, the number of returned features and links. Clients follow
the `next` link until there is none:

```json
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "id": 1043,
      "geometry": { "type": "Point", "coordinates": [15.4395, 47.0707] },
      "properties": {
        "HYD_ID": "H-1043",
        "HYD_TYPE": "Overground",
        "DISTRICT": "Lend",
        "DIAMETER": 100,
        "FLOW_RATE": 13.3,
        "LAST_INSPECTION": "2024-05-17T00:00:00",
        "ACTIVE": true
      }
    }
  ],
  "links": [
    { "href": "…/collections/c0/items?limit=100&offset=100&f=json", "rel": "self", "type": "application/geo+json" },
    { "href": "…/collections/c0/items?limit=100&offset=200&f=json", "rel": "next", "type": "application/geo+json" }
  ],
  "timeStamp": "2026-10-19T08:15:00Z",
  "numberReturned": 100
}
```

A `limit` above the maximum record count of the service is reduced to the maximum.

### A single feature

```bash
curl "$OGC/collections/c0/items/1043"
```

### Spatial filter and CRS

```bash
# hydrants in the inner city (bbox in CRS84)
curl "$OGC/collections/c0/items?bbox=15.425,47.062,15.452,47.078&limit=500"

# the same bbox in UTM 33N, and the geometries in UTM 33N as well
curl "$OGC/collections/c0/items?bbox=532300,5212600,534400,5214400&bbox-crs=$UTM33&crs=$UTM33"

# geometries in EPSG:4326 => coordinates are latitude/longitude!
curl "$OGC/collections/c0/items/1043?crs=http://www.opengis.net/def/crs/EPSG/0/4326"
```

Responses with `crs` contain the header `Content-Crs: <http://www.opengis.net/def/crs/EPSG/0/25833>`.

### Simple property filters

Every queryable field can be used as a query parameter. `*` is a wildcard for text fields:

```bash
# all underground hydrants
curl "$OGC/collections/c0/items?HYD_TYPE=Underground"

# underground hydrants with a diameter of 150 mm in the district Lend
curl "$OGC/collections/c0/items?HYD_TYPE=Underground&DIAMETER=150&DISTRICT=Lend"

# hydrant ids starting with "H-10"
curl "$OGC/collections/c0/items?HYD_ID=H-10*"

# inactive hydrants, hydrants inspected on a date
curl "$OGC/collections/c0/items?ACTIVE=false"
curl "$OGC/collections/c0/items?LAST_INSPECTION=2024-05-17"
```

### CQL2 filters

More complex queries use a CQL2-Text expression in the `filter` parameter. Let `curl`
URL-encode the expression with `-G --data-urlencode`:

```bash
# hydrants with a diameter of at least 100 mm and a low flow rate
curl -G "$OGC/collections/c0/items" \
     --data-urlencode "filter=DIAMETER >= 100 AND FLOW_RATE < 10"

# underground hydrants in two districts
curl -G "$OGC/collections/c0/items" \
     --data-urlencode "filter=HYD_TYPE = 'Underground' AND DISTRICT IN ('Lend', 'Gries')"

# not inspected since 2023 (or never)
curl -G "$OGC/collections/c0/items" \
     --data-urlencode "filter=LAST_INSPECTION < DATE('2023-01-01') OR LAST_INSPECTION IS NULL"

# inspected in a period (timestamps are compared in UTC)
curl -G "$OGC/collections/c0/items" \
     --data-urlencode "filter=LAST_INSPECTION BETWEEN TIMESTAMP('2024-01-01T00:00:00Z') AND TIMESTAMP('2024-06-30T23:59:59Z')"

# district names starting with "ge", case-insensitive (Geidorf, ...)
curl -G "$OGC/collections/c0/items" \
     --data-urlencode "filter=CASEI(DISTRICT) LIKE CASEI('ge%')"

# active hydrants, except the overground ones with a small diameter
curl -G "$OGC/collections/c0/items" \
     --data-urlencode "filter=ACTIVE = TRUE AND NOT (HYD_TYPE = 'Overground' AND DIAMETER < 80)"

# filter and bbox together, 200 per page, as UTM 33N
curl -G "$OGC/collections/c0/items" \
     --data-urlencode "filter=DIAMETER = 150" \
     --data-urlencode "bbox=15.425,47.062,15.452,47.078" \
     --data-urlencode "limit=200" \
     --data-urlencode "crs=$UTM33"
```

Unknown fields, wrong value types or syntax errors result in `400 Bad Request`:

```json
{
  "code": "InvalidParameterValue",
  "description": "Invalid CQL2 filter: can't compare number with text"
}
```

Editing features
----------------

Only the hydrants (`c0`) are editable. Editing requires the `edit` right for the `_ogcapi`
interface of the service. The examples log in with HTTP Basic authentication as the
(fictional) token user `editor`:

```bash
AUTH="editor:secret"
```

### Which methods are allowed?

```bash
curl -i -X OPTIONS -u $AUTH "$OGC/collections/c0/items"
# Allow: GET, OPTIONS, POST

curl -i -X OPTIONS -u $AUTH "$OGC/collections/c0/items/1043"
# Allow: GET, OPTIONS, PUT, PATCH, DELETE

curl -i -X OPTIONS -u $AUTH "$OGC/collections/c1/items"
# Allow: GET, OPTIONS   (the water mains are not editable)
```

### Create a feature

`POST` a GeoJSON feature to the items. The response is `201 Created`, the `Location` header
contains the url of the new feature:

```bash
curl -i -X POST -u $AUTH "$OGC/collections/c0/items" \
     -H "Content-Type: application/geo+json" \
     --data '{
       "type": "Feature",
       "geometry": { "type": "Point", "coordinates": [15.4412, 47.0731] },
       "properties": {
         "HYD_ID": "H-2001",
         "HYD_TYPE": "Underground",
         "DISTRICT": "Lend",
         "DIAMETER": 100,
         "ACTIVE": true
       }
     }'
```

```
HTTP/1.1 201 Created
Location: https://maps.example.com/gview-server/ogcapi/city/waterworks/collections/c0/items/2001
```

With the `Content-Crs` header, the geometry can be sent in any supported CRS, e.g. UTM 33N:

```bash
curl -i -X POST -u $AUTH "$OGC/collections/c0/items" \
     -H "Content-Type: application/geo+json" \
     -H "Content-Crs: <$UTM33>" \
     --data '{
       "type": "Feature",
       "geometry": { "type": "Point", "coordinates": [533512.4, 5213871.9] },
       "properties": { "HYD_ID": "H-2002", "HYD_TYPE": "Overground", "DIAMETER": 80 }
     }'
```

The geometry is projected into the CRS of the data. Properties that are not editable
(e.g. the object id) or unknown are ignored.

### Change properties or the geometry (PATCH)

`PATCH` only changes what is in the body (JSON merge patch):

```bash
# record an inspection
curl -i -X PATCH -u $AUTH "$OGC/collections/c0/items/2001" \
     -H "Content-Type: application/merge-patch+json" \
     --data '{ "type": "Feature", "properties": { "LAST_INSPECTION": "2026-10-19", "FLOW_RATE": 12.5 } }'

# move the hydrant, the properties are not changed
curl -i -X PATCH -u $AUTH "$OGC/collections/c0/items/2001" \
     -H "Content-Type: application/geo+json" \
     --data '{ "type": "Feature", "geometry": { "type": "Point", "coordinates": [15.4415, 47.0733] } }'

# clear a value
curl -i -X PATCH -u $AUTH "$OGC/collections/c0/items/2001" \
     -H "Content-Type: application/merge-patch+json" \
     --data '{ "type": "Feature", "properties": { "FLOW_RATE": null } }'
```

The response is `204 No Content`.

### Replace a feature (PUT)

`PUT` replaces the whole feature: the geometry is required, properties that are missing in the
body are set to `null`:

```bash
curl -i -X PUT -u $AUTH "$OGC/collections/c0/items/2001" \
     -H "Content-Type: application/geo+json" \
     --data '{
       "type": "Feature",
       "id": 2001,
       "geometry": { "type": "Point", "coordinates": [15.4415, 47.0733] },
       "properties": {
         "HYD_ID": "H-2001",
         "HYD_TYPE": "Overground",
         "DISTRICT": "Lend",
         "DIAMETER": 150,
         "FLOW_RATE": 18.0,
         "LAST_INSPECTION": "2026-10-19",
         "ACTIVE": true
       }
     }'
```

### Delete a feature

```bash
curl -i -X DELETE -u $AUTH "$OGC/collections/c0/items/2002"
# HTTP/1.1 204 No Content
```

### Errors

| Situation | Status |
|---|---|
| Not logged in / no `edit` right | `401` (with `WWW-Authenticate`) / `403` |
| The feature does not exist | `404` |
| The layer doesn't allow the operation (e.g. `POST` to `c1`) | `405` |
| Wrong geometry type (a line for the hydrants), no geometry on `POST`/`PUT`, invalid value | `400` |
| Body is not JSON (e.g. form data) | `415` |

```bash
curl -X POST -u $AUTH "$OGC/collections/c0/items" \
     -H "Content-Type: application/geo+json" \
     --data '{ "type": "Feature", "geometry": { "type": "LineString", "coordinates": [[15.44, 47.07], [15.45, 47.08]] }, "properties": {} }'
```

```json
{
  "code": "InvalidParameterValue",
  "description": "Invalid geometry type for a Point collection"
}
```

### QGIS

QGIS edits OGC API collections directly: add the connection
`https://maps.example.com/gview-server/ogcapi/city/waterworks` (*WFS / OGC API - Features*,
version *OGC API - Features*) with a *Basic* authentication for the user `editor`, add the
hydrants layer and use *Toggle Editing*. QGIS sends the same `POST`, `PUT`, `PATCH` and
`DELETE` requests as shown above.
