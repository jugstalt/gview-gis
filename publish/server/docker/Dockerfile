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

# Run as the non-root user "app" (UID 1654) of the .NET base image.
# Everything the app writes at runtime must be writable for this user:
#   /app/_config                  created on first start from _setup (or mounted)
#   /etc/gview/gview-repository   default repository (GV_REPOSITORY_PATH), mount a volume here
#   /etc/gview-repository         repository path used in the docs examples
# Host volumes must be writable for UID 1654.
RUN mkdir -p /app/_config /etc/gview/gview-repository /etc/gview-repository && \
    chown -R app:app /app/_config /etc/gview /etc/gview-repository
USER app
