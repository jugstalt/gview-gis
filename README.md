# gview5

The software consists of desktop applications for creating maps and the gView server for publishing the maps as map services (WMS, GeoServices REST, etc).

*gView Carto* and *gView DataExplorer* runs on windows and require die .NET Framework >=4.7.2 
*gView Server* is fully .NET Core (AspNetCore 3.1.x). It can run on windows, linux, mac and as a docker container 

Download the leatest release as ZIP from [Releases](https://github.com/jugstalt/gview5/releases)
Unzip and run...

Read the installation documentation:
* [English](docs/en/index.md)
* [German](docs/de/index.md)

Some screenshots to demonstrate some of the features of gview GIS

## Deskop: gView Carto

Create maps in an userfriendly UI

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-carto1.png)

Set rendering for each layer

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-carto2.png)

Set labelling for any vector layer

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-carto3.png)

## Desktop: gView DataExplorer

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-explorer1.png)

Browse your spatial data or copy vector data to different data formats

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-explorer2.png)


## gView Server

A map server that exports gView Carto map as *GeoServices REST*, *ArcXML*, *WMS*, *WFS*, *WMTS* services

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server1.png)

Manage the services and check the service status (running, stopped, error)

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server2.png)

Set the security for a service or for a folder (group of services)

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server3.png)

Publish services by uploading gView Carto map projects

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server4.png)

Browse the services via the GeoServices Rest API in YAML or JSON

![alt text](https://raw.githubusercontent.com/jugstalt/gview5/master/content/img/gview5-server5.png)


