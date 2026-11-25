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

## Networks

**Network**:
The graph of nodes and edges built from the point and line layers of a map, stored next to the map and searched without the source data.
_Avoid_: Topology, net model

**Network type**:
The classification of a node or edge, made of facets such as group, layer and attribute value (e.g. `[LowVoltage,Box,*]`); searches and rules select elements by type patterns.
_Avoid_: Category, class

**Tier**:
A level of the network such as high, medium or low voltage: the edges of some types, with their own sources, sinks and blocked nodes.
_Avoid_: Ebene, layer, level

**Separate tiers**:
A way to build a network in which lines connect only to points of their own tier, so that lines and points of different tiers at one place stay apart.
_Avoid_: Tier isolation

**Partition**:
A group of node types of which only one point may stand at one place; points of different partitions at one place are merged in the order of the map's table of contents.
_Avoid_: Node group

**Connect rule**:
A rule that decides whether a line end connects to, or a line is split at, a point of a given type.
_Avoid_: Snapping rule

**Helper node**:
A node the build creates where a line end meets no point, so the line still has two ends.
_Avoid_: Virtual node, dummy node

**Merged point**:
A point that was merged into another node at the same place and can still be named by its own label.
_Avoid_: Duplicate, alias point

**Node conflict**:
Two real points that are too close to be told apart but must not be merged.
_Avoid_: Collision

**Source**:
A node where supply starts, such as a transformer.
_Avoid_: Feeder, origin, root

**Sink**:
A node the supply should reach, such as a customer.
_Avoid_: Consumer, end point

**Branch**:
An edge at a source along which supply leaves it, such as a connector in a transformer station.
_Avoid_: Outlet, feeder, Abzweig

**Feeders**:
The answer to which source and branch feed each line of a network.
_Avoid_: Supply areas, coloring

**Supply kind**:
How a line is fed in the feeders: by one branch (edge), by more than one (multiple: rings, double supply), or by none along it (open, e.g. at a separation point).
_Avoid_: Feed type

**Outage**:
The part of the network cut off from its sources when nodes or edges are switched off.
_Avoid_: Failure area, blackout

**Validation rule**:
A check of a built network, such as "every customer reaches a transformer", reported as violations in a separate validate step and never during the build.
_Avoid_: Build check, integrity rule

**Network area**:
The bounding box or polygon a search or validation is restricted to; edges crossing its border are boundary edges and are not followed.
_Avoid_: Extent filter, clip

**Shard**:
A spatial piece of a stored network that is loaded only when a search reaches it.
_Avoid_: Tile, chunk
