# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

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

