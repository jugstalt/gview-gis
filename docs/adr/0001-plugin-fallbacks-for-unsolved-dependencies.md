---
status: accepted
---

# Plugin fallbacks for unsolved dependencies

GDAL and MrSID native libraries no longer ship with gView, yet existing projects reference the GDAL raster dataset (`43DFABF1-…`) and the MrSID raster file dataset (`D4812641-…`) by GUID. So the PlugInManager now substitutes a fallback plugin whenever an original plugin reports unsolved dependencies. The fallback is declared **on the fallback plugin** (`[PlugInFallbackFor(guid)]`, e.g. on RasterNET), not on the original. The original then needs no knowledge of its replacement, several originals can share one fallback, and the original's assembly never has to change to gain one.

## Consequences

- **Saving migrates the project.** A project loaded through a fallback is saved with the fallback's GUID and format. If the admin installs GDAL/MrSID later, already re-saved projects stay on the fallback. This is deliberate, because the fallback is the plugin that actually opened the data. Map loading reports each substitution at the new `Info` level, so the users know before they save. `Info` is used instead of `Warning` so that a strict gView.Server `CriticalErrorLevel` does not refuse to publish a service that works.
- A fallback must be able to load the original's persisted stream, not just its connection string. For example, RasterNET reads MrSID's `"Directory"` key and keeps saving it, so that no layer references are lost.
- Code that asks "is GDAL available?" can no longer use `PlugInManager.Create(guid) != null`. It must query the dependency state explicitly.

## Considered Options

- Declaring the fallback on the original via an extended `IPlugInDependencies`: rejected, because it couples every original to its replacement and touches every existing implementer.
- Persisting the original GUID while serving the fallback: rejected, because the saved stream would come from one plugin and be read back by another.
