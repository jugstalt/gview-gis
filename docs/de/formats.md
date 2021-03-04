Datenformate für Karten und Dienste
===================================

Vektordaten
-----------

Folgende Vektordaten können mit *gView GIS* verwendet werden:


 Format                   | Lesend   | Schreibend  | Anmerkung                     
--------------------------|----------|-------------|-------------------------------
 Esri Shape               |    x     |             |                               
 ArcSDE (SQL Server)      |    x     |      x      |  Geometrie muss als GEOMETRY oder GEOGRAPHY verspeichet sein                 
 Post GIS                 |    x     |      x      |                               
 MS SQL Server            |    x     |      x      |  Geometrie muss als GEOMETRY oder GEOGRAPHY verspeichet sein 
 MS SQL   Feature Database|    x     |      x      |  proprietäres gView Format              
 PostgeSQL Feature Database|    x     |      x      |  proprietäres gView Format                               
 SQLite   Feature Database                 |    x     |      x      |  proprietäres gView Format                    

Rasterdaten
-----------

Folgende Rasterdaten können mit *gview GIS* verwendet werden.


 Format                             | Quelle (Bibliothek)                           
------------------------------------|--------------------------------------------
 JPEG, PNG                          | GDI+ (.NET)                                   
 JPEG 2000, MrSid                   | LizardTec Lib.                                
 JPEG, TIF, GeoTIFF, ESRI Grid (adf), ECW, DEM, XPM   | GDAL                                          
                                             


gView Server Kartendienste
--------------------------

Dienste, die mit dem Kartenserver veröffentlicht werden können in folgenden Formaten abgefragt werden.

 
 Format                   | Lesend   | Schreibend  | Anmerkung                     
--------------------------|----------|-------------|-------------------------------
 WMS                      | X        |             | 1.1.1, 1.3.0                  
 WFS                      | X        |             | 1.0.0                         
 WMTS                     | X        |             | 1.0.0                         
 ArcXML                   | X        |             | 1.0.0                          
 GeoServices REST         | X        | X           |                               

Bei GeoServices REST handelt es sich um einen ([Open Web Foundation](http://www.openwebfoundation.org/faqs/users-of-owf-agreements)) Standard, der von
[ESRI](https://www.esri.com/en-us/arcgis/open-vision/overview) entwickelt wurde. Dieses Format wird auch vom ArcGIS Server verwendet.
Die Schnittstelle bietet neben *MapServices* auch *FeatureServices*, mit denen Geo-Objekte angelegt und bearbeitet 
werden können.


[Weiter..](desktop/init.md)
 