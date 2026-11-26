/*
 * gView Server — test map of a service (admins only)
 *
 * Drives one interface of the service at a time (WMS, GeoServices REST, GeoJson) like a client:
 * it reads the capabilities of the interface first, then requests map images and identify results.
 * Every request goes through send(), which logs it (status, time, size, content type) in the request log.
 * The login cookie authenticates the requests; "Test as anonymous" sends them without it.
 * Never puts a token into a URL; "Copy curl" uses a placeholder.
 */
(function () {
    'use strict';

    var cfgElement = document.getElementById('tm-config');
    if (!cfgElement || !window.ol || !window.proj4) {
        return;
    }

    var cfg = JSON.parse(cfgElement.textContent);
    var storeKey = 'gview-testmap-extent:' + cfg.id;
    var MAX_LOG = 200;
    var PREVIEW_CHARS = 20000;

    // ==================================================================== helpers

    function $(id) { return document.getElementById(id); }

    function h(tag, attrs, children) {
        var e = document.createElement(tag);
        if (attrs) {
            Object.keys(attrs).forEach(function (k) {
                var v = attrs[k];
                if (v === null || v === undefined || v === false) { return; }
                if (k === 'text') { e.textContent = v; }
                else if (k === 'className') { e.className = v; }
                else if (k.indexOf('on') === 0 && typeof v === 'function') { e.addEventListener(k.substring(2), v); }
                else { e.setAttribute(k, v === true ? '' : v); }
            });
        }
        (children || []).forEach(function (c) {
            if (c === null || c === undefined) { return; }
            e.appendChild(typeof c === 'string' ? document.createTextNode(c) : c);
        });
        return e;
    }

    function query(params) {
        return Object.keys(params)
            .filter(function (k) { return params[k] !== null && params[k] !== undefined; })
            .map(function (k) { return encodeURIComponent(k) + '=' + encodeURIComponent(params[k]); })
            .join('&');
    }

    function withQuery(url, params) {
        return url + (url.indexOf('?') >= 0 ? '&' : '?') + query(params);
    }

    function fmtSize(bytes) {
        if (bytes === null || bytes === undefined) { return ''; }
        if (bytes < 1024) { return bytes + ' B'; }
        if (bytes < 1024 * 1024) { return (bytes / 1024).toFixed(1) + ' KB'; }
        return (bytes / 1024 / 1024).toFixed(2) + ' MB';
    }

    function fmtMs(ms) {
        if (ms === null || ms === undefined) { return ''; }
        return ms < 1000 ? Math.round(ms) + ' ms' : (ms / 1000).toFixed(2) + ' s';
    }

    function fmtTime(d) {
        function p(n) { return (n < 10 ? '0' : '') + n; }
        return p(d.getHours()) + ':' + p(d.getMinutes()) + ':' + p(d.getSeconds());
    }

    function round(v, crs) {
        var digits = isGeographic(crs) ? 7 : 2;
        return Number(v.toFixed(digits));
    }

    function bboxString(extent, crs) {
        return extent.map(function (v) { return round(v, crs); }).join(',');
    }

    function isGeographic(crs) {
        var p = ol.proj.get(crs);
        return p && p.getUnits() === 'degrees';
    }

    function epsgNumber(crs) {
        return parseInt(String(crs).replace(/^EPSG:/i, ''), 10);
    }

    function validExtent(e) {
        return e && e.length === 4 && e.every(isFinite) && e[2] > e[0] && e[3] > e[1];
    }

    function copy(text, button) {
        if (!navigator.clipboard) { return; }
        navigator.clipboard.writeText(text).then(function () {
            var label = button.textContent;
            button.textContent = 'Copied';
            setTimeout(function () { button.textContent = label; }, 1200);
        });
    }

    // ==================================================================== request log

    var log = [];
    var nextId = 1;
    var selectedId = null;

    function isAnonymous() { return $('tm-anonymous').checked; }

    /*
     * Sends a request and logs it.
     * opts: { iface, op, url, method, body, contentType, signal, as: 'blob' | 'text' }
     * Resolves to { entry, ok, status, blob, text }, rejects on network errors and aborts.
     */
    function send(opts) {
        var entry = {
            id: nextId++,
            time: new Date(),
            iface: opts.iface,
            op: opts.op,
            method: opts.method || 'GET',
            url: opts.url,
            body: opts.body || null,
            anonymous: isAnonymous(),
            state: 'pending',
            status: null,
            ms: null,
            size: null,
            contentType: '',
            blob: null,
            text: null,
            objectUrl: null
        };
        addEntry(entry);

        var started = performance.now();
        var init = {
            method: entry.method,
            credentials: entry.anonymous ? 'omit' : 'same-origin',
            signal: opts.signal
        };
        if (entry.body) {
            init.body = entry.body;
            init.headers = { 'Content-Type': opts.contentType || 'application/json' };
        }

        return fetch(entry.url, init)
            .then(function (response) {
                entry.status = response.status;
                entry.contentType = (response.headers.get('content-type') || '').split(';')[0];
                return response.blob().then(function (blob) {
                    entry.ms = performance.now() - started;
                    entry.size = blob.size;
                    entry.blob = blob;
                    entry.state = response.ok ? 'ok' : 'error';
                    var isImage = entry.contentType.indexOf('image/') === 0;
                    var textPromise = isImage ? Promise.resolve(null) : blob.text();
                    return textPromise.then(function (text) {
                        entry.text = text;
                        // GeoServices REST answers errors with HTTP 200 and { "error": ... }
                        if (text && entry.state === 'ok' && /^\s*\{\s*"error"\s*:/.test(text)) {
                            entry.state = 'error';
                        }
                        updateEntry(entry);
                        return { entry: entry, ok: entry.state === 'ok', status: entry.status, blob: blob, text: text, isImage: isImage };
                    });
                });
            })
            .catch(function (err) {
                entry.ms = performance.now() - started;
                entry.state = err && err.name === 'AbortError' ? 'aborted' : 'failed';
                entry.text = entry.state === 'failed' ? String(err && err.message || err) : null;
                updateEntry(entry);
                throw err;
            });
    }

    function addEntry(entry) {
        log.unshift(entry);
        while (log.length > MAX_LOG) {
            var removed = log.pop();
            if (removed.objectUrl) { URL.revokeObjectURL(removed.objectUrl); }
        }
        renderLog();
    }

    function updateEntry(entry) {
        renderLog();
        if (selectedId === entry.id) { renderDetail(entry); }
    }

    function statusText(entry) {
        if (entry.tiles) {
            var counts = {};
            entry.tiles.forEach(function (t) {
                var key = t.state === 'pending' ? '…' : t.state === 'failed' ? 'failed' : String(t.status);
                counts[key] = (counts[key] || 0) + 1;
            });
            return entry.tiles.length + ': ' + Object.keys(counts).map(function (k) { return counts[k] + '× ' + k; }).join(', ');
        }
        switch (entry.state) {
            case 'pending': return '…';
            case 'aborted': return 'aborted';
            case 'failed': return 'failed';
            default: return String(entry.status) + (entry.state === 'error' && entry.status === 200 ? ' (error)' : '') + (entry.warning ? ' ⚠' : '');
        }
    }

    function renderLog() {
        var filter = $('tm-log-filter').value;
        var rows = $('tm-log-rows');
        rows.textContent = '';

        var shown = log.filter(function (e) { return !filter || e.op === filter; });
        shown.forEach(function (e) {
            var tr = h('tr', {
                className: 'tm-log-row tm-st-' + (e.warning && e.state === 'ok' ? 'warn' : e.state) + (e.id === selectedId ? ' selected' : ''),
                onclick: function () { selectEntry(e.id); }
            }, [
                h('td', { text: fmtTime(e.time) }),
                h('td', { text: e.iface }),
                h('td', { text: e.op }),
                h('td', { text: e.method }),
                h('td', { className: 'tm-status', text: statusText(e) + (e.anonymous ? ' · anon' : '') }),
                h('td', { className: 'num', text: fmtMs(e.ms) }),
                h('td', { className: 'num', text: fmtSize(e.size) }),
                h('td', { text: e.contentType })
            ]);
            rows.appendChild(tr);
        });

        $('tm-log-empty').hidden = shown.length > 0;
        $('tm-log-count').textContent = log.length ? '(' + log.length + (log.length >= MAX_LOG ? ', newest ' + MAX_LOG : '') + ')' : '';
    }

    function selectEntry(id) {
        selectedId = selectedId === id ? null : id;
        renderLog();
        var entry = log.filter(function (e) { return e.id === selectedId; })[0];
        if (entry) {
            renderDetail(entry);
        } else {
            $('tm-log-detail').hidden = true;
        }
    }

    function curl(entry) {
        var parts = ['curl'];
        if (entry.method !== 'GET') { parts.push('-X ' + entry.method); }
        parts.push("'" + entry.url.replace(/'/g, "'\\''") + "'");
        if (!cfg.isPublic && !entry.anonymous) {
            parts.push('-H "Authorization: Bearer <TOKEN>"');
        }
        if (entry.body) {
            parts.push("-H 'Content-Type: application/json'");
            parts.push("--data '" + entry.body.replace(/'/g, "'\\''") + "'");
        }
        return parts.join(' ');
    }

    function parameterRows(entry) {
        var rows = [];
        try {
            var u = new URL(entry.url, location.href);
            u.searchParams.forEach(function (value, key) { rows.push([key, value]); });
        } catch (e) { }
        return rows;
    }

    function tileTable(entry) {
        return h('table', { className: 'tm-params tm-tiles' }, [h('tbody', null, entry.tiles.map(function (t) {
            return h('tr', { className: 'tm-st-' + t.state }, [
                h('th', { className: 'tm-status', text: t.state === 'pending' ? '…' : t.state === 'failed' ? 'failed' : String(t.status) }),
                h('th', { text: fmtMs(t.ms) }),
                h('th', { text: fmtSize(t.size) }),
                h('td', null, [h('a', { href: t.url, target: '_blank', rel: 'noopener', text: t.url })])
            ]);
        }))]);
    }

    function preview(entry) {
        if (entry.tiles) { return tileTable(entry); }
        if (entry.state === 'pending') { return h('p', { className: 'muted', text: 'Waiting for the response…' }); }
        if (entry.state === 'aborted') { return h('p', { className: 'muted', text: 'Aborted by a newer request after ' + fmtMs(entry.ms) + '.' }); }
        if (entry.blob && entry.contentType.indexOf('image/') === 0) {
            if (!entry.objectUrl) { entry.objectUrl = URL.createObjectURL(entry.blob); }
            return h('div', { className: 'tm-preview-image' }, [h('img', { src: entry.objectUrl, alt: 'response image' })]);
        }
        var text = entry.text || '';
        if (/json/.test(entry.contentType) || /^\s*[\{\[]/.test(text)) {
            try { text = JSON.stringify(JSON.parse(text), null, 2); } catch (e) { }
        }
        var truncated = text.length > PREVIEW_CHARS;
        return h('div', null, [
            h('pre', { className: 'tm-preview-text', text: truncated ? text.substring(0, PREVIEW_CHARS) : text }),
            truncated ? h('p', { className: 'muted', text: 'Truncated: ' + fmtSize(entry.size) + ' in total. Open the request in a new tab for the whole response.' }) : null
        ]);
    }

    function renderDetail(entry) {
        var detail = $('tm-log-detail');
        detail.textContent = '';
        detail.hidden = false;

        var copyUrl = h('button', { type: 'button', className: 'btn', text: 'Copy URL' });
        copyUrl.addEventListener('click', function () { copy(entry.url, copyUrl); });
        var copyCurl = h('button', { type: 'button', className: 'btn', text: 'Copy curl' });
        copyCurl.addEventListener('click', function () { copy(curl(entry), copyCurl); });

        var actions = h('div', { className: 'tm-detail-actions' }, [
            copyUrl,
            copyCurl,
            entry.method === 'GET' ? h('a', { className: 'btn', href: entry.url, target: '_blank', rel: 'noopener', text: 'Open in new tab' }) : null,
            h('span', { className: 'grow' }),
            h('button', { type: 'button', className: 'btn', text: 'Close', onclick: function () { selectEntry(entry.id); } })
        ]);

        var params = parameterRows(entry);
        var paramTable = params.length
            ? h('table', { className: 'tm-params' }, [h('tbody', null, params.map(function (p) {
                return h('tr', null, [h('th', { text: p[0] }), h('td', { text: p[1] })]);
            }))])
            : null;

        detail.appendChild(h('div', { className: 'tm-detail-head' }, [
            h('b', { text: entry.iface + ' · ' + entry.op }),
            h('span', { className: 'ogc-badge', text: entry.method }),
            h('span', { className: 'muted', text: statusText(entry) + ' · ' + fmtMs(entry.ms) + ' · ' + fmtSize(entry.size) })
        ]));
        detail.appendChild(h('div', { className: 'tm-detail-url', text: entry.tiles ? (entry.url + '  (first tile of ' + entry.tiles.length + ')') : entry.url }));
        if (entry.warning) {
            detail.appendChild(h('p', { className: 'tm-warning', text: '⚠ ' + entry.warning }));
        }
        if (!cfg.isPublic) {
            detail.appendChild(h('p', {
                className: 'tm-hint',
                text: entry.anonymous
                    ? 'Sent without login (anonymous).'
                    : 'Secured service: this page sends the login cookie, other clients need a token (curl: replace <TOKEN>).'
            }));
        }
        detail.appendChild(actions);
        if (entry.body) {
            detail.appendChild(h('div', { className: 'tm-detail-label', text: 'Body' }));
            detail.appendChild(h('pre', { className: 'tm-preview-text', text: entry.body }));
        }
        if (paramTable) {
            detail.appendChild(h('div', { className: 'tm-detail-label', text: 'Parameters' }));
            detail.appendChild(paramTable);
        }
        detail.appendChild(h('div', { className: 'tm-detail-label', text: 'Response' }));
        detail.appendChild(preview(entry));
    }

    $('tm-log-filter').addEventListener('change', renderLog);
    $('tm-log-clear').addEventListener('click', function () {
        log.forEach(function (e) { if (e.objectUrl) { URL.revokeObjectURL(e.objectUrl); } });
        log = [];
        selectedId = null;
        $('tm-log-detail').hidden = true;
        renderLog();
    });

    /*
     * Tiles: one log entry per view (state.tileGroup, reset on movestart) with all its tiles.
     * Returns a callback the loader calls when the tile is done.
     */
    function logTile(ifaceTitle, url) {
        var group = state.tileGroup;
        if (!group) {
            group = {
                id: nextId++, time: new Date(), iface: ifaceTitle, op: 'tiles', method: 'GET', url: url,
                anonymous: isAnonymous(), state: 'pending', status: null, ms: null, size: 0, contentType: '', tiles: []
            };
            state.tileGroup = group;
            addEntry(group);
        }
        var tile = { url: url, state: 'pending', status: null, ms: null, size: null };
        group.tiles.push(tile);
        var started = performance.now();
        updateEntry(group);

        return function (status, size, contentType, failed) {
            tile.ms = performance.now() - started;
            tile.status = status;
            tile.size = size;
            tile.state = failed ? 'failed' : (status >= 200 && status < 300 ? 'ok' : 'error');
            group.size = group.tiles.reduce(function (sum, t) { return sum + (t.size || 0); }, 0);
            group.ms = Math.max.apply(null, group.tiles.map(function (t) { return t.ms || 0; }));
            group.contentType = group.contentType || contentType || '';
            group.state = group.tiles.some(function (t) { return t.state === 'pending'; }) ? 'pending'
                : group.tiles.some(function (t) { return t.state !== 'ok'; }) ? 'error' : 'ok';
            updateEntry(group);
        };
    }

    // ---- absolute urls from capabilities with another host than this page

    function asAdvertised() { return $('tm-asadvertised').checked; }

    function noteOrigin(url) {
        var origin;
        try { origin = new URL(url, location.href).origin; } catch (e) { return; }
        if (origin === location.origin || state.advertisedOrigin === origin) { return; }
        state.advertisedOrigin = origin;
        $('tm-notice-text').textContent = 'The capabilities point to ' + origin + ', another host than this page (' + location.origin + '): '
            + 'check the online resource of the server. ' + (asAdvertised() ? 'The requests go to that host.' : 'The requests are sent to this server instead.');
        $('tm-notice').hidden = false;
    }

    /* an url from the capabilities: rewritten to this host unless "Use URLs as advertised" */
    function rewrite(url) {
        noteOrigin(url);
        if (asAdvertised()) { return url; }
        try {
            var u = new URL(url, location.href);
            if (u.origin === location.origin) { return url; }
            // keep the template braces of WMTS urls ({TileMatrix}, ...): URL would encode them
            return location.origin + url.substring(u.origin.length);
        } catch (e) { return url; }
    }

    function resetNotice() {
        state.advertisedOrigin = null;
        $('tm-notice').hidden = true;
    }

    // ==================================================================== projections

    var axisNorthFirst = {};   // 'EPSG:4326' → true: WMS 1.3.0 wants north (lat) first

    /* asks the server (not logged: it is not a request of the service) for unknown EPSG codes */
    function ensureProjections(codes) {
        var missing = codes.filter(function (c) { return !(c in axisNorthFirst); });
        if (!missing.length) { return Promise.resolve(); }

        return fetch(withQuery(cfg.projectionsUrl, { codes: missing.join(',') }), { credentials: 'same-origin' })
            .then(function (r) { return r.ok ? r.json() : {}; })
            .then(function (defs) {
                missing.forEach(function (code) {
                    var def = defs[code];
                    axisNorthFirst[code] = !!(def && def.axis === 'neu');
                    if (def && !ol.proj.get(code)) {
                        proj4.defs(code, def.proj4);
                    }
                });
                ol.proj.proj4.register(proj4);
            })
            .catch(function () { missing.forEach(function (c) { axisNorthFirst[c] = false; }); });
    }

    function usable(code) { return !!ol.proj.get(code); }

    // ==================================================================== interfaces

    function layerIds(caps, visibleOnly) {
        return caps.layers
            .filter(function (l) { return !l.isGroup && (!visibleOnly || l.visible); })
            .map(function (l) { return l.id; });
    }

    var adapters = {};

    // ---- WMS 1.3.0 ---------------------------------------------------------
    adapters.wms = {
        capabilities: function (iface, crs) {
            return send({ iface: iface.title, op: 'capabilities', url: withQuery(iface.url, { SERVICE: 'WMS', VERSION: '1.3.0', REQUEST: 'GetCapabilities' }), as: 'text' })
                .then(function (r) {
                    if (!r.ok) { throw new Error('GetCapabilities failed (' + r.status + ')'); }
                    var xml = new DOMParser().parseFromString(r.text, 'application/xml');
                    if (xml.getElementsByTagName('parsererror').length) { throw new Error('GetCapabilities: invalid XML'); }

                    function children(node, name) {
                        return Array.prototype.filter.call(node.childNodes, function (n) { return n.localName === name; });
                    }
                    function text(node, name) {
                        var c = children(node, name)[0];
                        return c ? c.textContent.trim() : '';
                    }

                    var capability = xml.getElementsByTagNameNS('*', 'Capability')[0];
                    var root = capability ? children(capability, 'Layer')[0] : null;
                    if (!root) { throw new Error('GetCapabilities: no layers'); }

                    var layers = [];
                    function walk(node, parentId) {
                        children(node, 'Layer').forEach(function (l) {
                            var id = text(l, 'Name') || ('group-' + layers.length);
                            var sub = children(l, 'Layer');
                            layers.push({
                                id: id,
                                name: text(l, 'Title') || id,
                                parentId: parentId,
                                isGroup: sub.length > 0,
                                visible: true,
                                queryable: l.getAttribute('queryable') === '1'
                            });
                            walk(l, id);
                        });
                    }
                    walk(root, null);

                    var crsList = children(root, 'CRS').map(function (c) { return c.textContent.trim().toUpperCase(); })
                        .filter(function (c) { return /^EPSG:\d+$/.test(c); });

                    var boxes = {};
                    children(root, 'BoundingBox').forEach(function (b) {
                        boxes[(b.getAttribute('CRS') || '').toUpperCase()] = ['minx', 'miny', 'maxx', 'maxy'].map(function (a) { return parseFloat(b.getAttribute(a)); });
                    });
                    var geo = children(root, 'EX_GeographicBoundingBox')[0];
                    var geoExtent = geo
                        ? [parseFloat(text(geo, 'westBoundLongitude')), parseFloat(text(geo, 'southBoundLatitude')), parseFloat(text(geo, 'eastBoundLongitude')), parseFloat(text(geo, 'northBoundLatitude'))]
                        : null;

                    var getFeatureInfo = xml.getElementsByTagNameNS('*', 'GetFeatureInfo')[0];
                    var infoFormats = getFeatureInfo ? children(getFeatureInfo, 'Format').map(function (f) { return f.textContent.trim(); }) : [];

                    return ensureProjections(crsList).then(function () {
                        var full = null;
                        Object.keys(boxes).forEach(function (code) {
                            if (!full && usable(code) && validExtent(boxes[code])) {
                                var b = boxes[code];
                                full = { crs: code, extent: axisNorthFirst[code] ? [b[1], b[0], b[3], b[2]] : b };
                            }
                        });
                        if (!full && validExtent(geoExtent)) { full = { crs: 'EPSG:4326', extent: geoExtent }; }

                        return { layers: layers, crsList: crsList, fullExtent: full, initialExtent: null, infoFormats: infoFormats };
                    });
                });
        },

        bbox: function (extent, crs) {
            var e = axisNorthFirst[crs] ? [extent[1], extent[0], extent[3], extent[2]] : extent;
            return bboxString(e, crs);
        },

        mapRequest: function (iface, caps, extent, crs, width, height) {
            return {
                url: withQuery(iface.url, {
                    SERVICE: 'WMS', VERSION: '1.3.0', REQUEST: 'GetMap',
                    LAYERS: layerIds(caps, true).join(','), STYLES: '',
                    CRS: crs, BBOX: this.bbox(extent, crs),
                    WIDTH: width, HEIGHT: height, FORMAT: 'image/png', TRANSPARENT: 'TRUE'
                })
            };
        },

        identifyRequest: function (iface, caps, click) {
            var visible = layerIds(caps, true);
            var queryable = caps.layers.filter(function (l) { return !l.isGroup && l.visible && l.queryable; }).map(function (l) { return l.id; });
            return {
                url: withQuery(iface.url, {
                    SERVICE: 'WMS', VERSION: '1.3.0', REQUEST: 'GetFeatureInfo',
                    LAYERS: visible.join(','), QUERY_LAYERS: queryable.join(','), STYLES: '',
                    CRS: click.crs, BBOX: this.bbox(click.extent, click.crs),
                    WIDTH: click.width, HEIGHT: click.height,
                    I: Math.round(click.pixel[0]), J: Math.round(click.pixel[1]),
                    INFO_FORMAT: $('tm-infoformat').value || 'text/plain', FEATURE_COUNT: 10
                })
            };
        },

        identifyResults: function (r) {
            if (/json/.test(r.entry.contentType)) {
                var json = JSON.parse(r.text);
                // GetFeatureInfo (json) geometries are in the crs of the request (x/y)
                var features = (json.features || []).map(function (f) {
                    return { attributes: f.properties || {}, geometry: f.geometry ? geoJson.readGeometry(f.geometry) : null };
                });
                return [{ layer: 'Features', features: features }];
            }
            return null;  // shown as raw text
        }
    };

    // ---- GeoServices REST (MapServer) ---------------------------------------
    adapters.geoservices = {
        capabilities: function (iface) {
            return send({ iface: iface.title, op: 'capabilities', url: withQuery(iface.url, { f: 'json' }), as: 'text' })
                .then(function (r) {
                    var json = JSON.parse(r.text || '{}');
                    if (!r.ok || json.error) { throw new Error('MapServer: ' + (json.error && json.error.message || r.status)); }

                    var groups = {};
                    (json.layers || []).forEach(function (l) { if (l.parentLayerId >= 0) { groups[l.parentLayerId] = true; } });
                    var layers = (json.layers || []).map(function (l) {
                        return {
                            id: String(l.id),
                            name: l.name,
                            parentId: l.parentLayerId >= 0 ? String(l.parentLayerId) : null,
                            isGroup: !!groups[l.id] || (l.subLayerIds && l.subLayerIds.length > 0),
                            visible: l.defaultVisibility !== false,
                            queryable: true
                        };
                    });

                    function ext(e) {
                        if (!e || !e.spatialReference) { return null; }
                        var code = 'EPSG:' + (e.spatialReference.latestWkid || e.spatialReference.wkid);
                        var extent = [e.xmin, e.ymin, e.xmax, e.ymax];
                        return validExtent(extent) ? { crs: code, extent: extent } : null;
                    }

                    var sr = json.spatialReference ? 'EPSG:' + (json.spatialReference.latestWkid || json.spatialReference.wkid) : null;
                    var crsList = [sr, cfg.epsg > 0 ? 'EPSG:' + cfg.epsg : null, 'EPSG:3857', 'EPSG:4326']
                        .filter(function (c, i, a) { return c && a.indexOf(c) === i; });

                    return ensureProjections(crsList).then(function () {
                        return { layers: layers, crsList: crsList, fullExtent: ext(json.fullExtent), initialExtent: ext(json.initialExtent) };
                    });
                });
        },

        mapRequest: function (iface, caps, extent, crs, width, height) {
            var sr = epsgNumber(crs);
            return {
                url: withQuery(iface.url + '/export', {
                    bbox: bboxString(extent, crs), bboxSR: sr, imageSR: sr,
                    size: width + ',' + height, format: 'png', transparent: 'true', dpi: 96,
                    layers: 'show:' + (layerIds(caps, true).join(',') || '-1'),
                    f: 'image'
                })
            };
        },

        identifyRequest: function (iface, caps, click) {
            return {
                url: withQuery(iface.url + '/identify', {
                    geometry: round(click.coordinate[0], click.crs) + ',' + round(click.coordinate[1], click.crs),
                    geometryType: 'esriGeometryPoint', sr: epsgNumber(click.crs),
                    layers: 'visible:' + (layerIds(caps, true).join(',') || '-1'),
                    tolerance: 3, mapExtent: bboxString(click.extent, click.crs),
                    imageDisplay: click.width + ',' + click.height + ',96',
                    returnGeometry: 'true', f: 'json'
                })
            };
        },

        identifyResults: function (r) {
            var json = JSON.parse(r.text);
            var byLayer = {};
            var order = [];
            (json.results || []).forEach(function (res) {
                var key = res.layerName || String(res.layerId);
                if (!byLayer[key]) { byLayer[key] = []; order.push(key); }
                byLayer[key].push({
                    attributes: res.attributes || {},
                    geometry: res.geometry ? esriJson.readGeometry(res.geometry) : null
                });
            });
            return order.map(function (k) { return { layer: k, features: byLayer[k] }; });
        }
    };

    // ---- GeoJson service -----------------------------------------------------
    adapters.geojsonservice = {
        capabilities: function (iface, crs) {
            var serviceCrs = cfg.epsg > 0 ? 'EPSG:' + cfg.epsg : 'EPSG:4326';
            return send({ iface: iface.title, op: 'capabilities', url: withQuery(iface.url + '/capabilities', { crs: serviceCrs }), as: 'text' })
                .then(function (r) {
                    var json = JSON.parse(r.text || '{}');
                    if (!r.ok || json.type === 'ErrorResponse' || json.error) {
                        throw new Error('capabilities: ' + (json.errorMessage || json.message || (json.error && json.error.message) || r.status));
                    }

                    var groups = {};
                    (json.layers || []).forEach(function (l) { if (l.parentId) { groups[l.parentId] = true; } });
                    var layers = (json.layers || []).map(function (l) {
                        return {
                            id: String(l.id),
                            name: l.name,
                            parentId: l.parentId ? String(l.parentId) : null,
                            isGroup: !!groups[l.id] || /group/i.test(l.layerType || ''),
                            visible: l.defaultVisibility !== false,
                            queryable: true
                        };
                    });

                    var capsCrs = json.crs && json.crs.properties && json.crs.properties.name
                        ? json.crs.properties.name.toUpperCase()
                        : serviceCrs;

                    function ext(b) {
                        if (!b) { return null; }
                        var extent = [b.minX, b.minY, b.maxX, b.maxY];
                        return validExtent(extent) ? { crs: capsCrs, extent: extent } : null;
                    }

                    var crsList = [serviceCrs, 'EPSG:3857', 'EPSG:4326'].filter(function (c, i, a) { return a.indexOf(c) === i; });
                    return ensureProjections(crsList.concat([capsCrs])).then(function () {
                        return { layers: layers, crsList: crsList, fullExtent: ext(json.fullExtent), initialExtent: ext(json.initialExtent) };
                    });
                });
        },

        mapRequest: function (iface, caps, extent, crs, width, height) {
            return {
                url: withQuery(iface.url + '/map', {
                    bbox: bboxString(extent, crs), crs: crs,
                    width: width, height: height, format: 'png', transparent: 'true', dpi: 96,
                    layers: layerIds(caps, true).join(',') || '-1',
                    responseFormat: 'image'
                })
            };
        },

        identifyRequest: function (iface, caps, click) {
            return {
                url: withQuery(iface.url + '/identify', {
                    geometry: round(click.coordinate[0], click.crs) + ',' + round(click.coordinate[1], click.crs),
                    crs: click.crs,
                    layerOption: 'visible',
                    layers: layerIds(caps, true).join(',') || '-1',
                    tolerance: 3, mapExtent: bboxString(click.extent, click.crs),
                    imageWidth: click.width, imageHeight: click.height, dpi: 96,
                    returnGeometry: 'geometry', outCRS: click.crs
                })
            };
        },

        identifyResults: function (r) {
            var json = JSON.parse(r.text);
            return (json.results || []).map(function (res) {
                var fc = res.featureCollection || {};
                return {
                    layer: res.layerName || res.layerId,
                    features: (fc.features || []).map(function (f) {
                        var attributes = f.properties || {};
                        return { attributes: attributes, geometry: f.geometry ? geoJson.readGeometry(f.geometry) : null };
                    })
                };
            });
        }
    };

    // ---- query helpers -------------------------------------------------------

    function normalizeCrs(crs) {
        var value = String(crs || '').trim();
        var m = /EPSG(?::|\/0\/|::)(\d+)$/i.exec(value);
        if (m) { return 'EPSG:' + m[1]; }
        if (/CRS84$/i.test(value)) { return 'EPSG:4326'; }
        return null;
    }

    function crsUri(crs) {
        return 'http://www.opengis.net/def/crs/EPSG/0/' + epsgNumber(crs);
    }

    /* bbox in the axis order of the crs (OGC API bbox-crs, WFS 1.1.0, WMS 1.3.0) */
    function axisBbox(extent, crs) {
        var e = axisNorthFirst[crs] ? [extent[1], extent[0], extent[3], extent[2]] : extent;
        return bboxString(e, crs);
    }

    /* OGC API returns coordinates in the axis order of the crs: north first for EPSG:4326 */
    function swapXY(geometry) {
        if (geometry) {
            geometry.applyTransform(function (input, output, dimension) {
                var out = output || input;
                var stride = dimension || 2;
                for (var i = 0; i < input.length; i += stride) {
                    var x = input[i];
                    out[i] = input[i + 1];
                    out[i + 1] = x;
                }
                return out;
            });
        }
        return geometry;
    }

    function visibleLayers(caps) {
        return caps.layers.filter(function (l) { return !l.isGroup && l.visible; });
    }

    /* small box around a click: tolerance in pixels */
    function clickBox(click, pixels) {
        var resolution = (click.extent[2] - click.extent[0]) / click.width;
        var d = resolution * pixels;
        return [click.coordinate[0] - d, click.coordinate[1] - d, click.coordinate[0] + d, click.coordinate[1] + d];
    }

    function queryLimit() { return parseInt($('tm-limit').value, 10) || 100; }

    // ---- OGC API (Features, Maps) --------------------------------------------
    adapters.ogcapi = {
        capabilities: function (iface) {
            return send({ iface: iface.title, op: 'capabilities', url: withQuery(iface.url, { f: 'json' }), as: 'text' })
                .then(function (r) {
                    var landing = JSON.parse(r.text || '{}');
                    if (!r.ok) { throw new Error('landing page: ' + (landing.title || landing.detail || r.status)); }
                    var links = landing.links || [];
                    var self = links.filter(function (l) { return l.rel === 'self'; })[0];
                    if (self) { noteOrigin(self.href); }
                    var hasMap = links.some(function (l) { return /\/map$/.test(l.rel || ''); });

                    return send({ iface: iface.title, op: 'capabilities', url: withQuery(iface.url + '/collections', { f: 'json' }), as: 'text' })
                        .then(function (r2) {
                            var json = JSON.parse(r2.text || '{}');
                            if (!r2.ok) { throw new Error('collections: ' + (json.title || json.detail || r2.status)); }

                            var crsList = [];
                            var bbox = null;
                            var layers = (json.collections || []).map(function (c) {
                                (c.crs || []).map(normalizeCrs).forEach(function (code) { if (code && crsList.indexOf(code) < 0) { crsList.push(code); } });
                                var b = c.extent && c.extent.spatial && c.extent.spatial.bbox && c.extent.spatial.bbox[0];
                                if (b && b.length >= 4) {
                                    bbox = bbox ? [Math.min(bbox[0], b[0]), Math.min(bbox[1], b[1]), Math.max(bbox[2], b[2]), Math.max(bbox[3], b[3])] : b.slice(0, 4);
                                }
                                return {
                                    id: String(c.id), name: c.title || c.id, parentId: null, isGroup: false, visible: true, queryable: true,
                                    // the crs the items of this collection support (CRS84 is the default, not listed here)
                                    crs: (c.crs || []).filter(function (u) { return !/CRS84$/i.test(u); }).map(normalizeCrs).filter(Boolean)
                                };
                            });

                            // the maps of gView answer in EPSG:3857 and EPSG:4326 (see the OpenAPI document)
                            if (hasMap) { crsList = ['EPSG:3857', 'EPSG:4326']; }

                            return ensureProjections(crsList).then(function () {
                                return {
                                    kind: hasMap ? 'image' : 'vector',
                                    layers: layers,
                                    crsList: crsList,
                                    fullExtent: validExtent(bbox) ? { crs: 'EPSG:4326', extent: bbox } : null,
                                    initialExtent: null
                                };
                            });
                        });
                });
        },

        mapRequest: function (iface, caps, extent, crs, width, height) {
            return {
                url: withQuery(iface.url + '/map', {
                    f: 'png', bbox: axisBbox(extent, crs), 'bbox-crs': crsUri(crs), crs: crsUri(crs),
                    width: width, height: height, transparent: 'true',
                    collections: visibleLayers(caps).map(function (l) { return l.id; }).join(',')
                })
            };
        },

        items: function (iface, layer, extent, crs, limit, op, signal) {
            // the crs of the view only if the collection supports it, else CRS84 (the default) like a client
            var viewCrs = (layer.crs || []).indexOf(crs) >= 0;
            var params = viewCrs
                ? { f: 'json', bbox: axisBbox(extent, crs), 'bbox-crs': crsUri(crs), crs: crsUri(crs), limit: limit }
                : { f: 'json', bbox: bboxString(ol.proj.transformExtent(extent, crs, 'EPSG:4326'), 'EPSG:4326'), limit: limit };

            return send({
                iface: iface.title, op: op, signal: signal, as: 'text',
                url: withQuery(iface.url + '/collections/' + encodeURIComponent(layer.id) + '/items', params)
            }).then(function (r) {
                var json = JSON.parse(r.text || '{}');
                if (!r.ok) { return { layer: layer.name, features: [], warning: json.detail || json.title || ('HTTP ' + r.status) }; }
                return {
                    layer: layer.name,
                    more: (json.links || []).some(function (l) { return l.rel === 'next'; }),
                    features: (json.features || []).map(function (f) {
                        var g = f.geometry ? geoJson.readGeometry(f.geometry) : null;
                        if (g && viewCrs && axisNorthFirst[crs]) { swapXY(g); }
                        if (g && !viewCrs && crs !== 'EPSG:4326') { g.transform('EPSG:4326', crs); }   // CRS84: lon/lat
                        return { attributes: f.properties || {}, geometry: g };
                    })
                };
            });
        },

        identify: function (iface, caps, click) {
            var self = this;
            return Promise.all(visibleLayers(caps).map(function (l) {
                return self.items(iface, l, clickBox(click, 3), click.crs, 10, 'identify');
            })).then(function (groups) { return groups.filter(function (g) { return g.features.length || g.warning; }); });
        },

        loadFeatures: function (iface, caps, extent, crs, limit, signal) {
            var self = this;
            return Promise.all(visibleLayers(caps).map(function (l) { return self.items(iface, l, extent, crs, limit, 'features', signal); }));
        },

        queryExtent: function (iface, caps, extent, crs, limit) {
            var self = this;
            return Promise.all(visibleLayers(caps).map(function (l) { return self.items(iface, l, extent, crs, limit, 'query'); }));
        }
    };

    // ---- WMTS 1.0.0 ------------------------------------------------------------
    adapters.wmts = {
        kind: 'tiles',
        layerMode: 'radio',
        capabilities: function (iface) {
            return send({ iface: iface.title, op: 'capabilities', url: withQuery(iface.url, { SERVICE: 'WMTS', VERSION: '1.0.0', REQUEST: 'GetCapabilities' }), as: 'text' })
                .then(function (r) {
                    if (!r.ok) { throw new Error('GetCapabilities failed (' + r.status + ')'); }
                    var error = /<ERROR>([^<]*)<\/ERROR>/i.exec(r.text || '');
                    if (error) { throw new Error('WMTS: ' + error[1] + ' (the service has no tile cache)'); }

                    var parsed = new ol.format.WMTSCapabilities().read(r.text);
                    var contents = parsed.Contents || {};
                    var sets = contents.TileMatrixSet || [];
                    var codes = [];
                    sets.forEach(function (s) {
                        var code = normalizeCrs(s.SupportedCRS);
                        if (code) {
                            s.SupportedCRS = code;   // urn:ogc:def:crs:EPSG::31256 → EPSG:31256, the name OpenLayers knows
                            if (codes.indexOf(code) < 0) { codes.push(code); }
                        }
                    });

                    var layers = (contents.Layer || []).map(function (l, i) {
                        var setId = l.TileMatrixSetLink && l.TileMatrixSetLink[0] && l.TileMatrixSetLink[0].TileMatrixSet;
                        var set = sets.filter(function (s) { return s.Identifier === setId; })[0];
                        return {
                            id: l.Identifier, name: l.Title || l.Identifier, parentId: null, isGroup: false, visible: i === 0, queryable: false,
                            matrixSet: setId, crs: set ? set.SupportedCRS : null,
                            styles: (l.Style || []).map(function (st) { return st.Identifier; }),
                            wgs84: l.WGS84BoundingBox
                        };
                    });
                    if (!layers.length) { throw new Error('WMTS: the capabilities list no layers'); }

                    return ensureProjections(codes).then(function () {
                        // TopLeftCorner and the BoundingBox of the layers are in the axis order of the crs:
                        // OpenLayers expects x/y here
                        sets.forEach(function (s) {
                            if (axisNorthFirst[s.SupportedCRS]) {
                                (s.TileMatrix || []).forEach(function (m) { m.TopLeftCorner = [m.TopLeftCorner[1], m.TopLeftCorner[0]]; });
                            }
                        });
                        (contents.Layer || []).forEach(function (l) {
                            (l.BoundingBox || []).forEach(function (b) {
                                var code = normalizeCrs(b.crs);
                                if (code) { b.crs = code; }
                                if (code && axisNorthFirst[code] && b.extent) { b.extent = [b.extent[1], b.extent[0], b.extent[3], b.extent[2]]; }
                            });
                        });
                        var caps = { parsed: parsed, layers: layers, initialExtent: null };
                        adapters.wmts.selectLayer(caps, layers[0]);
                        return caps;
                    });
                });
        },

        /* one cache at a time: its grid decides the crs */
        selectLayer: function (caps, layer) {
            caps.layers.forEach(function (l) { l.visible = l === layer; });
            caps.crsList = layer.crs ? [layer.crs] : [];
            caps.styles = layer.styles;
            caps.style = layer.styles.indexOf(caps.style) >= 0 ? caps.style : (layer.styles[0] || 'default');
            caps.fullExtent = validExtent(layer.wgs84) ? { crs: 'EPSG:4326', extent: layer.wgs84 } : null;
        },

        createSource: function (iface, caps) {
            var layer = caps.layers.filter(function (l) { return l.visible; })[0];
            var options = ol.source.WMTS.optionsFromCapabilities(caps.parsed, { layer: layer.id, matrixSet: layer.matrixSet, style: caps.style });
            if (!options) { throw new Error('WMTS: no tile matrix set for ' + layer.id); }
            options.urls = (options.urls || []).map(rewrite);
            options.tileLoadFunction = function (tile, src) {
                var done = logTile(iface.title, src);
                fetch(src, { credentials: isAnonymous() ? 'omit' : 'same-origin' })
                    .then(function (response) {
                        return response.blob().then(function (blob) {
                            var type = (response.headers.get('content-type') || '').split(';')[0];
                            done(response.status, blob.size, type, false);
                            if (!response.ok || type.indexOf('image/') !== 0) { tile.setState(3); return; }   // TileState.ERROR
                            var url = URL.createObjectURL(blob);
                            var img = tile.getImage();
                            img.onload = img.onerror = function () { URL.revokeObjectURL(url); };
                            img.src = url;
                        });
                    })
                    .catch(function () { done(null, null, '', true); tile.setState(3); });
            };
            return new ol.source.WMTS(options);
        }
    };

    // ---- WFS 1.1.0 (GML) --------------------------------------------------------
    adapters.wfs = {
        kind: 'vector',
        capabilities: function (iface) {
            return send({ iface: iface.title, op: 'capabilities', url: withQuery(iface.url, { SERVICE: 'WFS', VERSION: '1.1.0', REQUEST: 'GetCapabilities' }), as: 'text' })
                .then(function (r) {
                    if (!r.ok) { throw new Error('GetCapabilities failed (' + r.status + ')'); }
                    var xml = new DOMParser().parseFromString(r.text, 'application/xml');
                    if (xml.getElementsByTagName('parsererror').length) { throw new Error('GetCapabilities: invalid XML'); }

                    function first(node, name) { return node.getElementsByTagNameNS('*', name)[0]; }
                    function text(node, name) { var n = first(node, name); return n ? n.textContent.trim() : ''; }

                    var crsList = [];
                    var bbox = null;
                    var layers = Array.prototype.map.call(xml.getElementsByTagNameNS('*', 'FeatureType'), function (ft) {
                        Array.prototype.forEach.call(ft.childNodes, function (n) {
                            if (n.localName === 'DefaultSRS' || n.localName === 'OtherSRS' || n.localName === 'SRS') {
                                var code = normalizeCrs(n.textContent);
                                if (code && crsList.indexOf(code) < 0) { crsList.push(code); }
                            }
                        });
                        var wgs = first(ft, 'WGS84BoundingBox');
                        if (wgs) {
                            var lo = text(wgs, 'LowerCorner').split(/\s+/).map(parseFloat), up = text(wgs, 'UpperCorner').split(/\s+/).map(parseFloat);
                            var b = [lo[0], lo[1], up[0], up[1]];
                            if (validExtent(b)) { bbox = bbox ? [Math.min(bbox[0], b[0]), Math.min(bbox[1], b[1]), Math.max(bbox[2], b[2]), Math.max(bbox[3], b[3])] : b; }
                        }
                        var name = text(ft, 'Name');
                        return { id: name, name: (text(ft, 'Title') || name), parentId: null, isGroup: false, visible: true, queryable: true };
                    });

                    return ensureProjections(crsList).then(function () {
                        return { layers: layers, crsList: crsList, fullExtent: bbox ? { crs: 'EPSG:4326', extent: bbox } : null, initialExtent: null };
                    });
                });
        },

        loadFeatures: function (iface, caps, extent, crs, limit, signal) {
            return Promise.all(visibleLayers(caps).map(function (l) {
                // the type name exactly as the capabilities advertise it, like a client
                return send({
                    iface: iface.title, op: 'features', signal: signal, as: 'text',
                    url: withQuery(iface.url, {
                        SERVICE: 'WFS', VERSION: '1.1.0', REQUEST: 'GetFeature', TYPENAME: l.id,
                        SRSNAME: crs, BBOX: axisBbox(extent, crs) + ',' + crs, MAXFEATURES: limit
                    })
                }).then(function (r) {
                    var text = r.text || '';
                    if (!r.ok || !/FeatureCollection/.test(text)) {
                        r.entry.warning = 'No FeatureCollection in the response';
                        updateEntry(r.entry);
                        return { layer: l.name, features: [], warning: r.entry.warning };
                    }
                    var srs = /srsName="([^"]+)"/.exec(text);
                    var dataCrs = srs ? normalizeCrs(srs[1]) : crs;
                    var olFeatures = [];
                    try { olFeatures = new ol.format.GML2().readFeatures(text); } catch (e) { olFeatures = []; }

                    return ensureProjections(dataCrs ? [dataCrs] : []).then(function () {
                        var warning = null;
                        if (dataCrs && dataCrs !== crs) {
                            warning = 'Geometries in ' + dataCrs + ', requested ' + crs + ' (SRSNAME)';
                            r.entry.warning = warning;
                            updateEntry(r.entry);
                        }
                        return {
                            layer: l.name,
                            warning: warning,
                            more: olFeatures.length >= limit,
                            features: olFeatures.map(function (f) {
                                var g = f.getGeometry();
                                // gView writes the coordinates in the axis order of the crs (north first for EPSG:31256, 4326)
                                if (g && dataCrs && axisNorthFirst[dataCrs]) { g = swapXY(g.clone()); }
                                if (g && dataCrs && dataCrs !== crs && usable(dataCrs)) { g = g.clone().transform(dataCrs, crs); }
                                var attributes = f.getProperties();
                                delete attributes[f.getGeometryName()];
                                delete attributes.boundedBy;
                                return { attributes: attributes, geometry: g || null };
                            })
                        };
                    });
                });
            }));
        }
    };

    // ---- bbox queries of the image interfaces ("Features in extent") -----------
    adapters.geoservices.queryExtent = function (iface, caps, extent, crs, limit) {
        var sr = epsgNumber(crs);
        return Promise.all(visibleLayers(caps).map(function (l) {
            return send({
                iface: iface.title, op: 'query', as: 'text',
                url: withQuery(iface.url + '/' + encodeURIComponent(l.id) + '/query', {
                    geometry: bboxString(extent, crs), geometryType: 'esriGeometryEnvelope', inSR: sr, outSR: sr,
                    spatialRel: 'esriSpatialRelIntersects', outFields: '*', returnGeometry: 'true', resultRecordCount: limit, f: 'json'
                })
            }).then(function (r) {
                var json = JSON.parse(r.text || '{}');
                if (!r.ok || json.error) { return { layer: l.name, features: [], warning: (json.error && json.error.message) || ('HTTP ' + r.status) }; }
                return {
                    layer: l.name,
                    more: !!json.exceededTransferLimit,
                    features: (json.features || []).map(function (f) {
                        return { attributes: f.attributes || {}, geometry: f.geometry ? esriJson.readGeometry(f.geometry) : null };
                    })
                };
            });
        }));
    };

    adapters.geojsonservice.queryExtent = function (iface, caps, extent, crs, limit) {
        return Promise.all(visibleLayers(caps).map(function (l) {
            return send({
                iface: iface.title, op: 'query', as: 'text',
                url: withQuery(iface.url + '/query/' + encodeURIComponent(l.id), {
                    bbox: bboxString(extent, crs), 'bbox-crs': crs, outCRS: crs, returnGeometry: 'geometry', limit: limit
                })
            }).then(function (r) {
                var json = JSON.parse(r.text || '{}');
                if (!r.ok || json.type === 'ErrorResponse') { return { layer: l.name, features: [], warning: json.errorMessage || json.message || ('HTTP ' + r.status) }; }
                return {
                    layer: l.name,
                    more: !!json.exceededLimit,
                    features: (json.features || []).map(function (f) {
                        return { attributes: f.properties || {}, geometry: f.geometry ? geoJson.readGeometry(f.geometry) : null };
                    })
                };
            });
        }));
    };

    var geoJson = new ol.format.GeoJSON();
    var esriJson = new ol.format.EsriJSON();

    // ==================================================================== state

    var state = {
        iface: null,      // { key, title, url }
        caps: null,
        crs: null,
        controller: null, // AbortController of the running map request
        vectorController: null,
        vectorTimer: null,
        tileGroup: null,  // log entry of the tiles of the current view
        advertisedOrigin: null,
        pending: 0,
        busySince: null
    };

    function adapter() { return adapters[state.iface.key]; }

    /* image (one map image per view), tiles (WMTS) or vector (the features of each view) */
    function kind() {
        return (state.caps && state.caps.kind) || adapter().kind || 'image';
    }

    // ==================================================================== map

    var imageLayer = new ol.layer.Image();
    var tileLayer = new ol.layer.Tile();
    var vectorSource = new ol.source.Vector();
    var vectorLayer = new ol.layer.Vector({
        source: vectorSource,
        style: new ol.style.Style({
            stroke: new ol.style.Stroke({ color: '#c2410c', width: 1.5 }),
            fill: new ol.style.Fill({ color: 'rgba(234,88,12,.12)' }),
            image: new ol.style.Circle({
                radius: 4,
                stroke: new ol.style.Stroke({ color: '#fff', width: 1 }),
                fill: new ol.style.Fill({ color: '#ea580c' })
            })
        })
    });
    var highlightSource = new ol.source.Vector();
    var highlightLayer = new ol.layer.Vector({
        source: highlightSource,
        style: new ol.style.Style({
            stroke: new ol.style.Stroke({ color: '#2563eb', width: 3 }),
            fill: new ol.style.Fill({ color: 'rgba(37,99,235,.15)' }),
            image: new ol.style.Circle({
                radius: 6,
                stroke: new ol.style.Stroke({ color: '#fff', width: 2 }),
                fill: new ol.style.Fill({ color: '#2563eb' })
            })
        })
    });
    var clickSource = new ol.source.Vector();
    var clickLayer = new ol.layer.Vector({
        source: clickSource,
        style: new ol.style.Style({
            image: new ol.style.Circle({
                radius: 5,
                stroke: new ol.style.Stroke({ color: '#d13b3b', width: 2 }),
                fill: new ol.style.Fill({ color: 'rgba(209,59,59,.25)' })
            })
        })
    });

    var map = new ol.Map({
        target: 'tm-map',
        layers: [tileLayer, imageLayer, vectorLayer, highlightLayer, clickLayer],
        controls: ol.control.defaults.defaults({ attribution: false }).extend([new ol.control.ScaleLine()]),
        view: new ol.View({ projection: 'EPSG:4326', center: [0, 0], zoom: 2 })
    });

    // for inspection in the browser console
    window.gviewTestMap = { map: map, state: state };

    function mapSize() { return map.getSize() || [800, 600]; }

    function currentExtent() {
        return map.getView().calculateExtent(mapSize());
    }

    function setBusy(delta) {
        state.pending = Math.max(0, state.pending + delta);
        if (state.pending > 0 && !state.busySince) {
            state.busySince = performance.now();
        } else if (state.pending === 0) {
            state.busySince = null;
        }
        updateBusy();
    }

    function updateBusy() {
        var busy = $('tm-busy');
        if (!state.busySince) { busy.hidden = true; return; }
        busy.hidden = false;
        busy.textContent = (state.iface && state.caps && kind() === 'vector' ? 'Loading features… ' : 'Loading map… ') + Math.floor((performance.now() - state.busySince) / 1000) + ' s';
    }
    setInterval(updateBusy, 500);

    function showMessage(text) {
        var m = $('tm-message');
        m.textContent = text || '';
        m.hidden = !text;
    }

    function loadImage(blob) {
        return new Promise(function (resolve, reject) {
            var url = URL.createObjectURL(blob);
            var img = new Image();
            img.onload = function () { URL.revokeObjectURL(url); resolve(img); };
            img.onerror = function () { URL.revokeObjectURL(url); reject(new Error('no image')); };
            img.src = url;
        });
    }

    /* the image source: one image at window size per view, a new view aborts the running request */
    function createImageSource() {
        var iface = state.iface, caps = state.caps, crs = state.crs;
        var adapter = adapters[iface.key];

        return new ol.source.Image({
            projection: crs,
            loader: function (extent, resolution) {
                var width = Math.round(ol.extent.getWidth(extent) / resolution);
                var height = Math.round(ol.extent.getHeight(extent) / resolution);

                if (state.controller) { state.controller.abort(); }
                var controller = new AbortController();
                state.controller = controller;

                var request = adapter.mapRequest(iface, caps, extent, crs, width, height);
                setBusy(1);

                return send({ iface: iface.title, op: 'map', url: request.url, signal: controller.signal, as: 'blob' })
                    .then(function (r) {
                        setBusy(-1);
                        if (!r.ok || !r.isImage) {
                            showMessage('The map request returned ' + (r.isImage ? r.status : (r.entry.contentType || r.status)) + ': see the request log.');
                            throw new Error('no map image');
                        }
                        showMessage(null);
                        // no HiDPI: the image has 1 pixel per css pixel, tell OpenLayers so it scales it right
                        return loadImage(r.blob).then(function (img) {
                            return { image: img, extent: extent, resolution: resolution, pixelRatio: 1 };
                        });
                    }, function (err) {
                        setBusy(-1);
                        if (err && err.name !== 'AbortError') { showMessage('The map request failed: ' + err.message); }
                        throw err;
                    });
            }
        });
    }

    function refreshMap() {
        if (!state.iface || !state.caps) { return; }
        var k = kind();
        state.tileGroup = null;
        imageLayer.setSource(k === 'image' ? createImageSource() : null);
        vectorSource.clear();
        if (k === 'tiles') {
            try {
                var source = adapter().createSource(state.iface, state.caps);
                tileLayer.setSource(source);
                // the cache has no tiles beyond its coarsest level: start there at most
                var coarsest = source.getTileGrid().getResolutions()[0];
                if (map.getView().getResolution() > coarsest) { map.getView().setResolution(coarsest); }
            } catch (e) {
                tileLayer.setSource(null);
                showMessage(e.message);
            }
        } else {
            tileLayer.setSource(null);
        }
        if (k === 'vector') { loadVector(); }
    }

    /* WFS, OGC API without maps: the features of the view, a new view aborts the running requests */
    function loadVector() {
        clearTimeout(state.vectorTimer);
        state.vectorTimer = setTimeout(function () {
            if (!state.iface || !state.caps || kind() !== 'vector') { return; }
            if (state.vectorController) { state.vectorController.abort(); }
            var controller = new AbortController();
            state.vectorController = controller;
            var iface = state.iface;

            setBusy(1);
            adapter().loadFeatures(iface, state.caps, currentExtent(), state.crs, queryLimit(), controller.signal)
                .then(function (groups) {
                    setBusy(-1);
                    if (state.vectorController !== controller) { return; }
                    vectorSource.clear();
                    var total = 0, more = false, warnings = [];
                    groups.forEach(function (g) {
                        more = more || g.more;
                        if (g.warning && warnings.indexOf(g.warning) < 0) { warnings.push(g.warning); }
                        g.features.forEach(function (f) {
                            if (!f.geometry) { return; }
                            var feature = new ol.Feature(f.geometry);
                            feature.set('tmLayer', g.layer);
                            feature.set('tmAttributes', f.attributes);
                            vectorSource.addFeature(feature);
                            total++;
                        });
                    });
                    showMessage(warnings.length ? warnings.join(' · ') : null);
                    $('tm-coords').textContent = total + ' features' + (more ? ' (limit reached)' : '');
                }, function (err) {
                    setBusy(-1);
                    if (err && err.name !== 'AbortError') { showMessage('Loading the features failed: ' + err.message); }
                });
        }, 0);
    }

    function storeExtent() {
        if (!state.crs) { return; }
        try { localStorage.setItem(storeKey, JSON.stringify({ crs: state.crs, extent: currentExtent() })); } catch (e) { }
    }

    function transformedExtent(ext, crs) {
        if (!ext || !validExtent(ext.extent) || !usable(ext.crs)) { return null; }
        var e = ext.crs === crs ? ext.extent : ol.proj.transformExtent(ext.extent, ext.crs, crs);
        return validExtent(e) ? e : null;
    }

    function fit(extent) {
        if (extent) { map.getView().fit(extent, { size: mapSize() }); }
    }

    function worldExtent(crs) {
        var p = ol.proj.get(crs);
        return (p && p.getExtent()) || ol.proj.transformExtent([-180, -85, 180, 85], 'EPSG:4326', crs);
    }

    /* switches the projection of the view, keeps the extent */
    function setView(crs, extent) {
        state.crs = crs;
        map.setView(new ol.View({ projection: crs, showFullExtent: true }));
        fit(extent || worldExtent(crs));
        imageLayer.setSource(null);
        highlightSource.clear();
        clickSource.clear();
    }

    map.on('moveend', function () {
        storeExtent();
        if (state.iface && state.caps && kind() === 'vector') { loadVector(); }
    });

    map.on('movestart', function () { state.tileGroup = null; });

    map.on('pointermove', function (evt) {
        if (!state.crs) { return; }
        var c = evt.coordinate;
        $('tm-coords').textContent = state.crs + '  ' + round(c[0], state.crs) + ', ' + round(c[1], state.crs);
    });

    // ==================================================================== layers panel

    function renderLayers() {
        var container = $('tm-layers');
        container.textContent = '';
        var caps = state.caps;
        if (!caps.layers.length) {
            container.appendChild(h('p', { className: 'muted', text: 'The capabilities list no layers.' }));
            return;
        }

        var byParent = {};
        caps.layers.forEach(function (l) {
            var key = l.parentId && caps.layers.some(function (p) { return p.id === l.parentId; }) ? l.parentId : '';
            (byParent[key] = byParent[key] || []).push(l);
        });

        function descendants(layer) {
            var result = [];
            (byParent[layer.id] || []).forEach(function (c) { result.push(c); result = result.concat(descendants(c)); });
            return result;
        }

        function list(parentKey) {
            var ul = h('ul', { className: 'tm-layer-list' });
            (byParent[parentKey] || []).forEach(function (l) {
                var radio = adapter().layerMode === 'radio';
                var checkbox = h('input', radio ? { type: 'radio', name: 'tm-layer' } : { type: 'checkbox' });
                checkbox.checked = l.isGroup ? descendants(l).some(function (d) { return d.visible; }) : l.visible;
                checkbox.addEventListener('change', function () {
                    if (radio) {
                        selectTileLayer(l);
                        return;
                    }
                    if (l.isGroup) {
                        descendants(l).forEach(function (d) { d.visible = checkbox.checked; });
                    } else {
                        l.visible = checkbox.checked;
                    }
                    renderLayers();
                    refreshMap();
                });
                var li = h('li', null, [
                    h('label', { className: l.isGroup ? 'tm-layer-group' : '' }, [checkbox, ' ' + l.name, h('span', { className: 'tm-layer-id', text: l.id })])
                ]);
                if (l.isGroup) { li.appendChild(list(l.id)); }
                ul.appendChild(li);
            });
            return ul;
        }

        container.appendChild(list(''));

        var styleRow = $('tm-style-row');
        styleRow.hidden = !(caps.styles && caps.styles.length > 1);
        if (!styleRow.hidden) {
            var select = $('tm-style');
            select.textContent = '';
            caps.styles.forEach(function (st) { select.appendChild(h('option', { value: st, text: st })); });
            select.value = caps.style;
        }
    }

    /* WMTS: another cache, possibly with another grid crs */
    function selectTileLayer(layer) {
        var caps = state.caps;
        adapter().selectLayer(caps, layer);
        var crs = caps.crsList[0];
        if (crs && crs !== state.crs && usable(crs)) {
            setView(crs, transformedExtent({ crs: state.crs, extent: currentExtent() }, crs) || transformedExtent(caps.fullExtent, crs));
        }
        fillCrs(caps.crsList);
        renderLayers();
        refreshMap();
    }

    // ==================================================================== identify

    function showSide(name) {
        document.querySelectorAll('.tm-side-tabs button').forEach(function (b) { b.classList.toggle('active', b.getAttribute('data-side') === name); });
        document.querySelectorAll('.tm-side-panel').forEach(function (p) { p.hidden = p.getAttribute('data-side') !== name; });
    }

    document.querySelectorAll('.tm-side-tabs button').forEach(function (b) {
        b.addEventListener('click', function () { showSide(b.getAttribute('data-side')); });
    });

    function attributeTable(features) {
        var keys = [];
        features.forEach(function (f) { Object.keys(f.attributes).forEach(function (k) { if (keys.indexOf(k) < 0) { keys.push(k); } }); });

        return h('div', null, features.map(function (f, i) {
            var feature = f.geometry ? new ol.Feature(f.geometry) : null;
            var head = h('div', { className: 'tm-feature-head' }, [
                h('span', { text: '#' + (i + 1) }),
                feature ? h('button', {
                    type: 'button', className: 'btn tm-small', text: 'Zoom',
                    onclick: function () {
                        highlightSource.clear();
                        highlightSource.addFeature(feature);
                        map.getView().fit(feature.getGeometry().getExtent(), { size: mapSize(), maxZoom: map.getView().getZoom() + 4, padding: [40, 40, 40, 40] });
                    }
                }) : null
            ]);
            return h('div', { className: 'tm-feature' }, [
                head,
                h('table', { className: 'tm-attributes' }, [h('tbody', null, keys.filter(function (k) { return k in f.attributes; }).map(function (k) {
                    var v = f.attributes[k];
                    return h('tr', null, [h('th', { text: k }), h('td', { text: v === null || v === undefined ? '' : (typeof v === 'object' ? JSON.stringify(v) : String(v)) })]);
                }))])
            ]);
        }));
    }

    function renderIdentify(groups, rawText) {
        var container = $('tm-identify');
        container.textContent = '';
        highlightSource.clear();

        if (groups === null) {
            // HTML, XML, text: shown as text, never rendered
            container.appendChild(h('pre', { className: 'tm-preview-text', text: (rawText || '').substring(0, PREVIEW_CHARS) }));
            return;
        }

        var total = 0;
        groups.forEach(function (g) {
            total += g.features.length;
            g.features.forEach(function (f) { if (f.geometry) { highlightSource.addFeature(new ol.Feature(f.geometry)); } });
            container.appendChild(h('details', { className: 'tm-result', open: groups.length < 6 }, [
                h('summary', null, [h('b', { text: g.layer }), ' ', h('span', { className: 'muted', text: '(' + g.features.length + ')' })]),
                g.warning ? h('p', { className: 'tm-warning', text: '⚠ ' + g.warning }) : null,
                g.more ? h('p', { className: 'tm-more', text: 'More features available: the limit was reached.' }) : null,
                attributeTable(g.features)
            ]));
        });

        if (!total && !groups.some(function (g) { return g.warning; })) {
            container.appendChild(h('p', { className: 'muted', text: 'Nothing found here.' }));
        }
    }

    map.on('singleclick', function (evt) {
        if (!state.iface || !state.caps) { return; }
        var adapter = adapters[state.iface.key];
        var size = mapSize();
        var click = {
            coordinate: evt.coordinate,
            pixel: evt.pixel,
            extent: currentExtent(),
            width: size[0],
            height: size[1],
            crs: state.crs
        };

        clickSource.clear();
        clickSource.addFeature(new ol.Feature(new ol.geom.Point(evt.coordinate)));
        showSide('identify');
        $('tm-identify').textContent = '';

        if (kind() === 'tiles') {
            $('tm-identify').appendChild(h('p', { className: 'muted', text: 'WMTS has no identify: the tiles are images only.' }));
            return;
        }

        if (kind() === 'vector') {
            // the features are loaded already: select them, no request
            var byLayer = {}, order = [];
            map.forEachFeatureAtPixel(evt.pixel, function (feature) {
                var name = feature.get('tmLayer');
                if (!byLayer[name]) { byLayer[name] = []; order.push(name); }
                byLayer[name].push({ attributes: feature.get('tmAttributes') || {}, geometry: feature.getGeometry() });
            }, { layerFilter: function (l) { return l === vectorLayer; }, hitTolerance: 3 });
            renderIdentify(order.map(function (n) { return { layer: n, features: byLayer[n] }; }));
            return;
        }

        $('tm-identify').appendChild(h('p', { className: 'muted', text: 'Identifying…' }));

        if (adapter.identify) {
            adapter.identify(state.iface, state.caps, click)
                .then(function (groups) { renderIdentify(groups); })
                .catch(function (err) { renderIdentify(null, 'The request failed: ' + (err && err.message || err)); });
            return;
        }

        var request = adapter.identifyRequest(state.iface, state.caps, click);
        send({ iface: state.iface.title, op: 'identify', url: request.url, as: 'text' })
            .then(function (r) {
                if (!r.ok) {
                    renderIdentify(null, r.text || ('HTTP ' + r.status));
                    return;
                }
                var groups = null;
                try { groups = adapter.identifyResults(r); } catch (e) { groups = null; }
                renderIdentify(groups, r.text);
            })
            .catch(function (err) {
                renderIdentify(null, 'The request failed: ' + (err && err.message || err));
            });
    });

    // ==================================================================== interface / crs switch

    function fillCrs(crsList) {
        var select = $('tm-crs');
        select.textContent = '';
        crsList.filter(usable).forEach(function (c) {
            select.appendChild(h('option', { value: c, text: c + (cfg.epsg > 0 && c === 'EPSG:' + cfg.epsg ? ' (service)' : '') }));
        });
        select.value = state.crs;
    }

    function fillInfoFormats(formats) {
        var row = $('tm-infoformat-row');
        var select = $('tm-infoformat');
        var previous = select.value;
        select.textContent = '';
        (formats || []).forEach(function (f) { select.appendChild(h('option', { value: f, text: f })); });
        row.hidden = !formats || !formats.length;
        if (formats && formats.length) {
            select.value = formats.indexOf(previous) >= 0 ? previous
                : formats.indexOf('application/json') >= 0 ? 'application/json'
                    : formats[0];
        }
    }

    function startExtent(caps, crs) {
        try {
            var stored = JSON.parse(localStorage.getItem(storeKey) || 'null');
            var e = stored && transformedExtent(stored, crs);
            if (e) { return e; }
        } catch (err) { }
        return transformedExtent(caps.initialExtent, crs) || transformedExtent(caps.fullExtent, crs) || worldExtent(crs);
    }

    function selectInterface(key) {
        var iface = cfg.interfaces.filter(function (i) { return i.key === key; })[0];
        if (!iface) { return; }

        document.querySelectorAll('.tm-seg button').forEach(function (b) {
            var active = b.getAttribute('data-iface') === key;
            b.classList.toggle('active', active);
            b.setAttribute('aria-selected', active ? 'true' : 'false');
        });
        try { history.replaceState(null, '', '#' + key); } catch (e) { }

        // keep what the user sees: the extent in the current projection
        var keep = state.crs ? { crs: state.crs, extent: currentExtent() } : null;

        if (state.controller) { state.controller.abort(); }
        if (state.vectorController) { state.vectorController.abort(); }
        state.iface = iface;
        state.caps = null;
        imageLayer.setSource(null);
        tileLayer.setSource(null);
        vectorSource.clear();
        resetNotice();
        $('tm-style-row').hidden = true;
        $('tm-query').hidden = true;
        $('tm-limit-field').hidden = true;
        $('tm-coords').textContent = '';
        highlightSource.clear();
        clickSource.clear();
        showMessage(null);
        $('tm-layers').textContent = '';
        $('tm-layers').appendChild(h('p', { className: 'muted', text: 'Loading capabilities…' }));
        $('tm-identify').textContent = '';
        $('tm-identify').appendChild(h('p', { className: 'muted', text: 'Click on the map to identify features.' }));

        var serviceCrs = cfg.epsg > 0 ? 'EPSG:' + cfg.epsg : 'EPSG:4326';

        adapters[key].capabilities(iface)
            .then(function (caps) {
                if (state.iface !== iface) { return; }   // switched again meanwhile
                state.caps = caps;

                var usableCrs = caps.crsList.filter(usable);
                var crs = keep && usableCrs.indexOf(keep.crs) >= 0 ? keep.crs
                    : usableCrs.indexOf(serviceCrs) >= 0 ? serviceCrs
                        : usableCrs[0] || 'EPSG:4326';

                if (keep) {
                    if (crs !== map.getView().getProjection().getCode()) {
                        setView(crs, transformedExtent(keep, crs));
                    }
                } else {
                    setView(crs, startExtent(caps, crs));
                }
                state.crs = crs;

                fillCrs(caps.crsList);
                fillInfoFormats(caps.infoFormats);
                $('tm-query').hidden = !(adapters[key].queryExtent && kind() === 'image');
                $('tm-limit-field').hidden = $('tm-query').hidden && kind() !== 'vector';
                renderLayers();
                refreshMap();
            })
            .catch(function (err) {
                if (state.iface !== iface) { return; }
                $('tm-layers').textContent = '';
                $('tm-layers').appendChild(h('p', { className: 'tm-error', text: (err && err.message) || String(err) }));
                showMessage('Could not read the capabilities of ' + iface.title + ': see the request log.');
            });
    }

    document.querySelectorAll('.tm-seg button').forEach(function (b) {
        b.addEventListener('click', function () { selectInterface(b.getAttribute('data-iface')); });
    });

    $('tm-crs').addEventListener('change', function () {
        var crs = this.value;
        if (!state.caps || crs === state.crs) { return; }
        var extent = transformedExtent({ crs: state.crs, extent: currentExtent() }, crs);
        setView(crs, extent);
        refreshMap();
    });

    $('tm-full').addEventListener('click', function () {
        if (state.caps) { fit(transformedExtent(state.caps.fullExtent, state.crs) || worldExtent(state.crs)); }
    });

    $('tm-initial').addEventListener('click', function () {
        if (state.caps) { fit(transformedExtent(state.caps.initialExtent, state.crs) || transformedExtent(state.caps.fullExtent, state.crs) || worldExtent(state.crs)); }
    });

    $('tm-anonymous').addEventListener('change', function () {
        // same view, but every request again: the capabilities may differ for anonymous users
        if (state.iface) { selectInterface(state.iface.key); }
    });

    $('tm-infoformat').addEventListener('change', function () { /* used with the next click */ });

    $('tm-style').addEventListener('change', function () {
        if (!state.caps) { return; }
        state.caps.style = this.value;
        refreshMap();
    });

    $('tm-limit').addEventListener('change', function () {
        if (state.caps && kind() === 'vector') { loadVector(); }
    });

    $('tm-asadvertised').addEventListener('change', function () {
        if (state.iface) { selectInterface(state.iface.key); }
    });

    $('tm-query').addEventListener('click', function () {
        if (!state.caps || !adapter().queryExtent) { return; }
        showSide('identify');
        $('tm-identify').textContent = '';
        $('tm-identify').appendChild(h('p', { className: 'muted', text: 'Querying the features in the extent…' }));
        clickSource.clear();
        adapter().queryExtent(state.iface, state.caps, currentExtent(), state.crs, queryLimit())
            .then(function (groups) { renderIdentify(groups); })
            .catch(function (err) { renderIdentify(null, 'The request failed: ' + (err && err.message || err)); });
    });


    // ==================================================================== start

    renderLog();
    var hash = (location.hash || '').substring(1);
    var first = cfg.interfaces.filter(function (i) { return i.key === hash; })[0] || cfg.interfaces[0];
    selectInterface(first.key);
})();
