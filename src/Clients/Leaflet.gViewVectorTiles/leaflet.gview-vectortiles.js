/*!
 * Leaflet.gViewVectorTiles
 *
 * Mapbox Vector Tiles (MVT) in Leaflet, styled with GL styles:
 * - gView vector tile caches in any projection, any scales and any tile size (`gview:grid` in the style):
 *   the map's CRS must be the projection of the cache, its zoom levels don't have to match the cache scales.
 * - classic WebMercator vector tiles (z/x/y; tiles, TileJSON or ESRI VectorTileServer sources, e.g. basemap.at)
 *   in WebMercator maps.
 *
 * - Needs only Leaflet (1.9). Maps in another projection than WebMercator use Proj4Leaflet as usual.
 * - No other dependencies: protobuf and MVT are decoded here.
 *
 * Usage:
 *   L.gViewVectorTiles('https://server/gview/vtc/myfolder/myservice').addTo(map);   // gView service
 *   L.gViewVectorTiles('https://example.com/styles/root.json').addTo(map);          // any GL style
 *
 * License: Apache-2.0 (as gView GIS)
 */
(function (factory) {
    if (typeof define === 'function' && define.amd) {
        define(['leaflet'], factory);
    } else if (typeof module === 'object' && module.exports) {
        module.exports = factory(require('leaflet'));
    } else {
        factory(window.L);
    }
}(function (L) {
    'use strict';

    // ------------------------------------------------------------------------------------------
    // Protobuf / MVT decoding
    // ------------------------------------------------------------------------------------------

    function Pbf(bytes) {
        this.buf = bytes;
        this.pos = 0;
        this.length = bytes.length;
        this.view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    }

    Pbf.prototype = {
        readVarint: function () {
            var result = 0, shift = 0, b;
            do {
                b = this.buf[this.pos++];
                // beyond 28 bits: multiply, bit operations are 32 bit in JS
                result += shift < 28 ? (b & 0x7f) << shift : (b & 0x7f) * Math.pow(2, shift);
                shift += 7;
            } while (b >= 0x80);
            return result;
        },
        readSVarint: function () {
            var n = this.readVarint();
            return n % 2 === 1 ? (n + 1) / -2 : n / 2;
        },
        readBytes: function () {
            var end = this.readVarint() + this.pos;
            var bytes = this.buf.subarray(this.pos, end);
            this.pos = end;
            return bytes;
        },
        readString: function () {
            return new TextDecoder('utf-8').decode(this.readBytes());
        },
        readFloat: function () { var v = this.view.getFloat32(this.pos, true); this.pos += 4; return v; },
        readDouble: function () { var v = this.view.getFloat64(this.pos, true); this.pos += 8; return v; },
        readPacked: function () {
            var end = this.readVarint() + this.pos, values = [];
            while (this.pos < end) values.push(this.readVarint());
            return values;
        },
        skip: function (type) {
            if (type === 0) this.readVarint();
            else if (type === 1) this.pos += 8;
            else if (type === 2) this.pos = this.readVarint() + this.pos;
            else if (type === 5) this.pos += 4;
            else throw new Error('Unsupported protobuf wire type ' + type);
        },
        // calls fn(tag, type) for every field of the message ending at end
        fields: function (end, fn) {
            while (this.pos < end) {
                var key = this.readVarint(), tag = key >> 3, type = key & 7, before = this.pos;
                if (fn(tag, type) === false) this.skip(type);
                else if (this.pos === before) this.skip(type);
            }
        }
    };

    var GEOMETRY_TYPES = { 1: 'Point', 2: 'LineString', 3: 'Polygon' };

    function decodeValue(pbf) {
        var end = pbf.readVarint() + pbf.pos, value = null;
        pbf.fields(end, function (tag) {
            switch (tag) {
                case 1: value = pbf.readString(); break;
                case 2: value = pbf.readFloat(); break;
                case 3: value = pbf.readDouble(); break;
                case 4: value = pbf.readVarint(); break;   // int64 (non-negative range is enough here)
                case 5: value = pbf.readVarint(); break;
                case 6: value = pbf.readSVarint(); break;
                case 7: value = pbf.readVarint() !== 0; break;
                default: return false;
            }
        });
        return value;
    }

    // MVT geometry commands → arrays of [x, y] paths (tile coordinates)
    function decodeGeometry(commands) {
        var paths = [], path = null, x = 0, y = 0, i = 0;
        while (i < commands.length) {
            var cmd = commands[i] & 0x7, count = commands[i] >> 3;
            i++;
            if (cmd === 1 || cmd === 2) {
                for (var c = 0; c < count; c++) {
                    var dx = commands[i++], dy = commands[i++];
                    x += (dx >> 1) ^ (-(dx & 1));
                    y += (dy >> 1) ^ (-(dy & 1));
                    if (cmd === 1) {
                        path = [[x, y]];
                        paths.push(path);
                    } else {
                        path.push([x, y]);
                    }
                }
            } else if (cmd === 7 && path) {
                path.closed = true;
            }
        }
        return paths;
    }

    function decodeTile(arrayBuffer) {
        var pbf = new Pbf(new Uint8Array(arrayBuffer)), layers = {};

        pbf.fields(pbf.length, function (tag) {
            if (tag !== 3) return false;

            var end = pbf.readVarint() + pbf.pos;
            var layer = { name: '', extent: 4096, keys: [], values: [], features: [] }, rawFeatures = [];

            pbf.fields(end, function (ltag) {
                switch (ltag) {
                    case 1: layer.name = pbf.readString(); break;
                    case 2: rawFeatures.push(pbf.readBytes()); break;
                    case 3: layer.keys.push(pbf.readString()); break;
                    case 4: layer.values.push(decodeValue(pbf)); break;
                    case 5: layer.extent = pbf.readVarint(); break;
                    default: return false;
                }
            });

            rawFeatures.forEach(function (bytes) {
                var fp = new Pbf(bytes), feature = { id: null, type: 'Unknown', properties: {}, geometry: [] };
                fp.fields(fp.length, function (ftag) {
                    switch (ftag) {
                        case 1: feature.id = fp.readVarint(); break;
                        case 2:
                            var tags = fp.readPacked();
                            for (var t = 0; t + 1 < tags.length; t += 2) {
                                feature.properties[layer.keys[tags[t]]] = layer.values[tags[t + 1]];
                            }
                            break;
                        case 3: feature.type = GEOMETRY_TYPES[fp.readVarint()] || 'Unknown'; break;
                        case 4: feature.geometry = decodeGeometry(fp.readPacked()); break;
                        default: return false;
                    }
                });
                layer.features.push(feature);
            });

            layers[layer.name] = layer;
        });

        return layers;
    }

    // ------------------------------------------------------------------------------------------
    // GL style expressions (the subset gView generates, plus a few common ones)
    // ------------------------------------------------------------------------------------------

    function evaluate(expr, feature, zoom) {
        if (!Array.isArray(expr)) return expr;

        var op = expr[0], props = feature ? feature.properties : {};
        var arg = function (i) { return evaluate(expr[i], feature, zoom); };

        switch (op) {
            case 'get': return props[expr[1]] === undefined ? null : props[expr[1]];
            case 'has': return expr[1] in props;
            case '!has': return !(expr[1] in props);
            case 'literal': return expr[1];
            case 'zoom': return zoom;
            case 'geometry-type': return feature ? feature.type : null;
            case '!': return !arg(1);
            case 'all': for (var a = 1; a < expr.length; a++) if (!arg(a)) return false; return true;
            case 'any': for (var b = 1; b < expr.length; b++) if (arg(b)) return true; return false;
            case '==': return arg(1) === arg(2);
            case '!=': return arg(1) !== arg(2);
            case '>': return arg(1) > arg(2);
            case '>=': return arg(1) >= arg(2);
            case '<': return arg(1) < arg(2);
            case '<=': return arg(1) <= arg(2);
            case 'in':
                var haystack = arg(2);
                return Array.isArray(haystack) ? haystack.indexOf(arg(1)) >= 0 : false;
            case 'to-string':
                var s = arg(1);
                return s === null || s === undefined ? '' : String(s);
            case 'to-number':
                var n = Number(arg(1));
                return isNaN(n) ? 0 : n;
            case 'concat':
                var text = '';
                for (var c = 1; c < expr.length; c++) { var v = arg(c); text += v === null || v === undefined ? '' : String(v); }
                return text;
            case 'match':
                var input = arg(1);
                for (var m = 2; m + 1 < expr.length; m += 2) {
                    var labels = Array.isArray(expr[m]) ? expr[m] : [expr[m]];
                    if (labels.indexOf(input) >= 0) return arg(m + 1);
                }
                return arg(expr.length - 1);
            case 'case':
                for (var k = 1; k + 1 < expr.length; k += 2) if (arg(k)) return arg(k + 1);
                return arg(expr.length - 1);
            case 'step':
                var stepInput = arg(1), result = arg(2);
                for (var st = 3; st + 1 < expr.length; st += 2) {
                    if (stepInput >= expr[st]) result = arg(st + 1); else break;
                }
                return result;
            case 'interpolate':
                var x = arg(2), stops = [];
                for (var p = 3; p + 1 < expr.length; p += 2) stops.push([expr[p], arg(p + 1)]);
                if (!stops.length) return null;
                if (x <= stops[0][0]) return stops[0][1];
                if (x >= stops[stops.length - 1][0]) return stops[stops.length - 1][1];
                for (var q = 0; q < stops.length - 1; q++) {
                    if (x >= stops[q][0] && x <= stops[q + 1][0]) {
                        var t = (x - stops[q][0]) / (stops[q + 1][0] - stops[q][0]);
                        if (expr[1] && expr[1][0] === 'exponential' && expr[1][1] !== 1) {
                            var base = expr[1][1];
                            t = (Math.pow(base, t * (stops[q + 1][0] - stops[q][0])) - 1) / (Math.pow(base, stops[q + 1][0] - stops[q][0]) - 1);
                        }
                        return stops[q][1] + t * (stops[q + 1][1] - stops[q][1]);
                    }
                }
                return stops[stops.length - 1][1];
            default:
                // an array literal (dasharray, offsets, ...)
                return expr;
        }
    }

    // legacy GL functions: { stops: [[zoom, value], ...], base, type, property, default }
    function evaluateFunction(fn, feature, zoom) {
        var stops = fn.stops || [];
        var input = fn.property ? (feature ? feature.properties[fn.property] : undefined) : zoom;

        if (fn.type === 'identity') return input === undefined ? fn['default'] : input;
        if (!stops.length) return fn['default'];

        // zoom-and-property stops ({ zoom, value }): use the zoom part only
        var key = function (s) { return typeof s[0] === 'object' && s[0] !== null ? s[0].zoom : s[0]; };

        if (fn.type === 'categorical') {
            for (var c = 0; c < stops.length; c++) if (key(stops[c]) === input) return stops[c][1];
            return fn['default'];
        }

        var numeric = typeof stops[0][1] === 'number';
        if (fn.type === 'interval' || !numeric || typeof input !== 'number') {
            var result = stops[0][1];
            for (var i = 0; i < stops.length; i++) { if (input >= key(stops[i])) result = stops[i][1]; else break; }
            return result;
        }

        // exponential (base 1 = linear)
        if (input <= key(stops[0])) return stops[0][1];
        if (input >= key(stops[stops.length - 1])) return stops[stops.length - 1][1];
        var base = fn.base || 1;
        for (var j = 0; j < stops.length - 1; j++) {
            var z0 = key(stops[j]), z1 = key(stops[j + 1]);
            if (input >= z0 && input <= z1) {
                var t = base === 1 ? (input - z0) / (z1 - z0) : (Math.pow(base, input - z0) - 1) / (Math.pow(base, z1 - z0) - 1);
                return stops[j][1] + t * (stops[j + 1][1] - stops[j][1]);
            }
        }
        return stops[stops.length - 1][1];
    }

    function evaluateValue(value, feature, zoom) {
        if (value && typeof value === 'object' && !Array.isArray(value) && value.stops) {
            return evaluateFunction(value, feature, zoom);
        }
        return evaluate(value, feature, zoom);
    }

    // "{field}" tokens of legacy text-field / icon-image values
    function resolveTokens(value, feature) {
        if (typeof value !== 'string' || value.indexOf('{') < 0) return value;
        return value.replace(/\{([^}]+)\}/g, function (_, name) {
            var v = feature.properties[name];
            return v === null || v === undefined ? '' : String(v);
        });
    }

    var LEGACY_COMPARISONS = ['==', '!=', '>', '>=', '<', '<=', 'in', '!in', 'has', '!has'];

    // legacy filters (["==", "field", value], ["all", [...], ...]) or expressions
    function isLegacyFilter(filter) {
        if (!Array.isArray(filter)) return false;
        var op = filter[0];
        if (op === 'all' || op === 'any' || op === 'none') {
            return filter.length > 1 && filter.slice(1).every(isLegacyFilter);
        }
        return LEGACY_COMPARISONS.indexOf(op) >= 0 && typeof filter[1] === 'string';
    }

    function legacyFilter(filter, feature) {
        var op = filter[0];
        switch (op) {
            case 'all': for (var a = 1; a < filter.length; a++) if (!legacyFilter(filter[a], feature)) return false; return true;
            case 'any': for (var b = 1; b < filter.length; b++) if (legacyFilter(filter[b], feature)) return true; return false;
            case 'none': for (var c = 1; c < filter.length; c++) if (legacyFilter(filter[c], feature)) return false; return true;
            case 'has': return filter[1] in feature.properties;
            case '!has': return !(filter[1] in feature.properties);
        }
        var value = filter[1] === '$type' ? feature.type : filter[1] === '$id' ? feature.id : feature.properties[filter[1]];
        switch (op) {
            case '==': return value === filter[2];
            case '!=': return value !== filter[2];
            case '>': return value > filter[2];
            case '>=': return value >= filter[2];
            case '<': return value < filter[2];
            case '<=': return value <= filter[2];
            case 'in': return filter.slice(2).indexOf(value) >= 0;
            case '!in': return filter.slice(2).indexOf(value) < 0;
        }
        return false;
    }

    function filterMatches(filter, feature, zoom) {
        if (!filter) return true;
        return isLegacyFilter(filter) ? legacyFilter(filter, feature) : !!evaluate(filter, feature, zoom);
    }

    function filterUsesZoom(filter) {
        return JSON.stringify(filter || null).indexOf('"zoom"') >= 0;
    }

    // true if a paint/layout value depends on feature attributes (then features are drawn one by one)
    function isDataDriven(value) {
        if (value === null || typeof value !== 'object') return false;
        if (!Array.isArray(value)) return !!value.property || value.type === 'identity';
        var json = JSON.stringify(value);
        return json.indexOf('"get"') >= 0 || json.indexOf('"has"') >= 0 || json.indexOf('"properties"') >= 0 ||
               json.indexOf('"geometry-type"') >= 0 || json.indexOf('"id"') >= 0 || json.indexOf('"feature-state"') >= 0;
    }

    function layerIsDataDriven(styleLayer) {
        if (styleLayer._gvDataDriven === undefined) {
            var paint = styleLayer.paint || {}, layout = styleLayer.layout || {}, driven = false;
            Object.keys(paint).forEach(function (k) { if (isDataDriven(paint[k])) driven = true; });
            Object.keys(layout).forEach(function (k) { if (isDataDriven(layout[k])) driven = true; });
            // cached on the style layer object (not part of the GL style)
            Object.defineProperty(styleLayer, '_gvDataDriven', { value: driven, enumerable: false });
        }
        return styleLayer._gvDataDriven;
    }

    // ------------------------------------------------------------------------------------------
    // Fonts, colors
    // ------------------------------------------------------------------------------------------

    function cssFont(textFont, size) {
        var name = (Array.isArray(textFont) ? textFont[0] : textFont) || 'Arial Regular';
        var words = String(name).split(' '), weight = '', style = '';
        while (words.length > 1) {
            var last = words[words.length - 1].toLowerCase();
            if (last === 'bold' || last === 'semibold' || last === 'black' || last === 'heavy') weight = 'bold ';
            else if (last === 'italic' || last === 'oblique') style = 'italic ';
            else if (['regular', 'normal', 'book', 'medium', 'light'].indexOf(last) < 0) break;
            words.pop();
        }
        return style + weight + Math.round(size * 10) / 10 + 'px "' + words.join(' ') + '", sans-serif';
    }

    var ANCHORS = {
        'center': [-0.5, -0.5], 'top': [-0.5, 0], 'bottom': [-0.5, -1], 'left': [0, -0.5], 'right': [-1, -0.5],
        'top-left': [0, 0], 'top-right': [-1, 0], 'bottom-left': [0, -1], 'bottom-right': [-1, -1]
    };

    // adds a path to the current canvas path, leaving out vertices closer than half a pixel to the last one
    // (detailed polygons at small scales: invisible, but expensive to rasterize)
    function addPath(ctx, path, tx, ty, close) {
        var n = path.length;
        if (!n) return;
        var lastX = tx(path[0]), lastY = ty(path[0]);
        ctx.moveTo(lastX, lastY);
        for (var i = 1; i < n; i++) {
            var x = tx(path[i]), y = ty(path[i]);
            if (i < n - 1 && Math.abs(x - lastX) < 0.5 && Math.abs(y - lastY) < 0.5) continue;
            ctx.lineTo(x, y);
            lastX = x; lastY = y;
        }
        if (close) ctx.closePath();
    }

    // glyphs of a text along a screen path, centered at distance center: [{ ch, x, y, angle, width }] or null
    // (doesn't fit, or the line bends more than maxAngle between neighbor glyphs). Kept upright: the path is
    // walked backwards when the text would run right to left.
    function placeAlongPath(ctx, text, path, center, maxAngle, letterSpacing) {
        var cumulative = [0];
        for (var i = 1; i < path.length; i++) {
            cumulative.push(cumulative[i - 1] + Math.hypot(path[i][0] - path[i - 1][0], path[i][1] - path[i - 1][1]));
        }
        var length = cumulative[cumulative.length - 1];

        var chars = Array.from(text);
        var widths = chars.map(function (ch) { return ctx.measureText(ch).width + letterSpacing; });
        var total = widths.reduce(function (a, b) { return a + b; }, 0) - letterSpacing;
        if (total > length) return null;

        function pointAt(distance, reversed) {
            var d = reversed ? length - distance : distance;
            for (var j = 1; j < path.length; j++) {
                if (cumulative[j] >= d) {
                    var segment = cumulative[j] - cumulative[j - 1] || 1, t = (d - cumulative[j - 1]) / segment;
                    var a = path[j - 1], b = path[j];
                    var angle = Math.atan2(b[1] - a[1], b[0] - a[0]);
                    if (reversed) angle += Math.PI;
                    return { x: a[0] + t * (b[0] - a[0]), y: a[1] + t * (b[1] - a[1]), angle: angle };
                }
            }
            return null;
        }

        // upright: the direction at the center decides
        var mid = pointAt(center, false);
        if (!mid) return null;
        var reversed = Math.cos(mid.angle) < 0;
        var start = (reversed ? length - center : center) - total / 2;
        if (start < 0 || start + total > length) return null;

        var glyphs = [], run = start, previous = null;
        for (var g = 0; g < chars.length; g++) {
            var at = pointAt(run + widths[g] / 2, reversed);
            if (!at) return null;
            if (previous !== null) {
                var bend = Math.abs(Math.atan2(Math.sin(at.angle - previous), Math.cos(at.angle - previous)));
                if (bend > maxAngle) return null;
            }
            previous = at.angle;
            glyphs.push({ ch: chars[g], x: at.x, y: at.y, angle: at.angle, width: widths[g] - letterSpacing });
            run += widths[g];
        }
        return glyphs;
    }

    var WEBMERCATOR_HALF = 20037508.342789244;
    var FILL_BATCH = 16;
    var WEBMERCATOR_RES0_512 = 2 * WEBMERCATOR_HALF / 512;   // style zoom 0 (MapLibre convention: 512 px world)

    function resolveUrl(url, base) {
        try {
            // URL encodes the {z}/{x}/{y} placeholders: decode them again
            return new URL(url, base).href.replace(/%7B/gi, '{').replace(/%7D/gi, '}');
        } catch (e) {
            return url;
        }
    }

    // ------------------------------------------------------------------------------------------
    // The layer
    // ------------------------------------------------------------------------------------------

    var GViewVectorTiles = L.Layer.extend({
        options: {
            // EPSG code of a gView cache; default: the map's crs.code
            epsg: null,
            // a style URL (any GL style), instead of {url}/style.json of a gView service
            style: null,
            pane: 'overlayPane',
            // labels and icons: hide those overlapping earlier ones
            declutter: true,
            // draw the tile borders and coordinates
            debug: false,
            // tiles kept in memory (all sources)
            maxCachedTiles: 384,
            // fetch options, e.g. { credentials: 'include' } or { headers: { Authorization: ... } }
            fetchOptions: {},
            attribution: null
        },

        /**
         * @param url  the vtc base of a gView service (https://server/gview/vtc/[folder/]service)
         *             or the URL of any GL style (…/style.json, …/root.json)
         */
        initialize: function (url, options) {
            L.setOptions(this, options);
            url = url || '';
            if (/\.json(\?|$)/i.test(url) && !this.options.style) {
                this.options.style = url;
                url = '';
            }
            this._url = url.replace(/\/+$/, '');
            this._tiles = {};        // key → { status, layers, used, filtered }
            this._tileOrder = [];
            this._style = null;
            this._sources = {};      // id → { grid, zooms, tileUrl, gview, minLevel, maxLevel }
            this._sprite = null;     // { index, image, pixelRatio }
            this._patterns = {};
            this._tinted = {};
            this._images = {};       // tile images: key → { canvas, zoomKey, entry }
        },

        getEvents: function () {
            var events = {
                viewreset: this._reset,
                zoom: this._onZoom,
                moveend: this._update,
                zoomend: this._update,
                resize: this._resize
            };
            if (this._map.options.zoomAnimation) {
                events.zoomanim = this._onAnimZoom;
            }
            return events;
        },

        onAdd: function (map) {
            this._canvas = L.DomUtil.create('canvas', 'leaflet-gview-vectortiles leaflet-zoom-animated');
            this._canvas.style.pointerEvents = 'none';
            this.getPane().appendChild(this._canvas);
            this._ctx = this._canvas.getContext('2d');

            this._resize();
            this._ready = this._ready || this._loadStyle();
            this._ready.then(L.bind(this._update, this), L.bind(function (error) {
                this.fire('error', { error: error });
                if (window.console) console.error('gViewVectorTiles:', error);
            }, this));
        },

        onRemove: function () {
            L.DomUtil.remove(this._canvas);
            this._canvas = this._ctx = null;
        },

        getAttribution: function () {
            return this.options.attribution;
        },

        /** The loaded GL style (after the 'load' event). */
        getStyle: function () { return this._style; },

        /** The tile grid of the (first) gView source: { epsg, origin, resolutions, tileSize, extent, webmercator }. */
        getGrid: function () {
            for (var id in this._sources) if (this._sources[id].gview) return this._sources[id].grid;
            var first = this._sources[Object.keys(this._sources)[0]];
            return first ? first.grid : null;
        },

        // ---------------------------------------------------------------- style, sources, sprite

        _fetch: function (url) {
            return fetch(url, this.options.fetchOptions).then(function (response) {
                if (!response.ok) throw new Error(response.status + ' ' + url);
                return response;
            });
        },

        _fetchJson: function (url) {
            var self = this;
            return this._fetch(url).then(function (r) { return r.text(); }).then(function (text) {
                try {
                    return JSON.parse(text);
                } catch (e) {
                    // an ESRI service root answers HTML without f=json
                    if (url.indexOf('f=json') < 0) {
                        return self._fetchJson(url + (url.indexOf('?') < 0 ? '?' : '&') + 'f=json');
                    }
                    throw new Error('No JSON: ' + url);
                }
            });
        },

        _loadStyle: function () {
            var epsg = this.options.epsg || this._mapEpsg();
            var styleUrl = this.options.style || (this._url + '/style.json' + (epsg ? '?epsg=' + epsg : ''));
            styleUrl = resolveUrl(styleUrl, window.location.href);
            var self = this;

            return this._fetchJson(styleUrl)
                .then(function (style) {
                    self._style = style;
                    var gviewGrid = (style.metadata || {})['gview:grid'] || null;

                    var used = {};
                    style.layers.forEach(function (l) { if (l.source) used[l.source] = true; });

                    return Promise.all(Object.keys(style.sources || {})
                        .filter(function (id) { return used[id] && style.sources[id].type === 'vector'; })
                        .map(function (id) { return self._loadSource(id, style.sources[id], styleUrl, gviewGrid); }));
                })
                .then(function () {
                    var sprite = self._style.sprite;
                    return sprite && typeof sprite === 'string' ? self._loadSprite(resolveUrl(sprite, styleUrl)) : null;
                })
                .then(function () { self.fire('load', { style: self._style }); });
        },

        _loadSource: function (id, source, styleUrl, gviewGrid) {
            var self = this, mapEpsg = this._mapEpsg();

            // gView cache: its own grid (any projection)
            if (gviewGrid && source.tiles) {
                if (mapEpsg && String(gviewGrid.epsg) !== String(mapEpsg)) {
                    return Promise.reject(new Error('The map is EPSG:' + mapEpsg + ', the tile cache EPSG:' + gviewGrid.epsg));
                }
                var zooms = gviewGrid.webmercator
                    ? gviewGrid.resolutions.map(function (r) { return Math.round(Math.log(156543.03392804097 / (r * gviewGrid.tileSize[0] / 256)) / Math.LN2); })
                    : gviewGrid.resolutions.map(function (r, i) { return i; });
                this._sources[id] = {
                    gview: true, grid: gviewGrid, zooms: zooms, tileUrl: resolveUrl(source.tiles[0], styleUrl),
                    minLevel: 0, maxLevel: gviewGrid.resolutions.length - 1
                };
                return Promise.resolve();
            }

            // classic source: WebMercator z/x/y, tiles direct or from a TileJSON / ESRI VectorTileServer
            if (mapEpsg && mapEpsg !== '3857' && mapEpsg !== '900913') {
                return Promise.reject(new Error('Source "' + id + '" is a WebMercator vector tile source, the map is EPSG:' + mapEpsg));
            }

            var resolved = source.tiles
                ? Promise.resolve({ tiles: source.tiles.map(function (t) { return resolveUrl(t, styleUrl); }), info: source })
                : this._fetchJson(resolveUrl(source.url, styleUrl)).then(function (info) {
                    var base = resolveUrl(source.url, styleUrl);
                    if (!/\/$/.test(base.split('?')[0])) base = base.split('?')[0] + '/';
                    return { tiles: (info.tiles || []).map(function (t) { return resolveUrl(t, base); }), info: info };
                });

            return resolved.then(function (r) {
                var info = r.info || {};
                var tileSize = source.tileSize || (info.tileInfo && info.tileInfo.rows) || info.tileSize || 512;
                var minLevel = source.minzoom !== undefined ? source.minzoom : (info.minzoom || 0);
                // ESRI: tiles exist up to maxLOD (the style's maxzoom is the display limit)
                var maxLevel = info.maxLOD !== undefined ? info.maxLOD
                    : source.maxzoom !== undefined ? source.maxzoom
                    : info.maxzoom !== undefined ? info.maxzoom : 14;

                var resolutions = [], zooms = [];
                for (var z = 0; z <= maxLevel; z++) {
                    resolutions.push(2 * WEBMERCATOR_HALF / (tileSize * Math.pow(2, z)));
                    zooms.push(z + Math.log(tileSize / 512) / Math.LN2);
                }

                if (!r.tiles.length) throw new Error('Source "' + id + '" has no tile URLs');

                self._sources[id] = {
                    gview: false,
                    grid: {
                        epsg: 3857, origin: [-WEBMERCATOR_HALF, WEBMERCATOR_HALF], resolutions: resolutions,
                        tileSize: [tileSize, tileSize], extent: [-WEBMERCATOR_HALF, -WEBMERCATOR_HALF, WEBMERCATOR_HALF, WEBMERCATOR_HALF],
                        webmercator: true
                    },
                    zooms: zooms, tileUrl: r.tiles[0], minLevel: minLevel, maxLevel: maxLevel
                };
            });
        },

        _loadSprite: function (spriteUrl) {
            var ratio = (window.devicePixelRatio || 1) > 1 ? 2 : 1, suffix = ratio === 2 ? '@2x' : '';
            var self = this;
            return this._fetchJson(spriteUrl + suffix + '.json')
                .then(function (index) {
                    return new Promise(function (resolve) {
                        var image = new Image();
                        image.crossOrigin = 'anonymous';
                        image.onload = function () { self._sprite = { index: index, image: image, pixelRatio: ratio }; resolve(); };
                        image.onerror = function () { resolve(); };   // draw without sprites
                        image.src = spriteUrl + suffix + '.png';
                    });
                })
                .catch(function () { /* no sprites */ });
        },

        _mapEpsg: function () {
            var code = this._map && this._map.options.crs && this._map.options.crs.code;
            var match = /EPSG:(\d+)/i.exec(code || '');
            return match ? match[1] : null;
        },

        // ---------------------------------------------------------------- canvas placement (as L.Renderer)

        _resize: function () {
            if (!this._canvas) return;
            var size = this._map.getSize(), ratio = window.devicePixelRatio || 1;
            this._canvas.width = Math.round(size.x * ratio);
            this._canvas.height = Math.round(size.y * ratio);
            this._canvas.style.width = size.x + 'px';
            this._canvas.style.height = size.y + 'px';
            this._update();
        },

        _reset: function () { this._update(); },

        _onZoom: function () {
            this._updateTransform(this._map.getCenter(), this._map.getZoom());
        },

        _onAnimZoom: function (e) {
            this._updateTransform(e.center, e.zoom);
        },

        _updateTransform: function (center, zoom) {
            if (!this._center) return;
            var map = this._map,
                scale = map.getZoomScale(zoom, this._zoom),
                viewHalf = map.getSize().multiplyBy(0.5),
                currentCenterPoint = map.project(this._center, zoom),
                topLeftOffset = viewHalf.multiplyBy(-scale).add(currentCenterPoint).subtract(map._getNewPixelOrigin(center, zoom));

            if (L.Browser.any3d) {
                L.DomUtil.setTransform(this._canvas, topLeftOffset, scale);
            } else {
                L.DomUtil.setPosition(this._canvas, topLeftOffset);
            }
        },

        _validResolution: function () {
            var res = this._mapResolution();
            return isFinite(res) && res > 0;
        },

        _update: function () {
            if (!this._map || !this._canvas || !this._style || !this._validResolution()) return;
            var map = this._map;
            this._center = map.getCenter();
            this._zoom = map.getZoom();
            L.DomUtil.setPosition(this._canvas, map.containerPointToLayerPoint([0, 0]));
            this._requestTiles();
            this._redraw();
        },

        // ---------------------------------------------------------------- grid math

        // map pixel resolution in CRS units per pixel (Leaflet's EPSG:3857 has a scale factor in its transformation)
        _mapResolution: function () {
            var crs = this._map.options.crs;
            var factor = crs.transformation && crs.transformation._a ? Math.abs(crs.transformation._a) : 1;
            return 1 / (crs.scale(this._map.getZoom()) * factor);
        },

        // the level to draw: nearest resolution (as OpenLayers), within the source's tile levels
        _level: function (source, resolution) {
            if (!source.gview) {
                // GL clients load the tiles of floor(zoom), and classic styles are made for that: a style layer
                // starting at zoom 16 shows data that only the level 16 tiles contain
                var zoom = this._styleZoom(source, resolution);
                var level = source.minLevel;
                for (var z = source.minLevel; z <= source.maxLevel && z < source.zooms.length; z++) {
                    if (source.zooms[z] <= zoom + 1e-6) level = z;
                }
                return level;
            }

            var res = source.grid.resolutions, best = source.minLevel, bestDiff = Infinity;
            for (var i = source.minLevel; i <= source.maxLevel && i < res.length; i++) {
                var diff = Math.abs(Math.log(res[i] / resolution));
                if (diff < bestDiff) { bestDiff = diff; best = i; }
            }
            return best;
        },

        // the (fractional) style zoom of a resolution
        _styleZoom: function (source, resolution) {
            if (!source.gview) {
                // MapLibre convention, independent of the tile size
                return Math.log(WEBMERCATOR_RES0_512 / resolution) / Math.LN2;
            }
            var res = source.grid.resolutions, zooms = source.zooms, n = res.length;
            if (n === 1) return zooms[0];
            if (resolution >= res[0]) return zooms[0] - Math.log(resolution / res[0]) / Math.log(res[0] / res[1]) * (zooms[1] - zooms[0]);
            for (var i = 0; i < n - 1; i++) {
                if (resolution <= res[i] && resolution >= res[i + 1]) {
                    return zooms[i] + Math.log(res[i] / resolution) / Math.log(res[i] / res[i + 1]) * (zooms[i + 1] - zooms[i]);
                }
            }
            return zooms[n - 1] + Math.log(res[n - 1] / resolution) / Math.log(res[n - 2] / res[n - 1]) * (zooms[n - 1] - zooms[n - 2]);
        },

        // world (CRS) → canvas pixel
        _toCanvas: function (x, y) {
            var map = this._map, crs = map.options.crs;
            var p = crs.transformation.transform(L.point(x, y), crs.scale(map.getZoom()));
            return p.subtract(map.getPixelOrigin()).subtract(map.containerPointToLayerPoint([0, 0]));
        },

        // canvas pixel → world (CRS)
        _toWorld: function (px, py) {
            var map = this._map, crs = map.options.crs;
            var p = L.point(px, py).add(map.containerPointToLayerPoint([0, 0])).add(map.getPixelOrigin());
            return crs.transformation.untransform(p, crs.scale(map.getZoom()));
        },

        _tileEnvelope: function (grid, level, row, col) {
            var res = grid.resolutions[level];
            var w = grid.tileSize[0] * res, h = grid.tileSize[1] * res;
            var minX = grid.origin[0] + col * w, maxY = grid.origin[1] - row * h;
            return { minX: minX, minY: maxY - h, maxX: minX + w, maxY: maxY };
        },

        _visibleTiles: function (grid, level) {
            var size = this._map.getSize(), res = grid.resolutions[level];
            var a = this._toWorld(0, 0), b = this._toWorld(size.x, size.y);
            var minX = Math.min(a.x, b.x), maxX = Math.max(a.x, b.x), minY = Math.min(a.y, b.y), maxY = Math.max(a.y, b.y);

            if (grid.extent) {
                minX = Math.max(minX, grid.extent[0]); minY = Math.max(minY, grid.extent[1]);
                maxX = Math.min(maxX, grid.extent[2]); maxY = Math.min(maxY, grid.extent[3]);
                if (minX >= maxX || minY >= maxY) return [];
            }

            var w = grid.tileSize[0] * res, h = grid.tileSize[1] * res;
            var col0 = Math.floor((minX - grid.origin[0]) / w), col1 = Math.floor((maxX - grid.origin[0]) / w);
            var row0 = Math.floor((grid.origin[1] - maxY) / h), row1 = Math.floor((grid.origin[1] - minY) / h);

            var tiles = [];
            if ((col1 - col0 + 1) * (row1 - row0 + 1) > 400) return tiles;   // safety: zoomed far out
            for (var r = row0; r <= row1; r++) {
                for (var c = col0; c <= col1; c++) {
                    if (r >= 0 && c >= 0) tiles.push({ level: level, row: r, col: c });
                }
            }
            return tiles;
        },

        // ---------------------------------------------------------------- tile loading

        _key: function (sourceId, t) { return sourceId + '|' + t.level + '/' + t.row + '/' + t.col; },

        _tileUrlFor: function (source, t) {
            // {z}: the scale index of a custom gView grid, the zoom level of a WebMercator cache
            var z = source.gview ? source.zooms[t.level] : t.level;
            return source.tileUrl
                .replace('{z}', z)
                .replace('{x}', t.col)
                .replace('{y}', t.row);
        },

        _requestTiles: function () {
            var resolution = this._mapResolution(), self = this;
            Object.keys(this._sources).forEach(function (id) {
                var source = self._sources[id];
                var level = self._level(source, resolution);
                self._visibleTiles(source.grid, level).forEach(function (t) {
                    var key = self._key(id, t);
                    if (self._tiles[key]) {
                        self._tiles[key].used = Date.now();
                        return;
                    }

                    var entry = self._tiles[key] = { status: 'loading', layers: null, used: Date.now(), filtered: {} };
                    self._tileOrder.push(key);

                    fetch(self._tileUrlFor(source, t), self.options.fetchOptions)
                        .then(function (r) {
                            // 204 (gView) and 404 (ESRI, outside the data) are empty tiles
                            if (r.status === 204 || r.status === 404) return new ArrayBuffer(0);
                            if (!r.ok) throw new Error(r.status);
                            return r.arrayBuffer();
                        })
                        .then(function (buffer) {
                            entry.layers = buffer.byteLength ? decodeTile(buffer) : {};
                            entry.status = 'loaded';
                            self._scheduleRedraw();
                        })
                        .catch(function () {
                            entry.status = 'error';
                        });
                });
            });
            this._pruneCache();
        },

        _pruneCache: function () {
            var max = this.options.maxCachedTiles;
            if (this._tileOrder.length <= max) return;
            var self = this;
            this._tileOrder.sort(function (a, b) { return (self._tiles[a] ? self._tiles[a].used : 0) - (self._tiles[b] ? self._tiles[b].used : 0); });
            while (this._tileOrder.length > max) {
                delete this._tiles[this._tileOrder.shift()];
            }
        },

        _scheduleRedraw: function () {
            if (this._redrawRequested) return;
            this._redrawRequested = true;
            L.Util.requestAnimFrame(function () {
                this._redrawRequested = false;
                this._redraw();
            }, this);
        },

        // the loaded tile for t, or the nearest loaded coarser tile covering it (drawn clipped to t)
        _drawableTile: function (sourceId, source, t) {
            var entry = this._tiles[this._key(sourceId, t)];
            if (entry && entry.status === 'loaded') return { tile: t, entry: entry };

            var grid = source.grid, env = this._tileEnvelope(grid, t.level, t.row, t.col);
            var cx = (env.minX + env.maxX) / 2, cy = (env.minY + env.maxY) / 2;
            for (var level = t.level - 1; level >= source.minLevel; level--) {
                var res = grid.resolutions[level], w = grid.tileSize[0] * res, h = grid.tileSize[1] * res;
                var parent = { level: level, col: Math.floor((cx - grid.origin[0]) / w), row: Math.floor((grid.origin[1] - cy) / h) };
                var pe = this._tiles[this._key(sourceId, parent)];
                if (pe && pe.status === 'loaded') return { tile: parent, entry: pe };
            }
            return null;
        },

        // ---------------------------------------------------------------- drawing

        _redraw: function () {
            if (!this._ctx || !this._style || !this._validResolution()) return;

            var ctx = this._ctx, ratio = window.devicePixelRatio || 1, size = this._map.getSize();
            ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
            ctx.clearRect(0, 0, size.x, size.y);

            var resolution = this._mapResolution();
            var self = this, labels = [];

            // per source: style zoom and the tiles to draw, with their placement on the canvas
            var perSource = {};
            Object.keys(this._sources).forEach(function (id) {
                var source = self._sources[id], grid = source.grid;
                var level = self._level(source, resolution);
                var drawables = [];
                self._visibleTiles(grid, level).forEach(function (t) {
                    var d = self._drawableTile(id, source, t);
                    if (!d) return;
                    var clipEnv = self._tileEnvelope(grid, t.level, t.row, t.col);
                    var env = self._tileEnvelope(grid, d.tile.level, d.tile.row, d.tile.col);
                    var p0 = self._toCanvas(env.minX, env.maxY), p1 = self._toCanvas(env.maxX, env.minY);
                    var c0 = self._toCanvas(clipEnv.minX, clipEnv.maxY), c1 = self._toCanvas(clipEnv.maxX, clipEnv.minY);
                    drawables.push({ entry: d.entry, x0: p0.x, y0: p0.y, sx: p1.x - p0.x, sy: p1.y - p0.y, clip: [c0.x, c0.y, c1.x - c0.x, c1.y - c0.y], tile: d.tile, req: t });
                });
                perSource[id] = { zoom: self._styleZoom(source, resolution), drawables: drawables };
            });

            var firstSource = perSource[Object.keys(perSource)[0]];
            var defaultZoom = firstSource ? firstSource.zoom : 0;

            var sourceIds = Object.keys(perSource);
            var cacheTiles = sourceIds.length === 1;   // one source: tiles are rendered to images and reused
            var usedImages = {};

            // background layers (and, with several sources, everything) directly on the canvas
            this._style.layers.forEach(function (styleLayer, layerIndex) {
                var current = perSource[styleLayer.source];
                var zoom = current ? current.zoom : defaultZoom;
                if (!self._layerVisible(styleLayer, zoom)) return;

                if (styleLayer.type === 'background') {
                    ctx.globalAlpha = self._paint(styleLayer, 'background-opacity', null, zoom, 1);
                    ctx.fillStyle = self._paint(styleLayer, 'background-color', null, zoom, 'transparent');
                    ctx.fillRect(0, 0, size.x, size.y);
                    ctx.globalAlpha = 1;
                    return;
                }
                if (!current || cacheTiles || styleLayer.type === 'symbol') return;

                current.drawables.forEach(function (d) {
                    ctx.save();
                    ctx.beginPath();
                    ctx.rect(d.clip[0], d.clip[1], d.clip[2], d.clip[3]);
                    ctx.clip();
                    self._drawLayerTile(ctx, styleLayer, layerIndex, d, zoom, null);
                    ctx.restore();
                });
            });

            // one source: geometry layers per tile from the tile image cache
            if (cacheTiles) {
                var id = sourceIds[0], current = perSource[id];
                current.drawables.forEach(function (d) {
                    var image = self._tileImage(id, d, current.zoom, usedImages);
                    if (image) {
                        ctx.drawImage(image, d.clip[0], d.clip[1], d.clip[2], d.clip[3]);
                        return;
                    }

                    // overzoomed far beyond the last level: the tile would be a huge image, draw it directly
                    ctx.save();
                    ctx.beginPath();
                    ctx.rect(d.clip[0], d.clip[1], d.clip[2], d.clip[3]);
                    ctx.clip();
                    self._style.layers.forEach(function (styleLayer, layerIndex) {
                        if (styleLayer.source !== id || styleLayer.type === 'symbol' || styleLayer.type === 'background') return;
                        if (!self._layerVisible(styleLayer, current.zoom)) return;
                        self._drawLayerTile(ctx, styleLayer, layerIndex, d, current.zoom, null);
                    });
                    ctx.restore();
                });
                // tile images not drawn now are dropped
                Object.keys(this._images).forEach(function (key) { if (!usedImages[key]) delete self._images[key]; });
            }

            // symbols (icons, labels) of all tiles, placed with collision handling on every redraw
            this._style.layers.forEach(function (styleLayer, layerIndex) {
                if (styleLayer.type !== 'symbol') return;
                var current = perSource[styleLayer.source];
                if (!current || !self._layerVisible(styleLayer, current.zoom)) return;
                current.drawables.forEach(function (d) {
                    self._drawLayerTile(ctx, styleLayer, layerIndex, d, current.zoom, labels);
                });
            });

            this._drawSymbols(ctx, labels);

            if (this.options.debug) {
                ctx.strokeStyle = 'rgba(255,0,0,.6)';
                ctx.fillStyle = 'rgba(255,0,0,.8)';
                ctx.font = '12px sans-serif';
                Object.keys(perSource).forEach(function (id) {
                    perSource[id].drawables.forEach(function (d) {
                        ctx.strokeRect(d.clip[0], d.clip[1], d.clip[2], d.clip[3]);
                        ctx.fillText(id + ' z' + d.tile.level + ' r' + d.tile.row + ' c' + d.tile.col, d.clip[0] + 4, d.clip[1] + 14);
                    });
                });
            }
        },

        _layerVisible: function (styleLayer, zoom) {
            if (styleLayer.minzoom !== undefined && zoom < styleLayer.minzoom) return false;
            if (styleLayer.maxzoom !== undefined && zoom >= styleLayer.maxzoom) return false;
            return (styleLayer.layout || {}).visibility !== 'none';
        },

        // the features of one tile for one style layer: drawn on ctx, or (symbols) collected into labels
        _drawLayerTile: function (ctx, styleLayer, layerIndex, d, zoom, labels) {
            var layer = d.entry.layers[styleLayer['source-layer']];
            if (!layer) return;

            // the filtered features of a tile per style layer: filters rarely depend on the zoom
            var zoomFilter = filterUsesZoom(styleLayer.filter);
            var features = zoomFilter ? null : d.entry.filtered[layerIndex];
            if (!features) {
                features = layer.features.filter(function (f) { return filterMatches(styleLayer.filter, f, zoom); });
                if (!zoomFilter) d.entry.filtered[layerIndex] = features;
            }
            if (!features.length) return;

            var scaleX = d.sx / layer.extent, scaleY = d.sy / layer.extent;
            var tx = function (p) { return d.x0 + p[0] * scaleX; };
            var ty = function (p) { return d.y0 + p[1] * scaleY; };
            var self = this;

            if (styleLayer.type === 'symbol') {
                if (labels) features.forEach(function (feature) { self._collectSymbol(labels, styleLayer, feature, zoom, tx, ty, d); });
            } else if (!layerIsDataDriven(styleLayer)) {
                // the same style for every feature: few paths, few fill/stroke calls
                this._drawFeatures(ctx, styleLayer, features, zoom, tx, ty);
            } else {
                features.forEach(function (feature) { self._drawFeature(ctx, styleLayer, feature, zoom, tx, ty); });
            }
        },

        // the geometry layers of a tile as image, rendered once per (tile, zoom step) and reused while panning
        _tileImage: function (sourceId, d, zoom, usedImages) {
            var zoomKey = Math.round(zoom * 2) / 2;
            var key = sourceId + '|' + d.req.level + '/' + d.req.row + '/' + d.req.col;
            usedImages[key] = true;

            var cached = this._images[key];
            if (cached && cached.zoomKey === zoomKey && cached.entry === d.entry) return cached.canvas;

            var ratio = window.devicePixelRatio || 1;
            var w = Math.max(1, Math.round(d.clip[2] * ratio)), h = Math.max(1, Math.round(d.clip[3] * ratio));
            if (w > 4096 || h > 4096) return null;   // zoomed far beyond the cache: not cached

            var canvas = cached && cached.canvas.width === w && cached.canvas.height === h ? cached.canvas : document.createElement('canvas');
            canvas.width = w;
            canvas.height = h;
            var c = canvas.getContext('2d');
            c.clearRect(0, 0, w, h);
            // draw in the coordinates of the map canvas, shifted to the tile
            c.setTransform(ratio * w / (d.clip[2] * ratio), 0, 0, ratio * h / (d.clip[3] * ratio), -d.clip[0] * ratio * w / (d.clip[2] * ratio), -d.clip[1] * ratio * h / (d.clip[3] * ratio));

            var self = this;
            this._style.layers.forEach(function (styleLayer, layerIndex) {
                if (styleLayer.source !== sourceId || styleLayer.type === 'symbol' || styleLayer.type === 'background') return;
                if (!self._layerVisible(styleLayer, zoom)) return;
                self._drawLayerTile(c, styleLayer, layerIndex, d, zoom, null);
            });

            this._images[key] = { canvas: canvas, zoomKey: zoomKey, entry: d.entry };
            return canvas;
        },

        _paint: function (styleLayer, name, feature, zoom, fallback) {
            var paint = styleLayer.paint || {};
            var value = paint[name] !== undefined ? evaluateValue(paint[name], feature, zoom) : undefined;
            return value === undefined || value === null ? fallback : value;
        },

        _layout: function (styleLayer, name, feature, zoom, fallback) {
            var layout = styleLayer.layout || {};
            var value = layout[name] !== undefined ? evaluateValue(layout[name], feature, zoom) : undefined;
            return value === undefined || value === null ? fallback : value;
        },

        _path: function (ctx, feature, tx, ty, close) {
            ctx.beginPath();
            feature.geometry.forEach(function (path) {
                addPath(ctx, path, tx, ty, close || path.closed);
            });
        },

        // all features of a tile with one style (no data driven properties) as one path
        _drawFeatures: function (ctx, styleLayer, features, zoom, tx, ty) {
            var type = styleLayer.type;

            // fills: one path with thousands of polygons is very slow to fill in browsers (nonzero over many
            // subpaths); small batches keep the number of calls low and each fill cheap
            if (type === 'fill' && features.length > FILL_BATCH) {
                for (var b = 0; b < features.length; b += FILL_BATCH) {
                    this._drawFeatures(ctx, styleLayer, features.slice(b, b + FILL_BATCH), zoom, tx, ty);
                }
                return;
            }

            var wanted = type === 'fill' ? 'Polygon' : type === 'circle' ? 'Point' : null;

            ctx.beginPath();
            var any = false;
            features.forEach(function (feature) {
                if (wanted ? feature.type !== wanted : feature.type === 'Point') return;
                any = true;
                if (type === 'circle') return;   // circles below
                feature.geometry.forEach(function (path) {
                    addPath(ctx, path, tx, ty, type === 'fill' || path.closed);
                });
            });
            if (!any) return;

            switch (type) {
                case 'fill':
                    var pattern = this._paint(styleLayer, 'fill-pattern', null, zoom, null);
                    ctx.globalAlpha = this._paint(styleLayer, 'fill-opacity', null, zoom, 1);
                    ctx.fillStyle = pattern ? (this._pattern(ctx, pattern) || 'transparent') : this._paint(styleLayer, 'fill-color', null, zoom, '#000');
                    // MVT rings have a defined winding (holes opposite): nonzero keeps overlapping features filled
                    ctx.fill('nonzero');
                    var outline = this._paint(styleLayer, 'fill-outline-color', null, zoom, null);
                    if (outline && !pattern) {
                        ctx.strokeStyle = outline;
                        ctx.lineWidth = 1;
                        ctx.stroke();
                    }
                    ctx.globalAlpha = 1;
                    break;

                case 'line':
                    var width = this._paint(styleLayer, 'line-width', null, zoom, 1);
                    if (width <= 0) return;
                    ctx.globalAlpha = this._paint(styleLayer, 'line-opacity', null, zoom, 1);
                    ctx.strokeStyle = this._paint(styleLayer, 'line-color', null, zoom, '#000');
                    ctx.lineWidth = width;
                    ctx.lineJoin = this._layout(styleLayer, 'line-join', null, zoom, 'miter');
                    ctx.lineCap = this._layout(styleLayer, 'line-cap', null, zoom, 'butt');
                    var dash = this._paint(styleLayer, 'line-dasharray', null, zoom, null);
                    ctx.setLineDash(Array.isArray(dash) ? dash.map(function (v) { return v * width; }) : []);
                    ctx.stroke();
                    ctx.setLineDash([]);
                    ctx.globalAlpha = 1;
                    break;

                case 'circle':
                    var radius = this._paint(styleLayer, 'circle-radius', null, zoom, 5);
                    ctx.beginPath();
                    features.forEach(function (feature) {
                        if (feature.type !== 'Point') return;
                        feature.geometry.forEach(function (path) {
                            path.forEach(function (p) {
                                ctx.moveTo(tx(p) + radius, ty(p));
                                ctx.arc(tx(p), ty(p), radius, 0, 2 * Math.PI);
                            });
                        });
                    });
                    ctx.globalAlpha = this._paint(styleLayer, 'circle-opacity', null, zoom, 1);
                    ctx.fillStyle = this._paint(styleLayer, 'circle-color', null, zoom, '#000');
                    ctx.fill();
                    var strokeWidth = this._paint(styleLayer, 'circle-stroke-width', null, zoom, 0);
                    if (strokeWidth > 0) {
                        ctx.globalAlpha = this._paint(styleLayer, 'circle-stroke-opacity', null, zoom, 1);
                        ctx.strokeStyle = this._paint(styleLayer, 'circle-stroke-color', null, zoom, '#000');
                        ctx.lineWidth = strokeWidth;
                        ctx.stroke();
                    }
                    ctx.globalAlpha = 1;
                    break;
            }
        },

        _drawFeature: function (ctx, styleLayer, feature, zoom, tx, ty) {
            var self = this;
            switch (styleLayer.type) {
                case 'fill':
                    if (feature.type !== 'Polygon') return;
                    this._path(ctx, feature, tx, ty, true);
                    var pattern = this._paint(styleLayer, 'fill-pattern', feature, zoom, null);
                    ctx.globalAlpha = this._paint(styleLayer, 'fill-opacity', feature, zoom, 1);
                    ctx.fillStyle = pattern ? (this._pattern(ctx, resolveTokens(pattern, feature)) || 'transparent') : this._paint(styleLayer, 'fill-color', feature, zoom, '#000');
                    ctx.fill('evenodd');
                    var outline = this._paint(styleLayer, 'fill-outline-color', feature, zoom, null);
                    if (outline && !pattern) {
                        ctx.strokeStyle = outline;
                        ctx.lineWidth = 1;
                        ctx.stroke();
                    }
                    ctx.globalAlpha = 1;
                    break;

                case 'line':
                    if (feature.type === 'Point') return;
                    this._path(ctx, feature, tx, ty, feature.type === 'Polygon');
                    var width = this._paint(styleLayer, 'line-width', feature, zoom, 1);
                    if (width <= 0) return;
                    ctx.globalAlpha = this._paint(styleLayer, 'line-opacity', feature, zoom, 1);
                    ctx.strokeStyle = this._paint(styleLayer, 'line-color', feature, zoom, '#000');
                    ctx.lineWidth = width;
                    ctx.lineJoin = this._layout(styleLayer, 'line-join', feature, zoom, 'miter');
                    ctx.lineCap = this._layout(styleLayer, 'line-cap', feature, zoom, 'butt');
                    var dash = this._paint(styleLayer, 'line-dasharray', feature, zoom, null);
                    ctx.setLineDash(Array.isArray(dash) ? dash.map(function (v) { return v * width; }) : []);
                    ctx.stroke();
                    ctx.setLineDash([]);
                    ctx.globalAlpha = 1;
                    break;

                case 'circle':
                    if (feature.type !== 'Point') return;
                    var radius = this._paint(styleLayer, 'circle-radius', feature, zoom, 5);
                    var strokeWidth = this._paint(styleLayer, 'circle-stroke-width', feature, zoom, 0);
                    feature.geometry.forEach(function (path) {
                        path.forEach(function (p) {
                            ctx.beginPath();
                            ctx.arc(tx(p), ty(p), radius, 0, 2 * Math.PI);
                            ctx.globalAlpha = self._paint(styleLayer, 'circle-opacity', feature, zoom, 1);
                            ctx.fillStyle = self._paint(styleLayer, 'circle-color', feature, zoom, '#000');
                            ctx.fill();
                            if (strokeWidth > 0) {
                                ctx.globalAlpha = self._paint(styleLayer, 'circle-stroke-opacity', feature, zoom, 1);
                                ctx.strokeStyle = self._paint(styleLayer, 'circle-stroke-color', feature, zoom, '#000');
                                ctx.lineWidth = strokeWidth;
                                ctx.stroke();
                            }
                        });
                    });
                    ctx.globalAlpha = 1;
                    break;
            }
        },

        // ---------------------------------------------------------------- sprites

        _spriteEntry: function (name) {
            return this._sprite && name && this._sprite.index[name] ? this._sprite.index[name] : null;
        },

        _pattern: function (ctx, name) {
            if (this._patterns[name] !== undefined) return this._patterns[name];
            var entry = this._spriteEntry(name);
            if (!entry) return (this._patterns[name] = null);

            var tile = document.createElement('canvas');
            tile.width = entry.width;
            tile.height = entry.height;
            tile.getContext('2d').drawImage(this._sprite.image, entry.x, entry.y, entry.width, entry.height, 0, 0, entry.width, entry.height);

            var pattern = ctx.createPattern(tile, 'repeat');
            var ratio = entry.pixelRatio || this._sprite.pixelRatio;
            if (pattern && pattern.setTransform && ratio > 1) {
                pattern.setTransform(new DOMMatrix().scale(1 / ratio));
            }
            return (this._patterns[name] = pattern);
        },

        // icon image (an SDF icon tinted with icon-color): { source, x, y, width, height }
        _iconImage: function (entry, name, color) {
            if (!entry.sdf || !color) {
                return { source: this._sprite.image, x: entry.x, y: entry.y, width: entry.width, height: entry.height };
            }

            var key = name + '|' + color;
            if (!this._tinted[key]) {
                var canvas = document.createElement('canvas');
                canvas.width = entry.width;
                canvas.height = entry.height;
                var c = canvas.getContext('2d');
                c.drawImage(this._sprite.image, entry.x, entry.y, entry.width, entry.height, 0, 0, entry.width, entry.height);
                // SDF: alpha above ~0.75 is inside the shape
                var data = c.getImageData(0, 0, entry.width, entry.height);
                for (var i = 3; i < data.data.length; i += 4) {
                    var a = data.data[i] / 255;
                    data.data[i] = Math.max(0, Math.min(255, Math.round((a - 0.7) / 0.1 * 255)));
                }
                c.putImageData(data, 0, 0);
                c.globalCompositeOperation = 'source-in';
                c.fillStyle = color;
                c.fillRect(0, 0, entry.width, entry.height);
                this._tinted[key] = canvas;
            }
            return { source: this._tinted[key], x: 0, y: 0, width: entry.width, height: entry.height };
        },

        // ---------------------------------------------------------------- symbols (icons, labels)

        _collectSymbol: function (labels, styleLayer, feature, zoom, tx, ty, drawable) {
            var placement = this._layout(styleLayer, 'symbol-placement', feature, zoom, 'point');
            var iconName = resolveTokens(this._layout(styleLayer, 'icon-image', feature, zoom, null), feature);
            var text = resolveTokens(this._layout(styleLayer, 'text-field', feature, zoom, null), feature);
            if (text !== null && text !== undefined) {
                text = String(text);
                var transform = this._layout(styleLayer, 'text-transform', feature, zoom, 'none');
                if (transform === 'uppercase') text = text.toUpperCase();
                else if (transform === 'lowercase') text = text.toLowerCase();
            }
            if (!iconName && !text) return;

            var anchorPoints = [];
            var keepViewport = placement === 'line-center' || this._layout(styleLayer, 'text-rotation-alignment', feature, zoom, 'map') === 'viewport';

            if (placement === 'point' || feature.type === 'Point') {
                feature.geometry.forEach(function (path) {
                    if (feature.type === 'Point') {
                        path.forEach(function (p) { anchorPoints.push({ x: tx(p), y: ty(p), angle: 0 }); });
                    } else if (path.length) {
                        // polygon/line with point placement: the middle of the path's box
                        var minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
                        path.forEach(function (p) { minX = Math.min(minX, p[0]); maxX = Math.max(maxX, p[0]); minY = Math.min(minY, p[1]); maxY = Math.max(maxY, p[1]); });
                        var c = [(minX + maxX) / 2, (minY + maxY) / 2];
                        anchorPoints.push({ x: tx(c), y: ty(c), angle: 0 });
                    }
                });
            } else {
                // line placement: every symbol-spacing pixels ('line'), or the middle of the longest part ('line-center')
                var spacing = placement === 'line' ? this._layout(styleLayer, 'symbol-spacing', feature, zoom, 250) : 0;
                var paths = feature.geometry.map(function (path) {
                    var pts = path.map(function (p) { return [tx(p), ty(p)]; }), length = 0;
                    for (var i = 1; i < pts.length; i++) length += Math.hypot(pts[i][0] - pts[i - 1][0], pts[i][1] - pts[i - 1][1]);
                    return { pts: pts, length: length };
                });

                var along = function (path, distance) {
                    var run = 0, pts = path.pts;
                    for (var j = 1; j < pts.length; j++) {
                        var segment = Math.hypot(pts[j][0] - pts[j - 1][0], pts[j][1] - pts[j - 1][1]);
                        if (run + segment >= distance && segment > 0) {
                            var t = (distance - run) / segment;
                            var angle = keepViewport ? 0 : Math.atan2(pts[j][1] - pts[j - 1][1], pts[j][0] - pts[j - 1][0]);
                            if (angle > Math.PI / 2) angle -= Math.PI;
                            if (angle < -Math.PI / 2) angle += Math.PI;
                            return { x: pts[j - 1][0] + t * (pts[j][0] - pts[j - 1][0]), y: pts[j - 1][1] + t * (pts[j][1] - pts[j - 1][1]), angle: angle, lineLength: path.length, path: pts, distance: distance };
                        }
                        run += segment;
                    }
                    return null;
                };

                if (spacing > 0) {
                    paths.forEach(function (path) {
                        if (path.length < 1) return;
                        var first = Math.min(spacing / 2, path.length / 2);
                        for (var d = first; d < path.length; d += spacing) {
                            var a = along(path, d);
                            if (a) anchorPoints.push(a);
                        }
                    });
                } else {
                    var longest = paths.reduce(function (best, p) { return !best || p.length > best.length ? p : best; }, null);
                    if (longest && longest.length > 0) {
                        var mid = along(longest, longest.length / 2);
                        if (mid) anchorPoints.push(mid);
                    }
                }
            }

            var clip = drawable.clip, self = this;
            var size = this._layout(styleLayer, 'text-size', feature, zoom, 16);
            var iconEntry = iconName ? this._spriteEntry(iconName) : null;
            var iconColor = this._paint(styleLayer, 'icon-color', feature, zoom, null);

            anchorPoints.forEach(function (a) {
                // only symbols anchored inside this tile: the neighbor tile draws its own
                if (a.x < clip[0] || a.y < clip[1] || a.x >= clip[0] + clip[2] || a.y >= clip[1] + clip[3]) return;

                labels.push({
                    x: a.x, y: a.y, angle: a.angle,
                    icon: iconEntry ? self._iconImage(iconEntry, iconName, iconColor) : null,
                    iconRatio: iconEntry ? (iconEntry.pixelRatio || self._sprite.pixelRatio) : 1,
                    iconSize: self._layout(styleLayer, 'icon-size', feature, zoom, 1),
                    iconRotate: self._layout(styleLayer, 'icon-rotate', feature, zoom, 0) * Math.PI / 180,
                    iconOverlap: self._layout(styleLayer, 'icon-allow-overlap', feature, zoom, false),
                    iconOptional: self._layout(styleLayer, 'icon-optional', feature, zoom, false),
                    iconIgnorePlacement: self._layout(styleLayer, 'icon-ignore-placement', feature, zoom, false),
                    textOptional: self._layout(styleLayer, 'text-optional', feature, zoom, false),
                    textIgnorePlacement: self._layout(styleLayer, 'text-ignore-placement', feature, zoom, false),
                    text: text || null,
                    font: cssFont(self._layout(styleLayer, 'text-font', feature, zoom, ['Arial Regular']), size),
                    size: size,
                    letterSpacing: self._layout(styleLayer, 'text-letter-spacing', feature, zoom, 0),
                    maxWidth: self._layout(styleLayer, 'text-max-width', feature, zoom, 10),
                    anchor: self._layout(styleLayer, 'text-anchor', feature, zoom, 'center'),
                    offset: self._layout(styleLayer, 'text-offset', feature, zoom, [0, 0]),
                    textRotate: self._layout(styleLayer, 'text-rotate', feature, zoom, 0) * Math.PI / 180,
                    textOverlap: self._layout(styleLayer, 'text-allow-overlap', feature, zoom, false),
                    padding: self._layout(styleLayer, 'text-padding', feature, zoom, 2),
                    color: self._paint(styleLayer, 'text-color', feature, zoom, '#000'),
                    opacity: self._paint(styleLayer, 'text-opacity', feature, zoom, 1),
                    haloColor: self._paint(styleLayer, 'text-halo-color', feature, zoom, null),
                    haloWidth: self._paint(styleLayer, 'text-halo-width', feature, zoom, 0),
                    alongLine: a.lineLength !== undefined && placement !== 'point',
                    lineLength: a.lineLength,
                    // 'line' placement, aligned to the map: the text follows the line glyph by glyph
                    curvedPath: placement === 'line' && !keepViewport && a.path ? a.path : null,
                    curvedDistance: a.distance,
                    maxAngle: self._layout(styleLayer, 'text-max-angle', feature, zoom, 45) * Math.PI / 180
                });
            });
        },

        _drawSymbols: function (ctx, labels) {
            var placed = [], declutter = this.options.declutter;

            function collides(box) {
                if (!box) return false;
                for (var i = 0; i < placed.length; i++) {
                    var p = placed[i];
                    if (box[0] < p[2] && box[2] > p[0] && box[1] < p[3] && box[3] > p[1]) return true;
                }
                return false;
            }

            function setLetterSpacing(px) {
                if ('letterSpacing' in ctx) ctx.letterSpacing = px ? px + 'px' : '0px';
            }

            function rotatedBox(l, dx, dy, width, height, angle, pad) {
                var cos = Math.cos(angle), sin = Math.sin(angle);
                var corners = [[dx, dy], [dx + width, dy], [dx, dy + height], [dx + width, dy + height]].map(function (c) {
                    return [l.x + c[0] * cos - c[1] * sin, l.y + c[0] * sin + c[1] * cos];
                });
                return [
                    Math.min.apply(null, corners.map(function (c) { return c[0]; })) - pad,
                    Math.min.apply(null, corners.map(function (c) { return c[1]; })) - pad,
                    Math.max.apply(null, corners.map(function (c) { return c[0]; })) + pad,
                    Math.max.apply(null, corners.map(function (c) { return c[1]; })) + pad
                ];
            }

            labels.forEach(function (l) {
                // ---- icon layout
                var icon = null;
                if (l.icon) {
                    var w = l.icon.width / l.iconRatio * l.iconSize, h = l.icon.height / l.iconRatio * l.iconSize;
                    icon = { w: w, h: h, box: [l.x - w / 2, l.y - h / 2, l.x + w / 2, l.y + h / 2] };
                }

                // ---- text layout
                var text = null;
                if (l.text) {
                    ctx.font = l.font;
                    setLetterSpacing(l.letterSpacing * l.size);

                    // point labels wrap at text-max-width (ems), as GL does
                    var lines = [l.text];
                    if (!l.alongLine && l.maxWidth > 0) {
                        var maxPx = l.maxWidth * l.size, words = l.text.split(' '), current = '';
                        lines = [];
                        words.forEach(function (word) {
                            var candidate = current ? current + ' ' + word : word;
                            if (current && ctx.measureText(candidate).width > maxPx) { lines.push(current); current = word; }
                            else current = candidate;
                        });
                        if (current) lines.push(current);
                    }

                    var lineHeight = l.size * 1.2;
                    var width = Math.max.apply(null, lines.map(function (line) { return ctx.measureText(line).width; }));
                    var height = lineHeight * lines.length;
                    setLetterSpacing(0);

                    if (l.curvedPath) {
                        // along the line, glyph by glyph
                        setLetterSpacing(0);
                        var glyphs = placeAlongPath(ctx, l.text, l.curvedPath, l.curvedDistance, l.maxAngle, l.letterSpacing * l.size);
                        if (glyphs) {
                            var half = l.size / 2 + l.padding;
                            text = {
                                glyphs: glyphs,
                                box: [
                                    Math.min.apply(null, glyphs.map(function (g) { return g.x; })) - half,
                                    Math.min.apply(null, glyphs.map(function (g) { return g.y; })) - half,
                                    Math.max.apply(null, glyphs.map(function (g) { return g.x; })) + half,
                                    Math.max.apply(null, glyphs.map(function (g) { return g.y; })) + half
                                ]
                            };
                        }
                    } else if (!(l.alongLine && width > l.lineLength)) {
                        // longer than its line: no text there, as GL
                        var anchor = ANCHORS[l.anchor] || ANCHORS.center;
                        var dx = anchor[0] * width + (l.offset[0] || 0) * l.size;
                        var dy = anchor[1] * height + (l.offset[1] || 0) * l.size;
                        var angle = l.angle + l.textRotate;
                        text = {
                            lines: lines, lineHeight: lineHeight, width: width, dx: dx, dy: dy, angle: angle,
                            box: rotatedBox(l, dx, dy, width, height, angle, l.padding)
                        };
                    }
                }
                if (!icon && !text) return;

                // ---- placement: icon and text of a symbol are placed together, as GL does
                var iconFits = !icon || !declutter || l.iconOverlap || !collides(icon.box);
                var textFits = !text || !declutter || l.textOverlap || !collides(text.box);

                var showIcon = !!icon, showText = !!text;
                if (!iconFits || !textFits) {
                    if (icon && text && iconFits && l.textOptional) showText = false;        // icon alone
                    else if (icon && text && textFits && l.iconOptional) showIcon = false;   // text alone
                    else return;
                }

                if (showIcon) {
                    ctx.save();
                    ctx.translate(l.x, l.y);
                    ctx.rotate(l.angle + l.iconRotate);
                    ctx.drawImage(l.icon.source, l.icon.x, l.icon.y, l.icon.width, l.icon.height, -icon.w / 2, -icon.h / 2, icon.w, icon.h);
                    ctx.restore();
                    if (!l.iconIgnorePlacement) placed.push(icon.box);
                }

                if (showText && text.glyphs) {
                    ctx.save();
                    ctx.font = l.font;
                    ctx.textBaseline = 'middle';
                    ctx.textAlign = 'center';
                    ctx.globalAlpha = l.opacity;
                    // first all halos, then all glyphs: halos must not cover neighbor glyphs
                    [true, false].forEach(function (halo) {
                        if (halo && !(l.haloColor && l.haloWidth > 0)) return;
                        text.glyphs.forEach(function (g) {
                            ctx.save();
                            ctx.translate(g.x, g.y);
                            ctx.rotate(g.angle);
                            if (halo) {
                                ctx.lineJoin = 'round';
                                ctx.lineWidth = l.haloWidth * 2;
                                ctx.strokeStyle = l.haloColor;
                                ctx.strokeText(g.ch, 0, 0);
                            } else {
                                ctx.fillStyle = l.color;
                                ctx.fillText(g.ch, 0, 0);
                            }
                            ctx.restore();
                        });
                    });
                    ctx.restore();
                    if (!l.textIgnorePlacement) placed.push(text.box);
                } else if (showText) {
                    ctx.save();
                    ctx.font = l.font;
                    setLetterSpacing(l.letterSpacing * l.size);
                    ctx.translate(l.x, l.y);
                    ctx.rotate(text.angle);
                    ctx.textBaseline = 'top';
                    ctx.textAlign = 'center';
                    ctx.globalAlpha = l.opacity;
                    text.lines.forEach(function (line, i) {
                        var lx = text.dx + text.width / 2, ly = text.dy + i * text.lineHeight + (text.lineHeight - l.size) / 2;
                        if (l.haloColor && l.haloWidth > 0) {
                            ctx.lineJoin = 'round';
                            ctx.lineWidth = l.haloWidth * 2;
                            ctx.strokeStyle = l.haloColor;
                            ctx.strokeText(line, lx, ly);
                        }
                        ctx.fillStyle = l.color;
                        ctx.fillText(line, lx, ly);
                    });
                    ctx.restore();
                    setLetterSpacing(0);
                    if (!l.textIgnorePlacement) placed.push(text.box);
                }
            });
        }
    });

    /**
     * A Proj4Leaflet CRS for the grid of a gView vector tile cache: the map's zoom levels are then exactly the
     * cache levels, plus `overzoomLevels` (default 3) levels with halved resolutions beyond the last cache level
     * (drawn with the last level's tiles). Set the map's maxZoom to `crs.options.resolutions.length - 1`.
     * Needs Proj4Leaflet (L.Proj.CRS) and the proj4 definition of the EPSG code.
     *   L.gViewVectorTiles.createCrs(style.metadata['gview:grid'], '+proj=tmerc ...', { overzoomLevels: 3 })
     */
    GViewVectorTiles.createCrs = function (grid, proj4Definition, options) {
        if (!L.Proj || !L.Proj.CRS) throw new Error('Proj4Leaflet is required for L.gViewVectorTiles.createCrs');

        var overzoom = options && options.overzoomLevels !== undefined ? options.overzoomLevels : 3;
        var resolutions = grid.resolutions.slice();
        for (var i = 0; i < overzoom; i++) {
            resolutions.push(resolutions[resolutions.length - 1] / 2);
        }

        return new L.Proj.CRS('EPSG:' + grid.epsg, proj4Definition, {
            origin: grid.origin,
            resolutions: resolutions,
            bounds: grid.extent ? L.bounds([grid.extent[0], grid.extent[1]], [grid.extent[2], grid.extent[3]]) : undefined
        });
    };

    GViewVectorTiles.decodeTile = decodeTile;
    GViewVectorTiles.evaluate = evaluateValue;

    L.GViewVectorTiles = GViewVectorTiles;
    L.gViewVectorTiles = function (url, options) {
        return new GViewVectorTiles(url, options);
    };
    L.gViewVectorTiles.createCrs = GViewVectorTiles.createCrs;
    L.gViewVectorTiles.decodeTile = decodeTile;

    return GViewVectorTiles;
}));
