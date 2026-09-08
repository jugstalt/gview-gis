# SpatiaLite support (`mod_spatialite`)

gView can store the geometry of a **SQLite feature database** in two open, non‑proprietary
formats:

| Flavor | File | Native library needed? |
|---|---|---|
| **GeoPackage** | `*.fdb.gpkg` | **No** – gView reads/writes the GeoPackage geometry blob and maintains the R‑Tree itself. |
| **SpatiaLite** | `*.fdb.sqlite` | **Yes** – the native `mod_spatialite` SQLite extension. |

> If you just want an open, portable SQLite feature database, **use GeoPackage
> (`*.fdb.gpkg`)** – it works out of the box with no extra install and opens directly in
> QGIS / GDAL / ArcGIS.

The SpatiaLite flavor (and the standalone *SpatiaLite* datasource, `*.sqlite` / `*.db`)
load the native **`mod_spatialite`** extension at runtime. It is **not shipped with gView**
because of its native dependency chain and licensing. If it is missing you get an error
that explains this and points here.

The standalone *GeoPackage* datasource (`*.gpkg`) and the GeoPackage feature-database
storage (`*.fdb.gpkg`) never load `mod_spatialite` - they are fully managed.

## How gView looks for `mod_spatialite`

In this order:

1. The environment variable **`GVIEW_MOD_SPATIALITE`** – set it to the full path of a
   `mod_spatialite` library (`.dll` / `.so` / `.dylib`).
2. `runtimes/<rid>/native/` next to the gView executable, e.g.
   `runtimes/win-x64/native/mod_spatialite.dll` (with **all** its dependency libraries in
   the same folder).
3. The plain library name handed to the OS loader (found via `PATH` / `LD_LIBRARY_PATH`).

## Getting the binaries

### Windows

Download the whole **`mod_spatialite-*-win-amd64`** bundle from
<https://www.gaia-gis.it/gaia-sins/> (or copy `mod_spatialite.dll` **and its siblings** –
`geos_c`, `geos`, `proj`, `libxml2`, `iconv-2`, `zlib`, … – from an OSGeo4W / QGIS `bin`
folder). Put the folder on `PATH`, or point `GVIEW_MOD_SPATIALITE` at the `.dll`, or copy
everything into `runtimes/win-x64/native/`.

`mod_spatialite.dll` does **not** work without its companion DLLs in the same directory.

### Linux

```
apt install libsqlite3-mod-spatialite      # Debian / Ubuntu
```

installs `/usr/lib/<arch>/mod_spatialite.so`, which the OS loader finds automatically.

### macOS

```
brew install libspatialite
```

## Licensing note

`mod_spatialite` and its dependency stack contain LGPL (SpatiaLite, GEOS, libiconv) and, in
the default gaia‑gis build, GPL components (`librttopo`, older `freexl`). gView itself is
Apache‑2.0 and only *loads* the extension at runtime; if you redistribute the binaries you
must ship the corresponding licenses/source. Building `mod_spatialite` with
`--disable-rttopo --disable-freexl` removes the GPL parts (topology functions and `.xls`
import, neither of which gView's feature‑database storage uses).
