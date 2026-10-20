# gView GIS

A GIS platform (MapServer, Carto, DataExplorer) whose data sources, renderers and tools are plugins identified by a GUID and persisted by that GUID in projects.

## Plugins

**Plugin**:
A component registered under a fixed GUID; projects reference it only by that GUID.
_Avoid_: Extension, module

**Plugin dependency**:
A native library or installed package a plugin needs at runtime that does not ship with gView (e.g. GDAL, MrSID).
_Avoid_: Prerequisite, requirement

**Unsolved dependency**:
A plugin dependency that cannot be found on the machine gView runs on.
_Avoid_: Missing DLL, broken plugin

**Original plugin**:
The plugin a project actually references by GUID, regardless of what is used to serve it at runtime.
_Avoid_: Legacy plugin, real plugin

**Fallback plugin**:
A plugin that stands in for an original plugin with unsolved dependencies and can open what the original persisted. Saving a project while a fallback is in use stores the fallback, not the original.
_Avoid_: Replacement, substitute, alternative provider
