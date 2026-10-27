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

1. **Repairs, without a switch.** SpatialNET implements `Polygon.Difference`, `SymDifference` and `Union` and the missing `Polyline` operations of `ITopologicalOperation`. Polygon ∩ Polyline is split by dimension, which covers clipping lines to areas. `AggregateGeometry` keeps its current behaviour. The corrected `Difference` is a behaviour change and is listed in the changelog.
2. **Replacements, behind switches.** Each area has its own switch in a static `GeometryEngineOptions` class in `gView.Framework.Geometry`, set by the hosts from their configuration (e.g. appsettings in gView.Server). The order is:
   1. `SpatialRelation.Check`, the spatial filter fallback: correctness matters most here, and the seam is small.
   2. Buffer (`Algorithm.PolygonBuffer`/`PolylineBuffer`/`PointBuffer`).
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
- The embedded Clipper and the `Topology` namespace stay until the switches that replace them are on by default. Removing them is a later decision.

## Considered Options

- **Implementing the interfaces on gView's interfaces in `gView.Framework.Core`** (`IPolygon : IPolygon2D`): rejected, because every project referencing Core would depend on SpatialNET, while only the concrete classes in `gView.Framework.Geometry` are ever created by the factory.
- **Wrapper classes or converting into SpatialNET's reference implementation:** rejected, because they cost copies on every call. Implementing the interfaces directly is what SpatialNET is designed for, and gView's model fits it (flat ring list, holes as typed rings).
- **Setting `GeometryFactory.Default` once at startup:** rejected because of startup order across the hosts. It would also make direct SpatialNET calls from other projects possible, which the rules above exclude.
- **One global switch:** rejected, because the areas differ in risk and performance and should become the default independently.
