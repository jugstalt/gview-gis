FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS final

# libfontconfig1 ... needed by skia
RUN apt-get update && \
    apt-get install -y libfontconfig1 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY /app .

# not 80 => errors with RunAsNonRoot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
# readOnlyRootFilesystem = true works with this:
ENV COMPlus_EnableDiagnostics=0

ENTRYPOINT ["dotnet", "gView.WebApps.dll"]
