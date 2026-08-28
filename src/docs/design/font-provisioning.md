# Design: Font provisioning for gView Server

Status: **Implemented** &nbsp;·&nbsp; Scope: `gView.Server`, `gView.GraphicsEngine*`

> Implementation notes (where it deviates from the proposal below):
> * Directory scanning lives in `gView.GraphicsEngine/FontProvisioning.cs` (`FontProvisioning.EnumerateFontFiles`).
> * Central registration: `SystemInfo.RegisterFontDirectories(params string[])` registers the
>   directories with the **currently active** engine (`GraphicsEngine.Current.Engine`) only -
>   the server picks exactly one engine at startup, so registering with the others is wasted
>   work. The server calls it from `gView.Server/AppCode/ServerFonts.cs`, wired into `Startup`
>   right after the graphics-engine selection (so `Current.Engine` is already the final one).
>   The per-engine registry is `static`, so it does not matter that the config-driven override
>   creates a fresh engine instance.
> * Skia registry resolution helper is `SkiaGraphicsEngine.TryResolveCustomTypeface(name, style)`
>   (Skia2x + Skia3x), consulted first in `SkiaFont`'s constructor.
> * The optional OS install uses `fc-cache -f` on Linux and `reg.exe add "HKCU\..."` on Windows
>   (the server targets `net10.0`, not `net10.0-windows`, so the registry API is not referenced).

## Problem

Many maps use symbols and labels that reference **TrueType/OpenType fonts** by family
name (marker symbols built from a font glyph, `TrueTypeMarkerSymbol`, label fonts, ...).
On a rendering server these fonts must be available to the process that draws the map.

Distributing and installing those fonts on a server is painful:

* OS font installation is **per user** on Windows and needs the right user context.
* On Linux it means dropping files into a fontconfig search path and refreshing the
  cache.
* In containers the image is ephemeral, so "install on every start" is wasted work and
  the container user often has no writable `$HOME`.
* A font installed while the server process is already running is **not reliably picked
  up** by that process (GDI/DirectWrite needs a `WM_FONTCHANGE` broadcast, fontconfig
  caches its search result at first use).

## Goal

Let an administrator point gView Server at a **directory of font files** (checked in
next to the service configuration, mounted into the container, ...). At startup the
server makes those fonts usable for map rendering, with **identical behaviour on
Windows and Linux** and no elevated privileges.

### Non-goals

* Hot reload of fonts without a server restart.
* Making the fonts visible to unrelated processes on the host (see
  [Optional: best-effort OS install](#optional-best-effort-os-install)).
* Font subsetting / substitution rules.

## Current state (as of this proposal)

| Concern | Where |
| --- | --- |
| Server config file (`_config/mapserver.json`, bound to `IConfiguration`) | [`Program.cs`](../../gView.Server/Program.cs) line ~45 |
| Graphics engine selection (default **Skia**, optional GDI+) | [`Startup.cs`](../../gView.Server/Startup.cs) `#region Graphics Engine` |
| Engine registration for all apps | `SystemInfo.RegisterDefaultGraphicEngines` in [`SystemInfo.cs`](../../gView.Framework/Common/SystemInfo.cs) |
| Engine contract | [`IGraphicsEngine`](../../gView.GraphicsEngine/Abstraction/IGraphicsEngine.cs) |
| Skia font resolution: `SKTypeface.FromFamilyName(name, style)` | [`SkiaFont.cs`](../../gView.GraphicsEngine.Skia3x/SkiaFont.cs) |
| Skia installed-font list, **statically cached** | `SkiaGraphicsEngine.GetInstalledFontNames()` / `GetDefaultFontName()` |
| GDI+ font resolution: `new Font(family, size, style)` | [`GdiFont.cs`](../../gView.GraphicsEngine.GdiPlus/GdiFont.cs) |
| GDI+ installed-font list, **statically cached** (`FontFamily.Families`) | [`GdiGraphicsEngine.cs`](../../gView.GraphicsEngine.GdiPlus/GdiGraphicsEngine.cs) |

Notes that shape the design:

* `System.Drawing.Common` (the GDI+ path) is **Windows-only** since .NET 7. The
  cross-platform server always runs on **Skia**, so "must work on Linux" effectively
  means "must work in the Skia engine".
* `GetInstalledFontNames()` and `GetDefaultFontName()` cache their result in `static`
  fields on first call. Any font registration must happen **before** the first call,
  or must invalidate those caches.
* `SKTypeface` is not thread-safe; the Skia engine already guards each typeface with a
  per-`Handle` `ThreadLocker` (`_threadLockers` in `SkiaFont`), and every `SkiaFont`
  construction is serialized through one static `_threadLocker`.
* The server serves hundreds of concurrent requests. Font *resolution* (this feature)
  must not add locking on the render hot path — registration is startup-only, so the
  lookup structures are treated as immutable afterwards and read lock-free.

## Considered approaches

### A. In-process font registry — **recommended**

Load the font files into the graphics engine at startup and resolve font family names
against them **before** falling back to the OS font manager.

* No privileges, no OS state, no `$HOME`.
* Deterministic and identical on Windows, Linux and in containers.
* The engine already owns font creation, so this is a small, well-contained seam.

### B. Copy into the OS user font store at startup — rejected as the primary mechanism

* **Windows per-user:** copy to `%LOCALAPPDATA%\Microsoft\Windows\Fonts` and add a
  value under `HKCU\Software\Microsoft\Windows NT\CurrentVersion\Fonts`. Works without
  admin only on Windows 10 1809+, and the already-running process will not see the new
  fonts without a `WM_FONTCHANGE` broadcast / restart.
* **Linux:** copy to `~/.local/share/fonts` and run `fc-cache -f`. A *new* process then
  sees the fonts; the running process may not, and it needs a writable `$HOME`.
* **Containers:** ephemeral image, frequently read-only `$HOME`, re-doing it on every
  boot is wasteful.

Kept only as an **optional** best-effort extra for the benefit of other tools in the
same environment (see below).

## Recommended design

### 1. Configuration

`_config/mapserver.json`:

```jsonc
{
  // ...
  "fonts": {
    // One or more directories scanned recursively for *.ttf, *.otf, *.ttc.
    // Relative paths resolve against the server content root.
    "directories": [ "{repository-path}/server/fonts" ],

    // Optional. Best-effort copy into the per-user OS font store as well,
    // for the benefit of other processes (GDAL, ...). Default: false.
    "install-to-system": false
  }
}
```

Read with the existing `Configuration.Section(...)` / `Configuration.Value(...)`
helpers. Update the sample [`_setup/_mapserver.json`](../../gView.Server/_setup/_mapserver.json)
and the user docs ([`en/server/config.md`](../en/server/config.md),
[`de/server/config.md`](../de/server/config.md)).

### 2. Abstraction change

Add to [`IGraphicsEngine`](../../gView.GraphicsEngine/Abstraction/IGraphicsEngine.cs):

```csharp
/// <summary>
/// Registers all font files (*.ttf, *.otf, *.ttc) found under <paramref name="path"/>
/// (recursively) with this engine, so map rendering can resolve their family names
/// without the font being installed in the operating system. Idempotent; safe to call
/// with a non-existent path (no-op).
/// </summary>
void RegisterFontDirectory(string path);
```

### 3. Skia implementation

`SkiaGraphicsEngine`:

* Static registry `ConcurrentDictionary<string, CustomFace[]>` keyed by family name
  (`StringComparer.OrdinalIgnoreCase`). `CustomFace` caches the `SKTypeface` plus its
  `Weight` / `Width` / `Slant` as plain ints so the resolve path never touches
  `SKTypeface` members.
* `RegisterFontDirectory` enumerates the directory, loads each file via
  `SKTypeface.FromFile(path)`, and appends a `CustomFace` under `typeface.FamilyName`
  (`AddOrUpdate` with a fresh array — the per-family array stays immutable). Runs under
  `_registrationLock`, which is a **startup-only** lock (registration happens in
  `Startup`'s constructor, before Kestrel listens) and is therefore never contended by
  request threads. Log every loaded `FamilyName` + style (see [Diagnostics](#6-diagnostics)).
* After loading, reset `_installedFontNames` / `_defaultFontName` so the new families
  show up in `GetInstalledFontNames()` (font pickers, `GetDefaultFontName()`).
* The default `SKFontManager` cannot be extended with an extra directory at runtime,
  which is why we keep our own registry.

`SkiaFont` constructor:

```csharp
var fontTypeFace = _threadLocker.GetInterLocked(() =>
    SkiaGraphicsEngine.TryResolveCustomTypeface(name, fontStyle)        // NEW: registry first
    ?? SKTypeface.FromFamilyName(name, fontStyle.ToSKFontStyle()));     // unchanged fallback
```

`TryResolveCustomTypeface` picks the nearest style match (weight + width + slant
scoring) from the family's `CustomFace[]`. It is **lock-free**: the array is immutable
after startup, and `ConcurrentDictionary.TryGetValue` needs no lock. It also runs inside
`SkiaFont`'s existing static `_threadLocker` (which already serializes every
`SKTypeface.FromFamilyName` call, since `SKTypeface` is not thread-safe), so the feature
adds **zero** additional locking on the render path. Thread-safety of the resolved
`SKTypeface` for drawing is unchanged: it still gets a per-`Handle` `ThreadLocker` via
the existing `_threadLockers` logic.

### 4. GDI+ implementation (Windows only)

`GdiGraphicsEngine`:

* Static `PrivateFontCollection`; `RegisterFontDirectory` calls `AddFontFile(path)` per
  file (under the startup-only `_registrationLock`), then rebuilds a
  `ConcurrentDictionary<string, FontFamily> _familiesByName` read index.
* Merge the new families into the cached `_installedFontNames`.

`GdiFont` constructor:

* `new Font("Name", ...)` does **not** see `PrivateFontCollection` families.
  `TryGetPrivateFontFamily` does a **lock-free** `_familiesByName.TryGetValue`; on a hit,
  `new Font(family, size, style)` (with an `IsStyleAvailable` fallback to an available
  style), otherwise the unchanged `new Font(name, size, style)`. Unlike the Skia path,
  GDI font construction has no outer serializing lock, so the lock-free lookup matters
  here for concurrent renders.

### 5. Startup wiring &nbsp;— ordering matters

Register the directories **after** the final engine is chosen and **before** the first
render / `PreloadServicesHostedService` runs, i.e. in the `#region Graphics Engine`
block of [`Startup.cs`](../../gView.Server/Startup.cs):

```csharp
var dirs = Configuration.Section("fonts:directories").Get<string[]>() ?? [];
// resolve relative paths against the content root, drop non-existent ones
SystemInfo.RegisterFontDirectories(resolvedDirs);   // -> GraphicsEngine.Current.Engine only
```

Register only with `GraphicsEngine.Current.Engine`: the server selects exactly one
engine at startup, and the per-engine typeface/`PrivateFontCollection` registries are
`static`, so registering with the other (unused) engine would just be wasted work.

Because `GetInstalledFontNames()` / `GetDefaultFontName()` cache statically on first
call, registration must not run later than the first font access.

Central helper: `SystemInfo.RegisterFontDirectories(params string[])`. Other hosts
(MxlUtil, DataExplorer, Carto) can call the same helper once they have selected their
engine.

### 6. Diagnostics

On startup, log one line per loaded file: file path → resolved `FamilyName` + style +
which engine. The most common failure is that a map references a family name that does
not match the font file's **internal** family name; this log is how an administrator
diagnoses it. Consider a summary line `"fonts: N files, M families registered"`.

## Optional: best-effort OS install

When `"install-to-system": true`, additionally (not instead) copy the font files into
the per-user OS store:

* **Linux:** copy to `~/.local/share/fonts`, then run `fc-cache -f` (ignore failure).
* **Windows:** copy to the per-user Fonts folder and add the `HKCU` registry value
  (ignore failure).

This helps other processes in the same environment (e.g. GDAL text rendering) but the
map rendering path must not depend on it succeeding.

## Edge cases & risks

| Case | Handling |
| --- | --- |
| Family name in `.mxl` ≠ internal name of the TTF | Cannot be fixed automatically; the startup log surfaces it. |
| Same family provided by OS **and** directory | Registry wins for rendering (predictable, admin-controlled). |
| Bold / Italic requested but only Regular file present | Style matching falls back to nearest; engines already synthesise style. |
| `.ttc` collections | `SKTypeface.FromFile` loads index 0; enumerate all faces if needed. |
| Corrupt / non-font file in the directory | Catch per file, log, continue. |
| Directory missing | No-op (allows the key to stay in a shared config). |
| Duplicate registration (called twice) | Idempotent: skip already-known file paths. |
| Config changed at runtime | Not supported — `mapserver.json` is loaded with `reloadOnChange: false`; restart required. |

## Affected files

* [`src/gView.GraphicsEngine/Abstraction/IGraphicsEngine.cs`](../../gView.GraphicsEngine/Abstraction/IGraphicsEngine.cs) — new method.
* [`src/gView.GraphicsEngine.Skia3x/SkiaGraphicsEngine.cs`](../../gView.GraphicsEngine.Skia3x/SkiaGraphicsEngine.cs) + [`SkiaFont.cs`](../../gView.GraphicsEngine.Skia3x/SkiaFont.cs) — registry + resolution + cache merge. Mirror in `Skia2x` if still built.
* [`src/gView.GraphicsEngine.GdiPlus/GdiGraphicsEngine.cs`](../../gView.GraphicsEngine.GdiPlus/GdiGraphicsEngine.cs) + [`GdiFont.cs`](../../gView.GraphicsEngine.GdiPlus/GdiFont.cs) — `PrivateFontCollection` + resolution + cache merge.
* [`src/gView.Framework/Common/SystemInfo.cs`](../../gView.Framework/Common/SystemInfo.cs) — optional central registration hook.
* [`src/gView.Server/Startup.cs`](../../gView.Server/Startup.cs) — read config, call registration.
* [`src/gView.Server/_setup/_mapserver.json`](../../gView.Server/_setup/_mapserver.json) — sample `fonts` block.
* [`src/docs/en/server/config.md`](../en/server/config.md), [`src/docs/de/server/config.md`](../de/server/config.md) — document the key.
* `CHANGELOG.md` — entry under the current version.

## Implementation checklist

1. [x] `IGraphicsEngine.RegisterFontDirectory(string)`.
2. [x] Skia: static registry, `SKTypeface.FromFile`, style matching in `SkiaFont` (Skia2x + Skia3x).
3. [x] Skia: merge families into `GetInstalledFontNames()` cache.
4. [x] GDI+: `PrivateFontCollection`, `FontFamily` lookup in `GdiFont`, cache merge.
5. [x] Central registration in `SystemInfo` + call site in `Startup.cs` (via `ServerFonts`).
6. [x] Read `fonts:directories` (+ optional `fonts:install-to-system`) from config.
7. [x] Startup logging of loaded families (`[fonts] ...` lines).
8. [x] Optional best-effort OS install behind the flag.
9. [x] Sample config + EN/DE docs + CHANGELOG.
10. [ ] Test: a map whose label/marker font is only in the directory renders correctly
    on Linux (Skia) and Windows. *(manual verification pending)*
