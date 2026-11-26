---
status: accepted
---

# WFS on the feature pipeline of OGC API Features

The WFS of gView (1.0.0 and 1.1.0) had its own code for everything: layer selection, filters, CRS handling and a GML writer that wrote GML 2 elements even for GML 3. Its bugs (type names that didn't resolve, geometries in the data CRS, north-first coordinates for `EPSG:xxxx`) came from that separate path. We rebuilt the WFS (2.0.0 new and primary, 1.1.0 and 1.0.0 kept) **on the same feature pipeline as OGC API Features** instead of fixing the old code, so both interfaces show the same features in the same way and are fixed in one place.

## Decisions

- **Shared pipeline** in `gView.Interoperability.Extensions/Features`: `OgcCollectionLayer` (which layers are feature types/collections, their property fields and queryables), `FeatureReader` (layer filter query, paging, CRS transformation, counting), `CrsReference` and `GeoJsonWriter`. A WFS feature type `gv:c{layerId}` is the OGC API collection `c{layerId}`.
- **One filter language:** Filter Encoding 1.x and FES 2.0 are parsed into the CQL2 syntax tree of OGC API and written as SQL by its `CqlSqlTranslator`. Property names are resolved against the queryables and literals are typed by the property, so no filter text reaches SQL. A gView query has only one spatial filter, so a spatial operator is only allowed at the top level or in a top-level `And` (anything else is `OptionNotSupported`).
- **Axis order by notation** (GeoServer convention, expected by QGIS and ArcGIS): WFS 1.0.0 is always x/y. For 1.1.0 and 2.0.0 `EPSG:xxxx` is x/y, `urn:ogc:def:crs:EPSG::xxxx` and `http://www.opengis.net/def/crs/EPSG/0/xxxx` use the axis order of the authority (lat/lon for EPSG:4326, north first for EPSG:31256). The same rule applies to output, `BBOX` and filter geometries; the srsName of a response echoes the notation of the request. GeoJSON is always x/y. OGC API keeps its own rule (always the authority's order, CRS84 lon/lat).
- **Read only:** no WFS-T, locking or own stored queries (only the mandatory `GetFeatureById`). Editing is done with OGC API Features and GeoServices REST, which already have access rights and ETags.
- **Not on the old code:** the old `Filter.cs`, `FeatureTranslator` and the XSD generated classes stay only for the WFS client data source and WMS GetFeatureInfo.

## Consequences

- WFS and OGC API expose the same layers: only visible TOC layers, without layers inside checkbox group layers, and the object id is the `gml:id` (`c{layerId}.{objectId}`), not an attribute. Type names `wfs:c0` of former capabilities still resolve.
- Date and boolean properties can only be filtered for data sources with a known SQL dialect (same as OGC API).
- `numberMatched` costs a count query; it can be switched off per server (`mapserver-defaults:wfsNumberMatched`) or per service (WFS export metadata `NumberMatched`).
- Responses are built in memory (`numberReturned` is an attribute of the root element), not streamed to the client.
