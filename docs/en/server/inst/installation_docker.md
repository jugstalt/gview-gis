Docker Container (Linux)
========================

The following section assumes a thorough knowledge of Docker.

The first thing to do is to create a Dockerfile. Here is an example in which it is also shown that
the necessary libraries *GDI+* and *proj4* are installed.

```

   FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0-buster-slim

   #
   # Install GDI +
   #
   RUN apt-get update
   RUN apt-get install -y libgdiplus lsof
   RUN apt-get install -y libc6-dev
   RUN ln -s /user/lib/libgdiplus.so.0.0.0 /usr/lib/gdiplus.dll

   #
   # Install Proj4 Lib
   #
   RUN apt-get install -y proj-bin
   RUN ln -s /usr/lib/x86_64-linux-gnu/libproj.so.13 /usr/lib/proj.dll

   #
   # Copy app (optional: copy from override for example _config...)
   #
   WORKDIR /app
   EXPOSE 80
   COPY /app .
   COPY /override .

   #
   # Install Fonts Example (optional)
   #
   run mkdir ~/.fonts
   COPY /fonts /fonts
   RUN cp /fonts/* ~/.fonts
   RUN apt-get install fontconfig
   RUN fc-cache -f -v

   ENTRYPOINT ["dotnet", "gView.Server.dll"]

```

A container can be created with the following command:

```
     
    docker build -t gview_server:5.0.1 .
    docker tag gview_server:5.0.1 gview_server:latest

```

It can be started with the following command:

```

   docker run -d -p 8085:80 --name=gview-server gview_server:latest

```


[Next..](installation_standalone.md)
   


