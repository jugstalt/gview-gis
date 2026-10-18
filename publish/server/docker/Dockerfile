# gview-server-base: base image without ENTRYPOINT.
# Derived images add their own _config and
#   ENTRYPOINT ["dotnet", "gView.Server.dll"]
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final

# libfontconfig1 ... needed by skia
RUN apt-get update && \
    apt-get install -y libfontconfig1 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY /app .

#
# Container Security
# https://techcommunity.microsoft.com/t5/azure-developer-community-blog/hardening-an-asp-net-container-running-on-kubernetes/ba-p/2542224
#
# not 80 => errors with RunAsNonRoot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
# readOnlyRootFilesystem = true works with this:
ENV COMPlus_EnableDiagnostics=0
