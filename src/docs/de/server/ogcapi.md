OGC API - Features
==================

Der gView Server stellt jeden Kartendienst zusätzlich als *OGC API - Features* Datensatz
bereit (Part 1 Core, Part 2 CRS by Reference, Part 3 Queryables). Clients wie QGIS, GDAL/OGR,
ArcGIS Pro, OpenLayers oder Leaflet können die Features damit ohne WMS/WFS oder GeoServices REST lesen.

> Derzeit nur lesender Zugriff. Editieren (Part 4) und Kartenbilder (OGC API - Maps) folgen.

URLs
----

| Ressource | URL |
|---|---|
| Alle Dienste (Linkliste) | `{server}/ogcapi` |
| Landing Page | `{server}/ogcapi/{ordner}/{dienst}` |
| Landing Page (Dienst im Root-Ordner) | `{server}/ogcapi/_/{dienst}` |
| API-Definition (OpenAPI 3.0) | `.../api` (`?f=html` zeigt Swagger UI) |
| Conformance-Klassen | `.../conformance` |
| Collections | `.../collections` |
| Collection | `.../collections/{collectionId}` |
| Queryables | `.../collections/{collectionId}/queryables` |
| Features | `.../collections/{collectionId}/items` |
| Feature | `.../collections/{collectionId}/items/{featureId}` |

`{server}` ist die konfigurierte `onlineresource-url`. Alle Ressourcen gibt es als JSON
(`?f=json`, für Features GeoJSON) und HTML (`?f=html`). Ohne `f` bekommen Browser HTML.

Collections
-----------

* Jeder sichtbare Feature-Layer der Karte wird eine Collection. Die Collection-Id ist
  `c{layerId}` (wie bei WMS/WFS), der Titel ist der Layername im Inhaltsverzeichnis.
* Gruppenlayer, Rasterlayer und Layer in *Checkbox*-Gruppenlayern sind keine Collections.
* Die Feature-Id ist die Objekt-Id (OID) des Features.
* Die Properties sind die sichtbaren Felder des Layers (ohne OID und Geometrie).

Abfrageparameter für Items
--------------------------

| Parameter | Beschreibung |
|---|---|
| `limit` | Anzahl der Features pro Seite. Standard 10, Maximum = `MaxRecordCount` des Dienstes |
| `offset` | Paging. Clients folgen dem `next`-Link der Antwort |
| `bbox` | `minx,miny,maxx,maxy` in der Achsenreihenfolge von `bbox-crs` |
| `bbox-crs` | Koordinatensystem der `bbox`, Standard CRS84 (lon/lat) |
| `crs` | Koordinatensystem der Geometrien in der Antwort, Standard CRS84 (lon/lat) |
| `datetime` | Wird akzeptiert, aber ignoriert (Layer haben noch keine Zeiteigenschaft) |
| `{feld}={wert}` | Filter auf ein Queryable-Feld. `*` ist bei Textfeldern ein Platzhalter, z.B. `NAME=Gra*`. Datum als `yyyy-MM-dd`, Boolean als `true`/`false` |
| `filter` | CQL2-Text-Filter (siehe unten) |
| `filter-lang` | Nur `cql2-text` (Standard) |

Queryables sind alle sichtbaren Text- und Zahlenfelder des Layers. Datums- und Boolean-Felder
sind nur Queryables, wenn der SQL-Dialekt der Datenquelle bekannt ist (PostGIS, MSSQL,
SpatiaLite, GeoPackage, FDB). Unbekannte Abfrageparameter führen zu `400 Bad Request`.

CQL2-Filter
-----------

Mit `filter` können Features über einen CQL2-Text-Ausdruck gefiltert werden (Part 3 Filtering), z.B.

```
filter=GEMEINDE LIKE 'Klagen%' OR (EINWOHNER > 10000 AND BEZIRK IN ('Villach', 'Spittal'))
```

Unterstützt werden:

* Vergleiche `= <> < > <= >=` (auch zwischen zwei Feldern), `AND`, `OR`, `NOT`, Klammern
* `IS [NOT] NULL`, `[NOT] LIKE` (`%`, `_`, Escape mit `\`), `[NOT] BETWEEN`, `[NOT] IN (...)`
* Literale: Text `'...'`, Zahlen, `TRUE`/`FALSE`, `DATE('2024-05-17')`, `TIMESTAMP('2024-05-17T10:30:00Z')`
* `CASEI(...)` für Vergleiche ohne Groß-/Kleinschreibung, z.B. `CASEI(NAME) LIKE CASEI('graz%')`

Zeitstempel werden in UTC verglichen. Vergleiche mit `DATE(...)` vergleichen nur das Datum.
Räumliche und zeitliche Operatoren (`S_INTERSECTS`, `T_AFTER`, ...), Funktionen und
`cql2-json` werden noch nicht unterstützt.

Koordinatensysteme
------------------

Pro Collection werden unterstützt: CRS84, das Speicher-Koordinatensystem der Daten und die
EPSG-Codes, die in gView Carto für den *WFS-Export* der Karte eingestellt sind. Koordinatensysteme
werden als URI angegeben, z.B. `http://www.opengis.net/def/crs/EPSG/0/31256`. Koordinaten
werden in der Achsenreihenfolge der Authority ausgegeben: CRS84 ist lon/lat, EPSG:4326 ist lat/lon.

Zugriffsrechte
--------------

Die Schnittstelle heißt `ogcapi`. Benutzer brauchen in den Zugriffsregeln eines
Dienstes/Ordners die Schnittstelle `_ogcapi` (oder `_all`) und zusätzlich:

* Metadaten (Landing Page, Collections, ...): nur die Schnittstelle
* Features (`items`): `query`

Anmelden können sich Benutzer mit HTTP Basic Authentication (gView Token-User), einem
Bearer-Token oder `?token=...`. Anfragen ohne ausreichende Rechte bekommen `401`
(mit `WWW-Authenticate`) bzw. `403`.

QGIS
----

*Layer > Layer hinzufügen > WFS / OGC API - Features Layer hinzufügen*, eine neue Verbindung
mit der Landing-Page-URL anlegen, z.B. `https://my.server.com/gview-server/ogcapi/_/meindienst`,
und *Version: OGC API - Features* wählen. Für geschützte Dienste eine *Basic*-Authentifizierung hinterlegen.
