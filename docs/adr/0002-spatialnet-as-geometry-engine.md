---
status: accepted
---

# SpatialNET as geometry engine

gView's topological operations are incomplete or wrong: `Polygon.Difference` and `SymDifference` call `Intersect`, and `Polyline` throws "not implemented" for `Intersect`, `Difference`, `SymDifference` and `Union`. The rest rests on the hand-written `Algorithm` class and an embedded copy of Clipper 6.4.2. SpatialNET is a planar geometry library built for hosts like gView. Its algorithms are extension methods on small read-only interfaces (`IPoint2D`, `IPolyline2D`, `IPolygon2D`, …), and it creates its results through a host-provided factory. It is meant to become the shared geometry core of gView and WebGIS, with gView as its first real host.

So the **concrete classes in `gView.Framework.Geometry`** (`Point`, `Path`, `Ring`, `Hole`, `Polyline`, `Polygon`, `MultiPoint`) implement the SpatialNET interfaces. Indexers are implemented explicitly, because gView's own indexers return `IPath`/`IRing`/`IPoint`. An internal `GViewGeometryFactory` creates gView classes from SpatialNET's coordinates. SpatialNET first **adds** what gView lacks or gets wrong. It replaces working gView code only later, area by area, behind switches.

## Rules

- **Only `gView.Framework.Geometry` references SpatialNET.** It references the NuGet packages with a pinned preview version. The reference cannot be kept private: gView's public geometry classes implement SpatialNET interfaces, so every dependent project needs the assembly to compile. The rule is therefore a convention: other gView projects use gView's own API (`ITopologicalOperation`, `Algorithm`, `SpatialRelation`) and never `using SpatialNET`. If a feature needs a capability gView does not wrap yet (e.g. TIN), the wrapper is added to `gView.Framework.Geometry`.
- **The factory is passed explicitly** on every call (`GViewGeometryFactory.Instance`). gView sets neither `GeometryFactory.Default` nor `Precision.DefaultGridSize`, so there is no dependency on startup order (Server, Carto, MAUI, tests) and no conflict with other code in the same process.
- **The factory** copies `Srs` from the `source` geometry, and creates a `Ring` for each shell and a `Hole` for each hole of a `PolygonPart`. Results therefore need no `VerifyHoles()`.
- **Grid size is automatic.** It is SpatialNET's default, derived from the coordinate magnitude, so it works for degrees and metres without knowing the SRS. A grid derived from `Srs` is only introduced if tests show that the automatic one is too coarse for geographic coordinates.
- **M is not carried.** Every gView `IPoint` has an `M`, and `M = 0` cannot be told apart from "no M". `PointM`/`PointM2`/`PointM3` are no way out: their `M` is an `object` tag, not a measure. So no gView class implements `IPointM`, and the factory reports `SupportsM = false`. Results lose M, as gView's own overlays already do. Z is carried (`IPoint3D` on `Point`).
- **Foreign `IGeometry` implementations** that are not gView's concrete classes are copied into gView classes before calling SpatialNET.

## Phases

1. **Repairs, without a switch.** SpatialNET implements `Polygon.Difference` and `SymDifference` (they computed `Intersect`, and no gView code calls them) and the `Polyline` operations of `ITopologicalOperation` (they threw "not implemented"). Polyline ∩ Polygon gives a polyline, which covers clipping lines to areas. An in-place operation whose result has another dimension than the geometry itself (e.g. polyline ∩ point) throws `NotSupportedException`; the `out` variants return such results. `Polygon.Union`, `Polygon.Intersect` and `AggregateGeometry` keep their current behaviour: union of polygons works today (`BufferQueryFilter` uses it) and is replaced only in phase 2, with merge.
2. **Replacements, behind switches.** Each area has its own switch in the static `GeometryEngineOptions` class in `gView.Framework.Geometry`, off by default. The switches start from the environment variable `GVIEW_SPATIALNET`, a list of areas such as `relation` or `all`. This works the same in every host (Server, WebApps, Cmd tools) without host code. Hosts and tests can also set the properties directly. Settings per host (e.g. appsettings) can be added if operators need the switches permanently. The order is:
   1. `SpatialRelation.Check`, the spatial filter fallback: correctness matters most here, and the seam is small. The filter geometry is prepared once per filter object (`PreparedGeometry`, cached weakly) because `Check` runs for every feature. The two envelope relations are decided first and always classically, before the switch is even read: `SpatialRelationMapEnvelopeIntersects` is the render path (every map draw and label pass, for every feature), and `SpatialRelationEnvelopeIntersects` compares bounding boxes only. Both need nothing but a box test. If the render path is ever moved to SpatialNET, it goes with clip in the last step, with a benchmark. Degenerate envelopes (click points) and aggregate geometries also stay on gView's implementation. Intended differences: SpatialNET follows DE-9IM, so touching geometries intersect, and a filter hole inside the candidate prevents `Contains`, which gView's implementation misses.
   2. Buffer. The switch (`buffer`) sits in the `Buffer()` methods of `Point`, `MultiPoint`, `Polyline` and `Polygon`, so multipoints and polylines are buffered in one pass instead of merging single buffers. Arcs use 16 segments per quarter circle: an angle works in degrees and metres alike, unlike an arc tolerance in map units, and it is close to gView's own 0.1 rad step. gView's conventions stay: points and lines with a distance ≤ 0 give `null`, a polygon with distance 0 is returned itself, a polygon shrunk away gives an empty polygon. `AggregateGeometry.Buffer` benefits through its parts; its final merge stays classic until step 3. The comparison tests revealed that the classic buffers are wrong in geographic coordinates: gView's Clipper integration rounds to a grid of 0.001 map units (`Acc = 1000`), about 100 m in degrees, so polygon buffers below that distance vanish and merged buffers snap to that grid.
   3. Merge and union (`MergePolygons`, `Merge`).
   4. `Clip.PerformClip`, last, because it is in the rendering hot path and works today.

   `Generalize` stays on gView's implementation for now.
3. **A switch becomes on by default** only when all of these hold:
   - Comparison tests old against new on real gView data (degrees and metres) pass, or each difference is explained.
   - For clip and buffer, a benchmark shows that rendering does not get noticeably slower.
   - SpatialNET has shipped at least one release without a breaking change to its interfaces.

## Consequences

- SpatialNET is a pre-release. A breaking change to its interfaces breaks `gView.Framework.Geometry`, but behind the switches it never changes gView's default behaviour.
- Tests go into a new project `src/tests/gView.Framework.Geometry.Tests` (net10.0, xUnit), next to the other test projects. It covers:
  - The adapter round trip: `Ring`/`Hole`, closing point, `Srs`.
  - The repaired operations.
  - The comparisons old against new for each switched area.
- `THIRD-PARTY-NOTICES` gets an entry for SpatialNET (Apache License 2.0) with the first package reference.
- The embedded Clipper and the `Topology` namespace stay until the switches that replace them are on by default. Removing them is a later decision. Until then, everything that runs through Clipper (polygon buffer, `FastMergePolygon`/`Merge`, clipping) is only precise to 0.001 map units, which is too coarse for geographic coordinates.

## Considered Options

- **Implementing the interfaces on gView's interfaces in `gView.Framework.Core`** (`IPolygon : IPolygon2D`): rejected, because every project referencing Core would depend on SpatialNET, while only the concrete classes in `gView.Framework.Geometry` are ever created by the factory.
- **Wrapper classes or converting into SpatialNET's reference implementation:** rejected, because they cost copies on every call. Implementing the interfaces directly is what SpatialNET is designed for, and gView's model fits it (flat ring list, holes as typed rings).
- **Setting `GeometryFactory.Default` once at startup:** rejected because of startup order across the hosts. It would also make direct SpatialNET calls from other projects possible, which the rules above exclude.
- **One global switch:** rejected, because the areas differ in risk and performance and should become the default independently.
