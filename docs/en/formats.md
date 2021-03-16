Data formats for maps and services
==================================

Vektordata
----------

The following vector data can be used with *gView GIS*:

 Format                   | read     | write       | note                          
--------------------------|----------|-------------|-------------------------------
 Esri Shape               |    x     |             |                               
 ArcSDE (SQL Server)      |    x     |      x      |  Geometry must be GEOMETRY or GEOGRAPHY 
 Post GIS                 |    x     |      x      |                               
 MS SQL Server            |    x     |      x      |  Geometry must be GEOMETRY or GEOGRAPHY                 
 MS SQL  Feature Database  |    x     |      x      |  proprietary gView format     
 PostgeSQL Feature Database|    x     |      x      |  proprietary gView format  
 SQLite  Feature Database |    x     |      x      |  proprietary gView format   


Rasterdata
----------

The following raster data can be used with *gview GIS*.

 Format                             | Source (Library)                  
------------------------------------|-----------------------------------------------
 JPEG, PNG                          | GDI+ (.NET)                               
 JPEG 2000, MrSid                   | LizardTec Lib.                                
 JPEG, TIF, GeoTIFF, ESRI Grid (adf), ECW, DEM, XPM  | GDAL 


gView Server Map Services
-------------------------

Services published to the map server can be requested in the following formats.


 Format                   | read     | write       | note                          
--------------------------|----------|-------------|-------------------------------
 WMS                      | X        |             | 1.1.1, 1.3.0                   
 WFS                      | X        |             | 1.0.0                          
 WMTS                     | X        |             | 1.0.0                         
 ArcXML                   | X        |             | 1.0.0                          
 GeoServices REST         | X        | X           |                                

GeoServices REST is an ([Open Web Foundation](http://www.openwebfoundation.org/faqs/users-of-owf-agreements)) standard developed by
[ESRI](https://www.esri.com/en-us/arcgis/open-vision/overview). This format is also used by the ArcGIS server.
In addition to *MapServices*, the interface also offers *FeatureServices*, with which geo-objects can created and edited. 
 

[Next..](desktop/index.md)