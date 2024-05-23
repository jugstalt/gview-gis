#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["gView.Server/gView.Server.csproj", "gView.Server/"]
COPY ["gView.Editor.Core/gView.Editor.Core.csproj", "gView.Editor.Core/"]
COPY ["gView.Core/gView.Core.csproj", "gView.Core/"]
COPY ["gView.GraphicsEngine/gView.GraphicsEngine.csproj", "gView.GraphicsEngine/"]
COPY ["gView.Server.Models/gView.Server.Models.csproj", "gView.Server.Models/"]
COPY ["gView.DataSources.MongoDb/gView.DataSources.MongoDb.csproj", "gView.DataSources.MongoDb/"]
COPY ["gView.Geometry/gView.Geometry.csproj", "gView.Geometry/"]
COPY ["gView.Db/gView.Db.csproj", "gView.Db/"]
COPY ["gView.System/gView.System.csproj", "gView.System/"]
COPY ["gView.GraphicsEngine.GdiPlus/gView.GraphicsEngine.GdiPlus.csproj", "gView.GraphicsEngine.GdiPlus/"]
COPY ["gView.GraphicsEngine.Skia/gView.GraphicsEngine.Skia.csproj", "gView.GraphicsEngine.Skia/"]
COPY ["gView.Math/gView.Math.csproj", "gView.Math/"]
COPY ["gView.OGC/gView.OGC.csproj", "gView.OGC/"]
COPY ["gView.Web/gView.Web.csproj", "gView.Web/"]
COPY ["gView.Data/gView.Data.csproj", "gView.Data/"]
COPY ["gView.Carto.Rendering/gView.Carto.Rendering.csproj", "gView.Carto.Rendering/"]
COPY ["gView.Carto/gView.Carto.csproj", "gView.Carto/"]
COPY ["gView.Symbology/gView.Symbology.csproj", "gView.Symbology/"]
COPY ["gView.Network/gView.Network.csproj", "gView.Network/"]
COPY ["gView.DataSources.Fdb/gView.DataSources.Fdb.csproj", "gView.DataSources.Fdb/"]
COPY ["gView.DataSources.Raster/gView.DataSources.Raster.csproj", "gView.DataSources.Raster/"]
COPY ["gView.Offline/gView.Offline.csproj", "gView.Offline/"]
COPY ["gView.DataSources.Shape/gView.DataSources.Shape.csproj", "gView.DataSources.Shape/"]
COPY ["gView.Metadata/gView.Metadata.csproj", "gView.Metadata/"]
COPY ["gView.Server.Connector/gView.Server.Connector.csproj", "gView.Server.Connector/"]
COPY ["gView.GraphicsEngine.Default/gView.GraphicsEngine.Default.csproj", "gView.GraphicsEngine.Default/"]
COPY ["gView.Azure/gView.Azure.csproj", "gView.Azure/"]
COPY ["gView.Interoperability.GeoServices/gView.Interoperability.GeoServices.csproj", "gView.Interoperability.GeoServices/"]
COPY ["gView.Plugins.Modules/gView.Plugins.Modules.csproj", "gView.Plugins.Modules/"]
COPY ["gView.DataSources.Fdb.SQLite/gView.DataSources.Fdb.SQLite.csproj", "gView.DataSources.Fdb.SQLite/"]
COPY ["gView.Interoperability.Server/gView.Interoperability.Server.csproj", "gView.Interoperability.Server/"]
COPY ["gView.XML/gView.XML.csproj", "gView.XML/"]
COPY ["gView.DataSources.Fdb.PostgreSql/gView.DataSources.Fdb.PostgreSql.csproj", "gView.DataSources.Fdb.PostgreSql/"]
COPY ["gView.DataSources.PostGIS/gView.DataSources.PostGIS.csproj", "gView.DataSources.PostGIS/"]
COPY ["gView.DataSources.Fdb.SqlServer/gView.DataSources.Fdb.SqlServer.csproj", "gView.DataSources.Fdb.SqlServer/"]
COPY ["gView.Interoperability.ArcXML/gView.Interoperability.ArcXML.csproj", "gView.Interoperability.ArcXML/"]
COPY ["gView.XML2/gView.XML2.csproj", "gView.XML2/"]
COPY ["gView.DataSources.EventTable/gView.DataSources.EventTable.csproj", "gView.DataSources.EventTable/"]
COPY ["gView.Security/gView.Security.csproj", "gView.Security/"]
COPY ["gView.Data.Joins/gView.Data.Joins.csproj", "gView.Data.Joins/"]
COPY ["gView.DataSources.GeoJson/gView.DataSources.GeoJson.csproj", "gView.DataSources.GeoJson/"]
COPY ["gView.DataSources.TileCache/gView.DataSources.TileCache.csproj", "gView.DataSources.TileCache/"]
COPY ["gView.Data.Fields/gView.Data.Fields.csproj", "gView.Data.Fields/"]
COPY ["gView.DataSources.OSGeo/gView.DataSources.OSGeo.csproj", "gView.DataSources.OSGeo/"]
COPY ["gView.DataSources.MSSqlSpatial/gView.DataSources.MSSqlSpatial.csproj", "gView.DataSources.MSSqlSpatial/"]
COPY ["gView.Interoperability.OGC/gView.Interoperability.OGC.csproj", "gView.Interoperability.OGC/"]
COPY ["gView.Logging/gView.Logging.csproj", "gView.Logging/"]
COPY ["gView.DataSources.VectorTileCache/gView.DataSources.VectorTileCache.csproj", "gView.DataSources.VectorTileCache/"]
RUN dotnet restore "gView.Server/gView.Server.csproj"
COPY . .
WORKDIR "/src/gView.Server"
RUN dotnet build "gView.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "gView.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "gView.Server.dll"]