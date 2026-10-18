# gview-server: gview-server-base + ENTRYPOINT => starts the server
# The nuke build passes the base image of the same version (--build-arg BASE_IMAGE=gview-server-base:{version})
ARG BASE_IMAGE=gview-server-base:latest
FROM ${BASE_IMAGE}

# use your own _config/mapserver.json
# WORKDIR /app/_config
# COPY /my-config .

WORKDIR /app

ENTRYPOINT ["dotnet", "gView.Server.dll"]
