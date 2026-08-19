# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

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

