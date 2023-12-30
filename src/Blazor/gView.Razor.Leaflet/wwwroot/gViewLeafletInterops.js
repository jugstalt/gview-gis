var gViewLeaflet = new function () {
    this.maps = {};
    this.layers = {};

    this.createInteractiveLayer = function (layer) {
        return {
            ...this.createLayer(layer),
            interactive: layer.isInteractive,
            bubblingMouseEvents: layer.isBubblingMouseEvents
        };
    };

    this.createLayer = function (obj) {
        return {
            id: obj.id,
            pane: obj.pane,
            attribution: obj.attribution
        };
    };

    this.addLayer = function (mapId, layer, layerId) {
        layer.id = layerId;
        this.layers[mapId].push(layer);
        layer.addTo(this.maps[mapId]);
    };

    // removes properties that can cause circular references
    this.cleanupEventArgsForSerialization = function (eventArgs) {
        const propertiesToRemove = [
            "target",
            "sourceTarget",
            "propagatedFrom",
            "originalEvent",
            "tooltip",
            "popup"
        ];

        const copy = {};

        for (let key in eventArgs) {
            if (!propertiesToRemove.includes(key) && eventArgs.hasOwnProperty(key)) {
                copy[key] = eventArgs[key];
            }
        }

        return copy;
    };

    this.mapEvents = function (mapElement, objectReference, eventHandlerDict) {
        for (let key in eventHandlerDict) {

            const handlerName = eventHandlerDict[key];

            mapElement.on(key, function (eventArgs) {
                objectReference.invokeMethodAsync(handlerName,
                    gViewLeaflet.cleanupEventArgsForSerialization(eventArgs));
            });
        }
    };

    this.connectMapEvents = function (map, objectReference) {
        this.connectInteractionEvents(map, objectReference);
        this.connectBBoxEvents(map);

        this.mapEvents(map, objectReference, {
            "zoomlevelschange": "NotifyZoomLevelsChange",
            "resize": "NotifyResize",
            "unload": "NotifyUnload",
            "viewreset": "NotifyViewReset",
            "load": "NotifyLoad",
            "zoomstart": "NotifyZoomStart",
            "movestart": "NotifyMoveStart",
            "zoom": "NotifyZoom",
            "move": "NotifyMove",
            "zoomend": "NotifyZoomEnd",
            "moveend": "NotifyMoveEnd",
            "mousemove": "NotifyMouseMove",
            "click": "NotifyMouseClick",
            "keypress": "NotifyKeyPress",
            "keydown": "NotifyKeyDown",
            "keyup": "NotifyKeyUp",
            "preclick": "NotifyPreClick",
            "bbox": "NotifyBBox"
        });
    };

    this.connectBBoxEvents = function (map) {
        let bboxMode = false;

        let _enableBBoxToolHandling = function (map) {

            map.dragging.disable();
            map.boxZoom.disable();
            map.keyboard.disable();
            //this.touchEventHandlers.bindBoxEvents();
        };
        let _disableBBoxToolHandling = function (map) {
            map.dragging.enable();
            map.touchZoom.enable();
            map.doubleClickZoom.enable();
            map.scrollWheelZoom.enable();
            map.boxZoom.enable();
            map.keyboard.enable();
        };

        let toolBoxLayer;

        map.on('mousedown', function (e) {
            bboxMode = e.originalEvent.ctrlKey;

            if (bboxMode) {
                _enableBBoxToolHandling(map);

                let bounds = L.latLngBounds(e.latlng, e.latlng);
                toolBoxLayer = L.rectangle(bounds, { color: '#ff7800', weight: 2 });
                map.addLayer(toolBoxLayer);
            }
        });
        map.on('mousemove', function (e) {
            if (bboxMode) {
                if (toolBoxLayer) {
                    let bounds = L.latLngBounds(toolBoxLayer.getBounds().getNorthWest(), e.latlng);
                    toolBoxLayer.setBounds(bounds);
                }
            }
        });
        map.on('mouseup', function (e) {
            if (bboxMode) {
                map.removeLayer(toolBoxLayer);

                _disableBBoxToolHandling(map);
                bboxMode = false;

                var eventArgs = {
                    northWest: toolBoxLayer.getBounds().getNorthWest(),
                    southEast: toolBoxLayer.getBounds().getSouthEast()
                };

                //console.log('bbox-args', eventArgs);

                map.fire('bbox', eventArgs);
            }
        });
    }

    this.connectLayerEvents = function (layer, objectReference) {
        this.mapEvents(layer, objectReference, {
            "add": "NotifyAdd",
            "remove": "NotifyRemove",
            "popupopen": "NotifyPopupOpen",
            "popupclose": "NotifyPopupClose",
            "tooltipopen": "NotifyTooltipOpen",
            "tooltipclose": "NotifyTooltipClose",
        });
    };

    this.connectInteractiveLayerEvents = function (interactiveLayer, objectReference) {
        this.connectLayerEvents(interactiveLayer, objectReference);
        this.connectInteractionEvents(interactiveLayer, objectReference);
    };

    this.connectMarkerEvents = function (marker, objectReference) {
        this.connectInteractiveLayerEvents(marker, objectReference);

        this.mapEvents(marker, objectReference, {
            "move": "NotifyMove",
            "dragstart": "NotifyDragStart",
            "movestart": "NotifyMoveStart",
            "drag": "NotifyDrag",
            "dragend": "NotifyDragEnd",
            "moveend": "NotifyMoveEnd",
        });
    };

    this.connectInteractionEvents = function (interactiveObject, objectReference) {
        this.mapEvents(interactiveObject, objectReference, {
            "click": "NotifyClick",
            "dblclick": "NotifyDblClick",
            "mousedown": "NotifyMouseDown",
            "mouseup": "NotifyMouseUp",
            "mouseover": "NotifyMouseOver",
            "mouseout": "NotifyMouseOut",
            "contextmenu": "NotifyContextMenu",
        });
    };

    this.resizeAllMaps = function () {
        for (var mapId in this.maps) {
            //console.log('refresh ' + mapId);
            if (this.maps[mapId]) {
                this.maps[mapId].invalidateSize();
            }
        }
    }
}();

window.gViewLeafletInterops = {
    create: function (map, crs, objectReference) {
        let mapOptions = {
            center: map.center,
            zoom: map.zoom,
            zoomControl: map.zoomControl,
            minZoom: map.minZoom ? map.minZoom : undefined,
            maxZoom: map.maxZoom ? map.maxZoom : undefined,
            maxBounds: map.maxBounds && map.maxBounds.item1 && map.maxBounds.item2 ? L.latLngBounds(map.maxBounds.item1, map.maxBounds.item2) : undefined,
            continuousWorld: map.continuousWorld,
            worldCopyJump: map.worldCopyJump,
            bounceAtZoomLimits: map.bounceAtZoomLimits
        }

        //console.log('crs', crs);
        if (crs) {
            let lCrs = new L.Proj.CRS('EPSG:' + crs.id, crs.proj4Parameters, {
                resolutions: crs.resolutions,
                origin: crs.origin
            });
            mapOptions.crs = lCrs;
            mapOptions.maxZoom = mapOptions.maxZoom || crs.resolutions.length;
        }

        //console.log('mapOptions', mapOptions);
        let leafletMap = L.map(map.id, mapOptions);

        gViewLeaflet.connectMapEvents(leafletMap, objectReference);
        gViewLeaflet.maps[map.id] = leafletMap;
        gViewLeaflet.layers[map.id] = [];
    },
    addTilelayer: function (mapId, tileLayer, objectReference) {
        let tileLayerOptions = {
            attribution: tileLayer.attribution,
            pane: tileLayer.pane,
            // ---
            tileSize: tileLayer.tileSize ? L.point(tileLayer.tileSize.width, tileLayer.tileSize.height) : undefined,
            opacity: tileLayer.opacity,
            updateWhenZooming: tileLayer.updateWhenZooming,
            updateInterval: tileLayer.updateInterval,
            zIndex: tileLayer.zIndex,
            bounds: tileLayer.bounds && tileLayer.bounds.item1 && tileLayer.bounds.item2 ? L.latLngBounds(tileLayer.bounds.item1, tileLayer.bounds.item2) : undefined,
            // ---
            continuousWorld: true,
            minZoom: tileLayer.minimumZoom,
            maxZoom: tileLayer.maximumZoom,
            maxNativeZoom: tileLayer.maxNativeZoom ? tileLayer.maxNativeZoom : tileLayer.maximumZoom,
            subdomains: tileLayer.subdomains,
            errorTileUrl: tileLayer.errorTileUrl,
            zoomOffset: tileLayer.zoomOffset,
            // TMS
            zoomReverse: tileLayer.isZoomReversed,
            detectRetina: tileLayer.detectRetina,
            // crossOrigin
        };

        //console.log('tileLayerOptions', tileLayerOptions);
        const layer = L.tileLayer(tileLayer.urlTemplate, tileLayerOptions);

        gViewLeaflet.addLayer(mapId, layer, tileLayer.id);
    },
    addImageLayer: function (mapId, image, objectReference) {
        const layerOptions = {
            ...gViewLeaflet.createInteractiveLayer(image),
            opacity: image.opacity,
            alt: image.alt,
            crossOrigin: image.crossOrigin,
            errorOverlayUrl: image.errorOverlayUrl,
            zIndex: image.zIndex,
            className: image.className
        };


        const southWest = L.latLng(image.southWest.lat, image.southWest.lng);
        const northEast = L.latLng(image.northEast.lat, image.northEast.lng);
        const bounds = L.latLngBounds(southWest, northEast);

        const imgLayer = L.imageOverlay(image.url, bounds, layerOptions);
        gViewLeaflet.addLayer(mapId, imgLayer, image.id);
    },
    removeLayer: function (mapId, layerId) {
        const remainingLayers = gViewLeaflet.layers[mapId].filter((layer) => layer.id !== layerId);
        const layersToBeRemoved = gViewLeaflet.layers[mapId].filter((layer) => layer.id === layerId);
        gViewLeaflet.layers[mapId] = remainingLayers;

        layersToBeRemoved.forEach(m => m.removeFrom(maps[mapId]));
    },
    fitBounds: function (mapId, southWest, northEast, padding, maxZoom) {
        const southWestLL = L.latLng(southWest.lat, southWest.lng);
        const northEastLL = L.latLng(northEast.lat, northEast.lng);
        const mapBounds = L.latLngBounds(southWestLL, northEastLL);
        gViewLeaflet.maps[mapId].fitBounds(mapBounds, {
            padding: padding == null ? null : L.point(padding.x, padding.y),
            maxZoom: maxZoom
        });
    },
    panTo: function (mapId, latLng, animate, duration, easeLinearity, noMoveStart) {
        const pos = L.latLng(latLng.lat, latLng.lng);
        gViewLeaflet.maps[mapId].panTo(pos, {
            animate: animate,
            duration: duration,
            easeLinearity: easeLinearity,
            noMoveStart: noMoveStart
        });
    },
    getCenter: function (mapId) {
        return gViewLeaflet.maps[mapId].getCenter();
    },
    getZoom: function (mapId) {
        return gViewLeaflet.maps[mapId].getZoom();
    },
    zoomIn: function (mapId, e) {
        const map = gViewLeaflet.maps[mapId];

        if (map.getZoom() < map.getMaxZoom()) {
            map.zoomIn(map.options.zoomDelta * (e.shiftKey ? 3 : 1));
        }
    },
    zoomOut: function (mapId, e) {
        const map = gViewLeaflet.maps[mapId];

        if (map.getZoom() > map.getMinZoom()) {
            map.zoomOut(map.options.zoomDelta * (e.shiftKey ? 3 : 1));
        }
    },
    getBounds: function (mapId) {
        return gViewLeaflet.maps[mapId].getBounds();
    },
    getImageSize: function (mapId) {
        return gViewLeaflet.maps[mapId].getSize();
    },
    destroy: function (mapId) {
        const map = gViewLeaflet.maps[mapId];
        if (map) {
            map.remove();
            gViewLeaflet.maps[mapId] = null;
        }
    },
    refresh: function (mapId) {
        const map = gViewLeaflet.maps[mapId];
        if (map) {
            //console.log('refresh map');
            map.invalidateSize();
        }
    },
    updateImageLayer: function (mapId, layerId, url, southWest, northEast) {
        let layer = gViewLeaflet.layers[mapId].find(l => l.id === layerId);
        if (layer !== undefined) {
            if (url) {
                //console.log(layer, url);
                layer.setUrl(url);
            }
            if (southWest && northEast) {
                const southWestLL = L.latLng(southWest.lat, southWest.lng);
                const northEastLL = L.latLng(northEast.lat, northEast.lng);
                const bounds = L.latLngBounds(southWestLL, northEastLL);

                layer.setBounds(bounds);
            }
        }
    },
}
