# gView GIS

<p align="center">
  <img src="https://img.shields.io/badge/License-Apache%202.0-blue.svg">
  <img src="https://img.shields.io/badge/Version-6.25.1701-brightgreen">
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-lightgrey">
</p>

<p align="center">
    <a href="https://docs.gviewonline.com/en/index.html" target="_blank"><img src="https://img.shields.io/badge/Documentation-English-green?style=flat-round"></a>
    <a href="https://docs.gviewonline.com/de/index.html" target="_blank"><img src="https://img.shields.io/badge/Documentation-German-green?style=flat-round"></a>
</p>

gView GIS is a user-friendly Open Source GI framework for creating map and feature services.

The software consists of Blazor applications for creating maps and the gView server for publishing the maps as map services (WMS, GeoServices REST, etc).

The Blazor app _gView.WebApps_ can started locally or run the server. It provides the gView applications _gView.Carto_ and _gView.Explorer_.

_gView.WebApps_ and _gView Server_ runs on Windows or Linux (require die Microsoft.AspNetCore.App 8.0)
and also Docker Images are available.

To Deploy the software on your machine use [gView.Deploy](https://github.com/jugstalt/gview-gis/releases).
This will also download the latest releases for your operating system.

You can manually download the latest Download the leatest [Releases](https://github.com/jugstalt/gview-gis/releases) here.

Read the installation documentation::

-   [English](https://docs.gviewonline.com/en/index.html)
-   [German](https://docs.gviewonline.com/de/index.html)

Some screenshots to demonstrate some of the features of gview GIS

## Deskop: gView Carto

Create maps in an userfriendly UI

![image](https://github.com/jugstalt/gview-gis/assets/26577522/4bbfbf7d-b1e2-4a58-8daf-dec202e34b3b)

Set rendering for each layer

![image](https://github.com/jugstalt/gview-gis/assets/26577522/daeca7cb-d717-49d2-bf21-5ff6f93733b0)

Set labelling for any vector layer

![image](https://github.com/jugstalt/gview-gis/assets/26577522/c4b9d272-fba0-4995-8f93-f4a5a4ac1fbf)

## Desktop: gView DataExplorer

![image](https://github.com/jugstalt/gview-gis/assets/26577522/1e5665cc-c9f5-4b4d-9778-6e23ac67dbb7)

Browse your spatial data or copy vector data to different data formats

![image](https://github.com/jugstalt/gview-gis/assets/26577522/8b1e2480-02f4-4d4c-8318-994b167eab9f)

## gView Server

A map server that exports gView Carto map as _GeoServices REST_, _ArcXML_, _WMS_, _WFS_, _WMTS_ services

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server1.png)

Manage the services and check the service status (running, stopped, error)

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server2.png)

Set the security for a service or for a folder (group of services)

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server3.png)

Publish services by uploading gView Carto map projects

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server4.png)

Browse the services via the GeoServices Rest API in YAML or JSON

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server5.png)
