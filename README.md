# gView GIS

<p align="center">
  <img src="https://img.shields.io/badge/License-Apache%202.0-blue.svg">
  <img src="https://img.shields.io/badge/Version-8.26.4001-brightgreen">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4">
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20Linux%20%7C%20Docker-lightgrey">
</p>

<p align="center">
    <a href="https://docs.gviewonline.com/en/index.html" target="_blank"><img src="https://img.shields.io/badge/Documentation-English-green?style=flat-round"></a>
    <a href="https://docs.gviewonline.com/de/index.html" target="_blank"><img src="https://img.shields.io/badge/Documentation-German-green?style=flat-round"></a>
</p>

gView GIS is a user-friendly open source GIS framework for creating maps and publishing them as map and feature services.

| Component | Description |
| --- | --- |
| **gView.WebApps** | Blazor web application with **gView Carto** (create maps) and **gView DataExplorer** (browse and convert spatial data). Runs locally or on a server. |
| **gView Server** | Map server that publishes gView Carto maps as **GeoServices REST** (MapServer / FeatureServer), **WMS**, **WFS**, **WMTS** and **ArcXML** services. |
| **gView.Cmd** | Command line tools (copy feature classes, tile caches, ...). Shipped together with gView.WebApps. |

## Installation

gView.WebApps and gView Server run on **Windows** and **Linux** and need the
[ASP.NET Core Runtime 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) (`Microsoft.AspNetCore.App`).

### gView.Deploy (recommended)

Use **gView.Deploy** from the [Releases](https://github.com/jugstalt/gview-gis/releases) to install and update
gView on your machine. It downloads the latest release for your operating system and sets up the configuration.

### Manual download

Each release provides one ZIP file per application and platform:

| File | Content |
| --- | --- |
| `gview-server-win64-{version}.zip` | gView Server for Windows |
| `gview-server-linux64-{version}.zip` | gView Server for Linux |
| `gview-webapps-win64-{version}.zip` | gView.WebApps + gView.Cmd for Windows |
| `gview-webapps-linux64-{version}.zip` | gView.WebApps + gView.Cmd for Linux |

Start scripts are included in each ZIP (`gview-server.bat|.sh` => `http://localhost:45622`,
`gview-webapps.bat|.sh` => `http://localhost:45623`).

### Docker

| Image | Description |
| --- | --- |
| `ghcr.io/jugstalt/gview-server` | gView Server, ready to run |
| `ghcr.io/jugstalt/gview-server-base` | gView Server without entry point, base for your own images (eg. with your own `_config`) |
| `ghcr.io/jugstalt/gview-webapps` | gView.WebApps |

```bash
docker run -d -p 45622:8080 -e GV_REPOSITORY_PATH=/etc/gview -e GV_ONLINERESOURCE_URL=http://localhost:45622 ghcr.io/jugstalt/gview-server
```

```bash
docker run -d -p 45623:8080 ghcr.io/jugstalt/gview-webapps
```

The containers listen on port `8080` (non-root friendly).

### Documentation

Read the installation and user documentation:

- [English](https://docs.gviewonline.com/en/index.html)
- [German](https://docs.gviewonline.com/de/index.html)

## Building from source

Requirements: [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) (or newer),
[Docker](https://www.docker.com/) for the docker images.

The solution is `gView-gis.sln`. For development, start `gView.Server` and/or `gView.WebApps`
from Visual Studio or with `dotnet run`.

### Deploy build (NUKE)

Releases are built with a [NUKE](https://nuke.build) build script ([build/Build.cs](build/Build.cs)).
It runs the tests, publishes gView Server, gView.WebApps and gView.Cmd for `win64` and `linux64`,
creates the release ZIP files and builds the docker images.

Run it from the repository root:

```bash
.\build.cmd
```

On Linux / macOS use `./build.sh`, or `build.ps1` in PowerShell. Without arguments the default target `Deploy` runs.

#### Targets

| Target | Description |
| --- | --- |
| `Test` | Runs all test projects in `src/tests` |
| `Compile` | Publishes gView Server, gView.WebApps and gView.Cmd for each platform into `artifacts/` (depends on `Test`) |
| `CleanArtifacts` | Removes developer configurations (`_config`) and third party `*.pdb` / `*.xml` files from the artifacts |
| `Zip` | Creates the release ZIP files in `{DeployRoot}\download` |
| `Docker` | Builds the docker images `gview-server-base`, `gview-server` and `gview-webapps` (only for `linux64`, not for editions) |
| `DockerPush` | Pushes the docker images to the registry |
| `Deploy` | `Zip` + `Docker` (default) |

Targets run their dependencies automatically: `Test` → `Compile` → `CleanArtifacts` → `Zip` / `Docker`.

#### Parameters

| Parameter | Default | Description |
| --- | --- | --- |
| `--version` | version from `SystemInfo.cs` | Version used for ZIP names and docker tags |
| `--edition` | (none) | `portable` uses the self-contained `*-portable` publish profiles and adds `-portable` to the ZIP names (no docker images) |
| `--platform` | `win64 linux64` | Platforms to build |
| `--deploy-root` | `c:\deploy\gview-gis` | Output root for `download\` (ZIP files) and `docker\` (docker build contexts) |
| `--configuration` | `Release` | Build configuration |
| `--docker-exe` | `docker` | Docker executable |
| `--docker-registry` | `ghcr.io/jugstalt` | Registry for the docker tags and `DockerPush` |
| `--skip [targets]` | | Skips the given targets (eg. `--skip Test`) |

#### Examples

Full release build (tests, ZIP files, docker images):

```bash
.\build.cmd
```

Only the ZIP files, without docker:

```bash
.\build.cmd Zip
```

Release build without running the tests:

```bash
.\build.cmd --skip Test
```

Portable edition (self-contained, no .NET runtime required):

```bash
.\build.cmd Zip --edition portable
```

Windows only:

```bash
.\build.cmd Zip --platform win64
```

Rebuild the docker images from the existing artifacts (no tests, no compile):

```bash
.\build.cmd Docker --skip Test Compile
```

Build and push the docker images to `ghcr.io/jugstalt` (requires `docker login ghcr.io`):

```bash
.\build.cmd DockerPush
```

Different version or output directory:

```bash
.\build.cmd --version 8.26.4002 --deploy-root d:\deploy\gview-gis
```

Show all targets and parameters:

```bash
.\build.cmd --help
```

#### Deploy assets

Files that are added to the published applications (start scripts, `_setup` configuration templates,
`sql` scripts, Dockerfiles) are located in [publish/](publish/):

```
publish/
  server/
    override/          added to all server packages
    win64/ linux64/    start scripts and platform specific overrides
    docker/            Dockerfile (gview-server-base), gview-server/Dockerfile
  webapps/
    override/          added to all webapps packages
    win64/ linux64/    start scripts and platform specific overrides
    docker/            Dockerfile, docker specific overrides
```

The ZIP structure (`{version}/server|webapps/...` + start script) is used by **gView.Deploy** and must not change.

## Screenshots

### gView Carto

Create maps in a user-friendly UI

![image](https://github.com/jugstalt/gview-gis/assets/26577522/4bbfbf7d-b1e2-4a58-8daf-dec202e34b3b)

Set the rendering for each layer

![image](https://github.com/jugstalt/gview-gis/assets/26577522/daeca7cb-d717-49d2-bf21-5ff6f93733b0)

Set labelling for any vector layer

![image](https://github.com/jugstalt/gview-gis/assets/26577522/c4b9d272-fba0-4995-8f93-f4a5a4ac1fbf)

### gView DataExplorer

![image](https://github.com/jugstalt/gview-gis/assets/26577522/1e5665cc-c9f5-4b4d-9778-6e23ac67dbb7)

Browse your spatial data or copy vector data to different data formats

![image](https://github.com/jugstalt/gview-gis/assets/26577522/8b1e2480-02f4-4d4c-8318-994b167eab9f)

### gView Server

Manage the services and check the service status (running, stopped, error)

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server1.png)

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server2.png)

Set the security for a service or for a folder (group of services)

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server3.png)

Publish services by uploading gView Carto map projects

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server4.png)

Browse the services via the GeoServices REST API in YAML or JSON

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server5.png)

## License

gView GIS is licensed under the [Apache License 2.0](LICENSE). Third party licenses: [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).
