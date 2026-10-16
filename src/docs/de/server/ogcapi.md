OGC API
=======

Der gView Server stellt jeden Kartendienst zusätzlich als *OGC API* Datensatz bereit:

* *OGC API - Features* (Part 1 Core, Part 2 CRS by Reference, Part 3 Filtering/Queryables, Part 4 Editieren)
* *OGC API - Maps* (Part 1: Kartenbilder des Dienstes und einzelner Layer)

Clients wie QGIS, GDAL/OGR, ArcGIS Pro, OpenLayers oder Leaflet können die Dienste damit ohne
WMS/WFS oder GeoServices REST nutzen.


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
| Kartenbild des Dienstes | `.../map` |
| Kartenbild eines Layers | `.../collections/{collectionId}/map` |

`{server}` ist die konfigurierte `onlineresource-url`. Alle Ressourcen gibt es als JSON
(`?f=json`, für Features GeoJSON) und HTML (`?f=html`). Ohne `f` bekommen Browser HTML.

Collections
-----------

* Jeder sichtbare Feature- und Rasterlayer der Karte wird eine Collection. Die Collection-Id ist
  `c{layerId}` (wie bei WMS/WFS), der Titel ist der Layername im Inhaltsverzeichnis.
* Feature-Layer haben Items und ein Kartenbild, Rasterlayer nur ein Kartenbild.
* Gruppenlayer und Layer in *Checkbox*-Gruppenlayern sind keine Collections.
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

Kartenbilder (OGC API - Maps)
-----------------------------

`.../map` liefert ein Kartenbild des Dienstes mit den standardmäßig sichtbaren Layern,
`.../collections/{collectionId}/map` nur den einen Layer. Maßstabsgrenzen der Layer gelten wie beim WMS.

| Parameter | Beschreibung |
|---|---|
| `bbox` | Kartenausschnitt `minx,miny,maxx,maxy` in der Achsenreihenfolge von `bbox-crs`. Standard: Ausdehnung des Dienstes bzw. Layers |
| `bbox-crs` | Koordinatensystem der `bbox`, Standard CRS84 (lon/lat) |
| `crs` | Koordinatensystem des Bildes, Standard: Koordinatensystem der Karte |
| `width`, `height` | Bildgröße in Pixel. Fehlt ein Wert, wird er aus dem Seitenverhältnis berechnet. Ohne beide: max. 1024 x 768 |
| `scale-denominator` | Maßstab (statt `width`/`height`, nur projizierte Koordinatensysteme) |
| `mm-per-pixel` | Darstellungsauflösung, Standard 0.28 (ca. 90 dpi) |
| `transparent` | `true`/`false`, Standard `false` |
| `bgcolor` | Hintergrundfarbe `0xRRGGBB`, Standard weiß |
| `collections` | Nur für `.../map`: die angezeigten Layer, z.B. `collections=c1,c4` |
| `f` | `png` (Standard) oder `jpg`, alternativ über den `Accept`-Header |

Die Bildgröße wird durch `MaxImageWidth`/`MaxImageHeight` des Dienstes begrenzt. Die Antwort
enthält die Header `Content-Crs` und `Content-Bbox`. Unterstützte Koordinatensysteme sind das
der Karte, die für den *WMS-Export* eingestellten EPSG-Codes und CRS84.

Features editieren (Part 4)
---------------------------

Feature-Layer, die im *Editor-Modul* der Karte (gView Carto) als editierbar eingetragen sind,
können über OGC API bearbeitet werden. Es gelten dieselben Regeln wie für den GeoServices
FeatureServer (ArcGIS Pro):

| Methode | URL | Edit-Statement | Antwort |
|---|---|---|---|
| `POST` | `.../collections/{collectionId}/items` | INSERT | `201` + `Location` des neuen Features |
| `PUT` | `.../collections/{collectionId}/items/{featureId}` | UPDATE | `204`, fehlende Properties werden `null` |
| `PATCH` | `.../collections/{collectionId}/items/{featureId}` | UPDATE | `204`, nur übergebene Properties/Geometrie werden geändert |
| `DELETE` | `.../collections/{collectionId}/items/{featureId}` | DELETE | `204` |

* Der Body ist ein GeoJSON-Feature (`Content-Type: application/geo+json`).
* Das Koordinatensystem der Geometrie kann mit dem Header `Content-Crs` angegeben werden
  (z.B. `<http://www.opengis.net/def/crs/EPSG/0/31256>`), Standard ist CRS84.
* Unbekannte und nicht editierbare Properties werden ignoriert. Ein falscher Geometrietyp führt zu `400`.
* `OPTIONS` auf `items` bzw. `items/{featureId}` liefert im `Allow`-Header die erlaubten Methoden
  (abhängig von den Edit-Statements des Layers und dem `edit`-Recht des Benutzers). QGIS nutzt das,
  um den Layer editierbar zu machen.
* Jede Änderung läuft (wenn die Datenquelle das unterstützt) in einer Transaktion.

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
* Kartenbilder (`map`): `map`
* Editieren (`POST`, `PUT`, `PATCH`, `DELETE`): `edit`

Anmelden können sich Benutzer mit HTTP Basic Authentication (gView Token-User), einem
Bearer-Token oder `?token=...`. Anfragen ohne ausreichende Rechte bekommen `401`
(mit `WWW-Authenticate`) bzw. `403`.

QGIS
----

*Layer > Layer hinzufügen > WFS / OGC API - Features Layer hinzufügen*, eine neue Verbindung
mit der Landing-Page-URL anlegen, z.B. `https://my.server.com/gview-server/ogcapi/_/meindienst`,
und *Version: OGC API - Features* wählen. Für geschützte Dienste eine *Basic*-Authentifizierung hinterlegen.
Editierbare Collections können in QGIS direkt bearbeitet werden (*Bearbeitungsmodus umschalten*).
