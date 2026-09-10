---
name: extension-methods
description: Where and how C# extension methods live in this repo (gview-gis) — the Extensions/ folder + Extensions namespace, one <Type>Extensions class per extended type, internal vs public, and preferring them over private helper methods bloating a domain class. Use whenever adding an extension method, refactoring private static helpers into extensions, or deciding where a small reusable helper belongs.
---

# Extension method conventions

Small, reusable glue (tolerant reads, null/empty checks, conversions, formatting, LINQ-ish
helpers) belongs in an **extension class**, not as a growing pile of `private static` helpers
inside a domain class. Keep the domain class focused on its job.

## Layout & naming

- **Folder:** `Extensions/` at the root of the project that needs them
  (`src/<Project>/Extensions/`). DI-registration helpers go one level deeper in
  `Extensions/DependencyInjection/`.
- **One file per extended type**, file name = class name = **`<Type>Extensions`**:
  `DataRowExtensions.cs`, `StringExtensions.cs`, `GeometryExtensions.cs`,
  `EnumerableExtensions.cs`. For DI: `ServicesExtensions` / `ServicesCollectionExtensions`.
- **Namespace:** file-scoped, `<ProjectRootNamespace>.Extensions`
  (e.g. `namespace gView.DataSources.Fdb.Extensions;`). DI helpers use
  `<ProjectRootNamespace>.Extensions.DependencyInjection`.
- **Visibility:**
  - `internal static class` when the helpers are only for their own project — this is the
    default, even for a type as general as `DataRow` or `string`. It keeps the public
    surface small and lets each project keep its own tailored set.
  - `public static class` only when another project genuinely consumes them (the
    `gView.Framework.*` libraries do this).

## Style

Both are fine; match the file you are editing and the project's recent direction:

- **Classic** `this`-parameter static methods — the dominant style across the repo, and the
  right choice for helpers with `out` parameters:

  ```csharp
  namespace gView.DataSources.Fdb.Extensions;

  internal static class DataRowExtensions
  {
      public static bool TryGetDouble(this DataRow row, string column, out double value) { ... }

      public static int GetInt32(this DataRow row, string column, int fallback)
          => row.TryGetDouble(column, out double value) ? (int)value : fallback;
  }
  ```

- **`extension(...)` member blocks** (C# 14) — used in newer framework files such as
  `src/gView.Framework.Geometry/Extensions/GeometryExtensions.cs`:

  ```csharp
  public static class GeometryExtensions
  {
      extension(IGeometry? geometry)
      {
          public bool IsNullOrEmptyGeometry() => geometry switch { ... };
      }
  }
  ```

## What does NOT go here

A helper that only makes sense for one caller and encodes domain knowledge (specific column
names, a particular business rule) stays as a `private` method next to that caller — even if
it is built on top of the generic extensions. Example: `AccessFDB.ReadSpatialIndexBounds`
knows the `SIMinX/SIMinY/SIMaxX/SIMaxY` column names, so it stays private in `Database.cs`
and calls `row.TryGetDouble(...)` internally.

## Tests

If the project has a matching xUnit test project and the extension has real logic (branching,
parsing, edge cases), add cases for it — see the `testing` skill. Trivial one-liners don't
need their own tests.
