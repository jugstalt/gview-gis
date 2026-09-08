# Bundled `mod_spatialite` native binaries

> **GeoPackage no longer uses these binaries.** The standalone GeoPackage datasource
> (`gView.DataSources.GeoPackage`) and the GeoPackage FDB storage type read / write the
> GeoPackage geometry (GPB) blob in managed code and maintain the R-Tree themselves, so a
> `*.gpkg` / `*.fdb.gpkg` file opens with **no native library at all**. Only the
> **SpatiaLite** flavor (`*.fdb.sqlite`) and this `gView.DataSources.SpatiaLite` project
> still need `mod_spatialite`.

`gView.DataSources.SpatiaLite` talks to SQLite through the repo's existing
`System.Data.SQLite` / `SourceGear.sqlite3` stack and loads the **`mod_spatialite`**
extension at connection open time to get the spatial SQL functions
(`GeomFromWKB`, `ST_AsBinary`, `ST_Intersects`, `CreateSpatialIndex`, …).

The native extension is **not** on NuGet in a usable cross‑platform form, so it is
vendored here, one folder per runtime identifier:

```
runtimes/
  win-x64/native/    mod_spatialite.dll  + dependency DLLs
  win-arm64/native/  mod_spatialite.dll  + dependency DLLs
  linux-x64/native/  mod_spatialite.so   (+ deps, or rely on the distro package)
  linux-arm64/native/mod_spatialite.so
  osx-x64/native/    mod_spatialite.dylib
  osx-arm64/native/  mod_spatialite.dylib
```

These files are copied next to the build output preserving the
`runtimes/<rid>/native/` layout (see the `.csproj`).

## Where to get the binaries

* **Windows** – take the whole `mod_spatialite-*-win-amd64` release bundle from
  <https://www.gaia-gis.it/gaia-sins/> (or the `mod_spatialite.dll` + siblings
  from an OSGeo4W / QGIS `bin` folder). `mod_spatialite.dll` needs its companion
  DLLs (`geos_c`, `geos`, `proj`, `libxml2`, `iconv-2`, `freexl`, `zlib`, …) in the
  same folder – copy **all** of them.
* **Linux** – `apt install libsqlite3-mod-spatialite` ships
  `/usr/lib/x86_64-linux-gnu/mod_spatialite.so`; either vendor it here or let
  `SpatiaLiteNative` fall back to the system copy on `LD_LIBRARY_PATH`.
* **macOS** – `brew install libspatialite` → `mod_spatialite.dylib`.

## Runtime override

Set `GVIEW_MOD_SPATIALITE` to an absolute path to a `mod_spatialite` library to
bypass the bundled lookup entirely (used by the test project and handy for local
debugging against a QGIS/OSGeo4W install).
