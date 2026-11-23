---
status: accepted
---

# gView as a plugin of gView.Pipe

gView.Pipe (the workflow app, sibling repository gview-pipe) gets nodes that read and write geodata through the gView dataset model: Read Features and Write Features, later spatial operations and network traces. The plugin `gView.Pipe.Plugins.Gis` lives here, because it needs most of gView and has to match the installed gView version. Pipe loads it from a gView folder (`Pipe:ExternalPlugins`, gview-pipe ADR 0044) into its own load context; its dependencies, the data sources included, come from that folder.

## Decisions

- **PlugInManager:** `PlugInManager.PluginDirectory` sets the folder scanned for `gView.*.dll`; without it, the folder of the entry assembly as before. Inside an isolated load context the plugins are loaded into that context (`AssemblyLoadContext.LoadFromAssemblyPath`), so their types share gView.Framework with the caller; in the default context it stays `Assembly.LoadFrom`. `gView.Pipe.*.dll` are never scanned: they reference the Pipe contracts, which only the Pipe host has.
- **Startup:** the plugin's `IPipePluginStartup` sets `PluginDirectory` to its own folder. Nothing else in gView knows about Pipe.
- **Contracts:** `gView.Pipe.Abstraction` is referenced as package with `ExcludeAssets="runtime"`; the host provides it. Until it is on nuget.org it comes from the sibling checkout (`nuget.config` next to the project, `dotnet pack src/gView.Pipe.Abstraction -c Release -o artifacts/packages` in gview-pipe).
- **Deployment:** the deploy build publishes the plugin (framework-dependent, per platform) and adds its files to the server and web apps ZIPs and Docker contexts without replacing any file of the app: only `gView.Pipe.Plugins.Gis.dll`, its `deps.json` and what the app doesn't have. So every gView installation contains the plugin; Pipe lists `<gView folder>/gView.Pipe.Plugins.Gis.dll` in `Pipe:ExternalPlugins`.
- **Development:** the build output of the plugin is a complete folder (it references the data sources it supports), so `Pipe:ExternalPlugins` can point at `src/gView.Pipe.Plugins.Gis/bin/Debug/net10.0/gView.Pipe.Plugins.Gis.dll`.
- **Feature classes:** the node Feature Class creates and deletes them through `IFeatureDatabase` of the dataset's database (OGC spatial databases such as PostGIS, the FDBs, shape files, GeoPackage, SpatiaLite); other data sources give an error. Truncate is `IFeatureUpdater.Delete(featureClass, "1=1")`.
- **Tests:** `gView.Pipe.Plugins.Gis.Tests` run the nodes with a small fake node context against a gView FDB in a SQLite file. Shape files can't be used for writing: gView can't update or delete in them.

## Consequences

- Building `gView-gis.sln` needs the package from gview-pipe in its `artifacts/packages` until the package is published.
- Code that finds its files relative to the entry assembly (`Assembly.GetEntryAssembly()`), e.g. SpatiaLite's native libraries via `AppContext.BaseDirectory`, WMS templates or UI dialogs, looks in the Pipe folder when gView runs inside Pipe. That doesn't matter for reading and writing features; fix such places when a node needs them.
