/*
 * PROTOTYPE — throwaway. gView.Server v9 admin UI: three variants of the same pages,
 * switchable via ?variant=A|B|C and ?page=home|browse|interfaces|rest|manage|security|login.
 *
 * The Manage pages run the REAL, UNCHANGED ../wwwroot/js/manage.js against a stubbed $.ajax,
 * so the markup the variants style is exactly what production renders. Only CSS differs there.
 * Razor pages (home, browse, rest, ...) are hand-written here as "what the new .cshtml would emit".
 */
(function () {
    const params = new URLSearchParams(location.search);
    const VARIANTS = {
        A: 'Console — dark sidebar, dense tables',
        B: 'Workspace — top navigation, cards',
        C: 'Explorer — icon rail, tree pane, split view'
    };
    const PAGES = ['home', 'browse', 'interfaces', 'rest', 'manage', 'security', 'login'];
    const variant = VARIANTS[params.get('variant')] ? params.get('variant') : 'A';
    const page = PAGES.includes(params.get('page')) ? params.get('page') : 'home';
    const folder = params.get('folder') || '';

    const href = (p, extra) => '?variant=' + variant + '&page=' + p + (extra ? '&' + extra : '');
    window.protoHref = href;

    // ------------------------------------------------------------------ icons (Lucide subset, MIT)
    const ICONS = {
        home: '<path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>',
        layers: '<path d="M12.83 2.18a2 2 0 0 0-1.66 0L2.6 6.08a1 1 0 0 0 0 1.83l8.58 3.91a2 2 0 0 0 1.66 0l8.58-3.9a1 1 0 0 0 0-1.83Z"/><path d="m22 17.65-9.17 4.16a2 2 0 0 1-1.66 0L2 17.65"/><path d="m22 12.65-9.17 4.16a2 2 0 0 1-1.66 0L2 12.65"/>',
        code: '<polyline points="16 18 22 12 16 6"/><polyline points="8 6 2 12 8 18"/>',
        shield: '<path d="M20 13c0 5-3.5 7.5-7.66 8.95a1 1 0 0 1-.67-.01C7.5 20.5 4 18 4 13V6a1 1 0 0 1 1-1c2 0 4.5-1.2 6.24-2.72a1.17 1.17 0 0 1 1.52 0C14.51 3.81 17 5 19 5a1 1 0 0 1 1 1z"/>',
        server: '<rect width="20" height="8" x="2" y="2" rx="2"/><rect width="20" height="8" x="2" y="14" rx="2"/><line x1="6" x2="6.01" y1="6" y2="6"/><line x1="6" x2="6.01" y1="18" y2="18"/>',
        log: '<path d="M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z"/><path d="M14 2v4a2 2 0 0 0 2 2h4"/><path d="M10 9H8"/><path d="M16 13H8"/><path d="M16 17H8"/>',
        lock: '<rect width="18" height="11" x="3" y="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/>',
        unlock: '<rect width="18" height="11" x="3" y="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 9.9-1"/>',
        play: '<polygon points="6 3 20 12 6 21 6 3"/>',
        stop: '<rect width="14" height="14" x="5" y="5" rx="2"/>',
        settings: '<line x1="21" x2="14" y1="4" y2="4"/><line x1="10" x2="3" y1="4" y2="4"/><line x1="21" x2="12" y1="12" y2="12"/><line x1="8" x2="3" y1="12" y2="12"/><line x1="21" x2="16" y1="20" y2="20"/><line x1="12" x2="3" y1="20" y2="20"/><line x1="14" x2="14" y1="2" y2="6"/><line x1="8" x2="8" y1="10" y2="14"/><line x1="16" x2="16" y1="18" y2="22"/>',
        refresh: '<path d="M21 12a9 9 0 1 1-9-9c2.52 0 4.93 1 6.74 2.74L21 8"/><path d="M21 3v5h-5"/>',
        network: '<circle cx="18" cy="5" r="3"/><circle cx="6" cy="12" r="3"/><circle cx="18" cy="19" r="3"/><line x1="8.59" x2="15.42" y1="13.51" y2="17.49"/><line x1="15.41" x2="8.59" y1="6.51" y2="10.49"/>',
        upload: '<path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" x2="12" y1="3" y2="15"/>',
        folder: '<path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"/>',
        folderplus: '<path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"/><line x1="12" x2="12" y1="10" y2="16"/><line x1="9" x2="15" y1="13" y2="13"/>',
        map: '<polygon points="3 6 9 3 15 6 21 3 21 18 15 21 9 18 3 21"/><line x1="9" x2="9" y1="3" y2="18"/><line x1="15" x2="15" y1="6" y2="21"/>',
        login: '<path d="M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4"/><polyline points="10 17 15 12 10 7"/><line x1="15" x2="3" y1="12" y2="12"/>',
        logout: '<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" x2="9" y1="12" y2="12"/>',
        sun: '<circle cx="12" cy="12" r="4"/><path d="M12 2v2"/><path d="M12 20v2"/><path d="m4.93 4.93 1.41 1.41"/><path d="m17.66 17.66 1.41 1.41"/><path d="M2 12h2"/><path d="M20 12h2"/><path d="m6.34 17.66-1.41 1.41"/><path d="m19.07 4.93-1.41 1.41"/>',
        moon: '<path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z"/>',
        monitor: '<rect width="20" height="14" x="2" y="3" rx="2"/><line x1="8" x2="16" y1="21" y2="21"/><line x1="12" x2="12" y1="17" y2="21"/>',
        search: '<circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/>',
        trash: '<path d="M3 6h18"/><path d="M19 6v14c0 1-1 2-2 2H7c-1 0-2-1-2-2V6"/><path d="M8 6V4c0-1 1-2 2-2h4c1 0 2 1 2 2v2"/>',
        info: '<circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/>',
        chevron: '<path d="m9 18 6-6-6-6"/>',
        up: '<path d="m18 15-6-6-6 6"/>',
        key: '<circle cx="7.5" cy="15.5" r="5.5"/><path d="m21 2-9.6 9.6"/><path d="m15.5 7.5 3 3L22 7l-3-3"/>',
        users: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/>',
        plug: '<path d="M12 22v-5"/><path d="M9 8V2"/><path d="M15 8V2"/><path d="M18 8v5a4 4 0 0 1-4 4h-4a4 4 0 0 1-4-4V8Z"/>',
        external: '<path d="M15 3h6v6"/><path d="M10 14 21 3"/><path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6"/>',
        x: '<path d="M18 6 6 18"/><path d="m6 6 12 12"/>'
    };
    const svg = (body, stroke) => '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="' + stroke +
        '" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">' + body + '</svg>';
    window.ico = (name, cls) => svg(ICONS[name], 'currentColor').replace('<svg ', '<svg class="ico ' + (cls || '') + '" aria-hidden="true" ');

    // mask-image vars so CSS can colour icons that manage.js creates (.icon.log, .icon.security1, ...)
    const icoCss = Object.keys(ICONS).map(k =>
        '--ico-' + k + ':url("data:image/svg+xml,' + encodeURIComponent(svg(ICONS[k], 'black')) + '");').join('');
    const st = document.createElement('style');
    st.textContent = ':root{' + icoCss + '}';
    document.head.appendChild(st);

    // ------------------------------------------------------------------ theme: light / dark / system
    const THEME_KEY = 'gview-server-theme';
    let themePref = 'system';
    try { themePref = localStorage.getItem(THEME_KEY) || 'system'; } catch (e) { }
    const mq = window.matchMedia('(prefers-color-scheme: dark)');
    const applyTheme = () => {
        const eff = themePref === 'system' ? (mq.matches ? 'dark' : 'light') : themePref;
        document.documentElement.setAttribute('data-theme', eff);
        document.documentElement.setAttribute('data-theme-pref', themePref);
        document.querySelectorAll('[data-theme-set]').forEach(b =>
            b.classList.toggle('active', b.getAttribute('data-theme-set') === themePref));
    };
    mq.addEventListener('change', applyTheme);
    window.setTheme = (t) => {
        themePref = t;
        try { localStorage.setItem(THEME_KEY, t); } catch (e) { }
        applyTheme();
    };
    applyTheme();
    const themeSwitch = () =>
        '<div class="theme-switch" role="group" aria-label="Color mode">' +
        ['light', 'system', 'dark'].map(t => '<button type="button" data-theme-set="' + t + '" title="' + t + '" onclick="setTheme(\'' + t + '\')">' +
            ico(t === 'light' ? 'sun' : t === 'dark' ? 'moon' : 'monitor') + '</button>').join('') + '</div>';

    // ------------------------------------------------------------------ fake data
    const FOLDERS = [
        { name: 'basemaps', hasSecurity: true },
        { name: 'utilities', hasSecurity: false },
        { name: 'networks', hasSecurity: false }
    ];
    const SERVICES = {
        '': [
            { name: 'osm_grey', status: 'running', runningSince: '2026-11-18 07:12:44' },
            { name: 'orthophoto_2025', status: 'running', runningSince: '2026-11-18 07:12:51', hasSecurity: true },
            { name: 'cadastre', status: 'idle', hasErrors: true },
            { name: 'admin_boundaries', status: 'stopped' },
            { name: 'zoning_plan', status: 'running', runningSince: '2026-11-17 22:40:03' }
        ],
        basemaps: [
            { name: 'basemap_light', status: 'running', runningSince: '2026-11-18 06:58:10', hasSecurity: true },
            { name: 'basemap_dark', status: 'idle', hasSecurity: true }
        ],
        utilities: [
            { name: 'water_supply', status: 'running', runningSince: '2026-11-18 07:01:00', hasNetwork: true },
            { name: 'sewer', status: 'running', runningSince: '2026-11-18 07:01:04', hasNetwork: true, hasErrors: true },
            { name: 'street_lights', status: 'stopped' }
        ],
        networks: [
            { name: 'power_grid', status: 'running', runningSince: '2026-11-16 18:23:31', hasNetwork: true, hasSecurity: true }
        ]
    };
    Object.keys(SERVICES).forEach(f => SERVICES[f].forEach(s => { s.folder = f; }));
    const findService = (id) => {
        const parts = id.split('/');
        const f = parts.length > 1 ? parts[0] : '', n = parts[parts.length - 1];
        return (SERVICES[f] || []).find(s => s.name === n);
    };
    const secResult = () => ({
        allTypes: ['map', 'query', 'edit', '_all', '_wms', '_wfs', '_wmts', '_ogcapi', '_geoservices'],
        accessRules: [
            { username: '_everyone', servicetypes: ['map', 'query', '_all'] },
            { username: 'editor', servicetypes: ['map', 'query', 'edit', '_wms', '_geoservices'] }
        ],
        allUsers: ['editor', 'viewer', 'gisportal', 'token-fieldapp~9f2c71ab'],
        anonymousUsername: '_everyone'
    });
    const qs = (url) => new URLSearchParams(url.split('?')[1] || '');

    const fakeApi = function (url, type, data) {
        const path = url.split('?')[0].replace(/^.*\/manage\//, '/manage/');
        const q = qs(url);
        switch (path) {
            case '/manage/folders': return { folders: FOLDERS };
            case '/manage/services': return { services: SERVICES[q.get('folder') || ''] || [] };
            case '/manage/serviceerrorlogs': return {
                errors: [
                    '2026-11-18 07:44:02 MapRequest: Layer "parcels" — connection timeout after 30s\nat gView.DataSources.PostGIS.PostGISFeatureClass.GetFeatures(...)',
                    '2026-11-18 06:10:17 Query: invalid where clause "OBJECTID = "'
                ], ticks: 638672
            };
            case '/manage/servicesecurity':
                if (type === 'post') { const s = findService(q.get('service')); s.hasSecurity = true; return { success: true, service: s }; }
                return secResult();
            case '/manage/foldersecurity':
                if (type === 'post') return { success: true, folder: { hasSecurity: true } };
                return secResult();
            case '/manage/setservicestatus': {
                const s = findService(q.get('service'));
                if (q.get('status') !== 'refresh') s.status = q.get('status');
                s.runningSince = s.status === 'running' ? '2026-11-18 09:31:07' : null;
                return { success: true, service: s };
            }
            case '/manage/servicenetwork':
            case '/manage/servicenetworkbuild': return {
                directory: 'D:\\gview\\networks\\' + q.get('service').split('/').pop(),
                built: true, created: '2026-11-12 14:03:55',
                nodeCount: 184233, edgeCount: 201877, aliasCount: 312, shardCount: 16, rules: 4,
                job: {
                    started: '2026-11-12 14:01:10', finished: '2026-11-12 14:03:55', running: false, succeeded: true,
                    log: ['reading layers: pipes, valves, hydrants', 'snapping endpoints (tolerance 0.01 m)', '184233 nodes, 201877 edges', 'writing 16 shards', 'done in 165 s']
                },
                reports: ['validate-2026-11-12.html', 'build-2026-11-12.log']
            };
            case '/manage/servicemetadata':
                if (type === 'post') return { success: true };
                return {
                    'OGC WMS': 'title: Cadastre\nabstract: Land parcels and buildings\nkeywords:\n  - cadastre\n  - parcels\n',
                    'GeoServices': 'description: Cadastre map service\ncopyrightText: (c) Survey Office\n'
                };
            case '/manage/tokenusers': return { users: ['editor', 'viewer', 'gisportal', 'token-fieldapp~9f2c71ab', 'token-kiosk~0be81d44'] };
            default: return { success: true };
        }
    };

    // stub monaco: the metadata dialog uses monaco.editor.create — a textarea is enough here
    window.monaco = {
        editor: {
            create: function (el) {
                const ta = document.createElement('textarea');
                ta.className = 'proto-monaco'; ta.spellcheck = false;
                el.appendChild(ta);
                return { getValue: () => ta.value, setValue: (v) => { ta.value = v || ''; } };
            }
        }
    };

    // ------------------------------------------------------------------ page content ("Razor output")
    const browseContent = () => {
        const svcs = SERVICES[folder] || [];
        let h = '<div class="page-head"><h1>Browse <span class="path">/' + (folder ? folder + '/' : '') + '</span></h1>' +
            '<label class="filter">' + ico('search') + '<input type="search" placeholder="Filter services…" oninput="protoFilter(this.value)"></label></div>';
        h += '<ul class="browseservices">';
        if (folder) h += '<li class="back"><a href="' + href('browse') + '">..</a></li>';
        if (!folder) FOLDERS.forEach(f => {
            h += '<li class="folder"><a href="' + href('browse', 'folder=' + f.name) + '">' + f.name + '/</a></li>';
        });
        svcs.forEach(s => {
            h += '<li class="service" data-name="' + s.name + '"><a href="' + href('rest') + '">' + s.name + '</a>' +
                '<div class="button-service-interfaces" onclick="location.href=protoHref(\'interfaces\')">All Interfaces...</div>' +
                '<div class="button-delete-service" data-service="' + s.name + '" onclick="protoConfirmDelete(this)" title="Delete service"></div></li>';
        });
        h += '</ul>';
        h += '<div class="gview5-form-upload">' +
            '<form onsubmit="return false"><div class="gview5-form-upload-header gview5-form-header" onclick="this.parentNode.classList.toggle(\'open\')"><h4>Publish Service</h4></div>' +
            '<div class="gview5-form-upload-body gview5-form-body">' +
            '<div class="attribute"><div class="label">Service Name</div><input class="form-control" placeholder="Name..."></div>' +
            '<div class="attribute"><label class="custom-file-upload">' + ico('upload') + ' XML File Upload<input type="file" hidden></label></div>' +
            '<button type="submit" class="btn btn-primary">Publish Service</button></div></form>';
        if (!folder) h += '<form onsubmit="return false"><div class="gview5-form-newfolder-header gview5-form-header" onclick="this.parentNode.classList.toggle(\'open\')"><h4>Create Folder</h4></div>' +
            '<div class="gview5-form-newfolder-body gview5-form-body">' +
            '<div class="attribute"><div class="label">Folder Name</div><input class="form-control" placeholder="Name..."></div>' +
            '<button type="submit" class="btn btn-primary">Create New Folder</button></div></form>';
        h += '</div>';
        return h;
    };
    window.protoFilter = (v) => {
        v = v.toLowerCase();
        document.querySelectorAll('ul.browseservices li.service, ul.browseservices li.folder').forEach(li =>
            li.style.display = !v || li.textContent.toLowerCase().includes(v) ? '' : 'none');
    };
    // replaces bootbox.confirm — same modal markup as manage.js, so one modal style for everything
    window.protoConfirmDelete = (el) => {
        const name = el.getAttribute('data-service');
        const b = document.createElement('div');
        b.className = 'gview5-modal-blocker';
        b.innerHTML = '<div class="modal-dialog modal-small"><div class="modal-title">Delete service?</div>' +
            '<div class="modal-body">Do you want to delete service <b>' + (folder ? folder + '/' : '') + name + '</b> now? This cannot be undone.</div>' +
            '<div class="modal-footer"><button class="button-close">Better not</button><button class="button-ok danger">Yes, delete!</button></div></div>';
        b.querySelectorAll('button').forEach(x => x.onclick = () => b.remove());
        document.body.appendChild(b);
    };

    const interfacesContent = () => {
        const caps = [
            ['WMS', 'OGC Web Map Service', [['GetCapabilities', '1.3.0', 'GET', '/ogc/cadastre?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetCapabilities'], ['GetCapabilities', '1.1.1', 'GET', '/ogc/cadastre?SERVICE=WMS&VERSION=1.1.1&REQUEST=GetCapabilities']]],
            ['WFS', 'OGC Web Feature Service', [['GetCapabilities', '2.0.0', 'GET', '/ogc/cadastre?SERVICE=WFS&VERSION=2.0.0&REQUEST=GetCapabilities']]],
            ['OGCAPI', 'OGC API Features / Maps', [['Landing page', '1.0', 'GET', '/ogcapi/cadastre'], ['Collections', '1.0', 'GET', '/ogcapi/cadastre/collections']]],
            ['GEOSERVICES', 'GeoServices REST', [['MapServer', '10.81', 'GET', '/geoservices/rest/services/cadastre/MapServer'], ['FeatureServer', '10.81', 'GET', '/geoservices/rest/services/cadastre/FeatureServer']]]
        ];
        let h = '<div class="page-head"><h1>Interfaces <span class="path">cadastre</span></h1></div><table class="service-capabilities"><thead><tr><th>Name</th><th>Version</th><th>Method</th><th>Request</th></tr></thead><tbody>';
        caps.forEach(c => {
            h += '<tr class="interpreter"><td colspan="3" class="interpreter-name">' + c[1] + '</td><td class="interpreter-name tag">#' + c[0] + '</td></tr>';
            c[2].forEach(r => {
                h += '<tr class="interpreter-capabilities"><td class="cap-name">' + r[0] + '</td><td class="cap-version">' + r[1] + '</td><td class="cap-method">' + r[2] +
                    '</td><td class="cap-request"><a href="#">https://gis.example.org' + r[3] + '</a></td></tr>';
            });
        });
        return h + '</tbody></table>';
    };

    // mimics HtmlExtensions.ToYamlHtml output (property-name / property-value / yaml-comment)
    const sp = (n) => '&nbsp;'.repeat(n);
    const yaml = (obj, s, isArr) => {
        let h = '<div class="yaml-code">', first = true;
        Object.keys(obj).forEach(k => {
            const v = obj[k];
            const nl = typeof v === 'object' && !(Array.isArray(v) && (v.length === 0 || typeof v[0] !== 'object'));
            let pre = sp(s);
            if (isArr && first) { pre += '-&nbsp;'; s += 2; }
            h += '<div class="property-name' + (nl ? ' array' : '') + '">' + pre + k + ':&nbsp;</div><div class="property-value">';
            if (Array.isArray(v)) {
                if (!v.length) h += '[]';
                else if (typeof v[0] !== 'object') h += '[' + v.join(', ') + ']';
                else v.forEach(x => { h += yaml(x, s + 2, true); });
            } else if (v && typeof v === 'object') h += yaml(v, s + 2);
            else if (typeof v === 'string' && v.indexOf('link:') === 0) h += '<a href="#">' + v.substr(5) + '</a>';
            else h += v;
            h += '</div><br/>';
            first = false;
        });
        return h + '</div>';
    };
    const restContent = () => {
        const crumbs = [['services', ''], ['cadastre (MapServer)', '']];
        let h = '<nav class="breadcrumb"><a href="#">rest</a>' + crumbs.map(c => ico('chevron') + '<a href="#">' + c[0] + '</a>').join('') + '</nav>';
        h += '<div class="html-body"><h3>MapService (YAML):</h3>' +
            '<div class="service-methods"><a href="#">Export Map</a><a href="#">Identify</a><a href="#">Find</a><a href="#">Legend</a><a href="#">Layers</a><a href="#" target="_blank">OGC API</a></div>' +
            '<div class="code-block">' + yaml({
                currentVersion: 10.81,
                serviceDescription: 'Cadastre: parcels, buildings and addresses',
                mapName: 'cadastre',
                capabilities: 'Map,Query',
                spatialReference: { wkid: 25833, latestWkid: 25833 },
                singleFusedMapCache: false,
                fullExtent: { xmin: 380000.0, ymin: 5800000.0, xmax: 420000.0, ymax: 5840000.0 },
                layers: [
                    { id: 0, name: 'link:Addresses', parentLayerId: -1, defaultVisibility: true, subLayerIds: [] },
                    { id: 1, name: 'link:Buildings', parentLayerId: -1, defaultVisibility: true, subLayerIds: [] },
                    { id: 2, name: 'link:Parcels', parentLayerId: -1, defaultVisibility: true, subLayerIds: [] }
                ],
                supportedImageFormatTypes: 'PNG32,PNG24,JPG',
                units: 'esriMeters'
            }, 0) + '</div>' +
            '<form onsubmit="return false"><table>' +
            '<tr><td>bbox:</td><td class="input"><input name="bbox" value="380000,5800000,420000,5840000"></td></tr>' +
            '<tr><td>size:</td><td class="input"><input name="size" value="800,600"></td></tr>' +
            '<tr><td>format:</td><td class="input"><select name="format"><option>png</option><option>jpg</option></select></td></tr>' +
            '<tr><td>f:</td><td class="input"><select name="f"><option>pjson</option><option>image</option></select></td></tr>' +
            '<tr><td></td><td><button>Submit</button></td></tr></table></form>' +
            '</div>';
        return h;
    };
    const loginContent = () =>
        '<div class="gview-login-panel"><div class="title">' + ico('server', 'login-logo') + '<div><h3>Login (Manager)</h3><div>gView Server</div></div></div>' +
        '<div class="error-message">Unknown username or password</div>' +
        '<form onsubmit="return false"><div class="attribute"><div class="label">Username</div><input class="form-control" placeholder="Username" autofocus></div>' +
        '<div class="attribute"><div class="label">Password</div><input type="password" class="form-control" placeholder="Password"></div>' +
        '<button class="btn btn-primary">Login</button></form></div>';
    const manageContent = (sub) =>
        '<div class="page-head"><h1>Manage</h1></div>' +
        '<nav class="manage-tabs"><a class="' + (sub === 'manage' ? 'active' : '') + '" href="' + href('manage') + '">' + ico('server') + 'Services</a>' +
        '<a class="' + (sub === 'security' ? 'active' : '') + '" href="' + href('security') + '">' + ico('users') + 'Security</a></nav>' +
        '<div class="gview-manage-body"></div>';

    // home differs per variant (structure is the point there)
    const SYS = { render: 'Skia', encode: 'Skia', proj: 'Proj4Net', version: '9.26.4701' };
    const homeA = () =>
        '<div class="page-head"><h1>Overview</h1><span class="muted">gView Server ' + SYS.version + '</span></div>' +
        '<div class="kpis"><div class="kpi"><span class="kpi-label">Services</span><span class="kpi-value">11</span><span class="kpi-sub"><i class="dot running"></i>7 running · <i class="dot idle"></i>2 idle · <i class="dot stopped"></i>2 stopped</span></div>' +
        '<div class="kpi"><span class="kpi-label">Folders</span><span class="kpi-value">3</span><span class="kpi-sub">1 secured</span></div>' +
        '<div class="kpi warn"><span class="kpi-label">Services with errors</span><span class="kpi-value">2</span><span class="kpi-sub">cadastre, utilities/sewer</span></div>' +
        '<div class="kpi"><span class="kpi-label">Networks</span><span class="kpi-value">3</span><span class="kpi-sub">last build 2026-11-12</span></div></div>' +
        '<h2 class="section-title">Go to</h2><div class="quicklinks">' +
        '<a href="' + href('browse') + '">' + ico('layers') + '<b>Browse Services</b><span>Folders, services and all their interfaces</span></a>' +
        '<a href="' + href('rest') + '">' + ico('code') + '<b>GeoServices (REST)</b><span>/geoservices/rest/services</span></a>' +
        '<a href="' + href('manage') + '">' + ico('shield') + '<b>Manage</b><span>Start/stop, security, metadata, networks</span></a></div>' +
        '<h2 class="section-title">Engine</h2><table class="kv"><tr><td>Rendering</td><td>' + SYS.render + '</td></tr><tr><td>Encoding</td><td>' + SYS.encode + '</td></tr><tr><td>Projection</td><td>' + SYS.proj + '</td></tr><tr><td>Version</td><td>' + SYS.version + '</td></tr></table>';
    const homeB = () =>
        '<section class="hero"><div class="hero-badge">v9</div><h1>gView Server</h1><p>Map, feature and network services over GeoServices REST, OGC WMS/WFS/WMTS and OGC API.</p></section>' +
        '<div class="main-nav-panels">' +
        '<a class="main-nav-panel browse" href="' + href('browse') + '">' + ico('layers') + '<div class="text">Browse Services</div><p>Folders, services, interfaces, publishing</p><span class="go">Open ' + ico('chevron') + '</span></a>' +
        '<a class="main-nav-panel geoservices" href="' + href('rest') + '">' + ico('code') + '<div class="text">GeoServices (REST)</div><p>Explore the REST directory in the browser</p><span class="go">Open ' + ico('chevron') + '</span></a>' +
        '<a class="main-nav-panel manage" href="' + href('manage') + '">' + ico('shield') + '<div class="text">Manage</div><p>Services, security, clients and tokens</p><span class="go">Open ' + ico('chevron') + '</span></a></div>' +
        '<div class="sysline"><span>' + ico('server') + 'Rendering <b>' + SYS.render + '</b></span><span>Encoding <b>' + SYS.encode + '</b></span><span>Projection <b>' + SYS.proj + '</b></span><span>Version <b>' + SYS.version + '</b></span></div>';
    const homeC = () =>
        '<div class="page-head"><h1>' + ico('server') + 'gView Server <span class="path">' + SYS.version + '</span></h1></div>' +
        '<div class="endpoints"><div class="ep-head">Endpoints</div>' +
        '<a href="' + href('browse') + '"><code>/BrowseServices</code><span>Browse services and interfaces</span></a>' +
        '<a href="' + href('rest') + '"><code>/geoservices/rest/services</code><span>GeoServices REST directory</span></a>' +
        '<a href="#"><code>/ogcapi</code><span>OGC API Features / Maps</span></a>' +
        '<a href="#"><code>/ogc/{service}</code><span>WMS · WFS · WMTS</span></a>' +
        '<a href="' + href('manage') + '"><code>/Manage</code><span>Administration</span></a></div>' +
        '<div class="endpoints"><div class="ep-head">Engine</div><div class="ep-row"><code>rendering</code><span>' + SYS.render + '</span></div><div class="ep-row"><code>encoding</code><span>' + SYS.encode +
        '</span></div><div class="ep-row"><code>projection</code><span>' + SYS.proj + '</span></div></div>';

    const content = () => {
        switch (page) {
            case 'browse': return browseContent();
            case 'interfaces': return interfacesContent();
            case 'rest': return restContent();
            case 'manage': case 'security': return manageContent(page);
            case 'login': return loginContent();
            default: return variant === 'A' ? homeA() : variant === 'B' ? homeB() : homeC();
        }
    };

    // ------------------------------------------------------------------ shells — one per variant, no shared layout
    const NAV = [['home', 'home', 'Home'], ['browse', 'layers', 'Browse Services'], ['rest', 'code', 'GeoServices (REST)'], ['manage', 'shield', 'Manage']];
    const isActive = (p) => p === page || (p === 'manage' && page === 'security') || (p === 'browse' && page === 'interfaces');
    const titleOf = { home: 'Overview', browse: 'Browse Services', interfaces: 'Interfaces', rest: 'GeoServices (REST)', manage: 'Manage · Services', security: 'Manage · Security', login: 'Login' };

    const shellA = () =>
        '<div class="a-app"><aside class="a-side"><a class="a-brand" href="' + href('home') + '">' + ico('map') + '<span>gView<b>Server</b></span><em>v9</em></a>' +
        '<nav>' + NAV.map(n => '<a class="' + (isActive(n[0]) ? 'active' : '') + '" href="' + href(n[0]) + '">' + ico(n[1]) + '<span>' + n[2] + '</span></a>').join('') + '</nav>' +
        '<div class="a-side-bottom"><div class="a-user">' + ico('users') + '<span>admin</span></div><a href="' + href('login') + '">' + ico('logout') + '<span>Logout</span></a>' +
        '<a href="#">' + ico('info') + '<span>About</span></a></div></aside>' +
        '<div class="a-main"><header class="a-top"><div class="a-crumb">gView Server ' + ico('chevron') + '<b>' + titleOf[page] + '</b></div>' + themeSwitch() + '</header>' +
        '<main class="a-content">' + content() + '</main>' +
        '<footer class="a-foot"><span>Rendering ' + SYS.render + '</span><span>Encoding ' + SYS.encode + '</span><span>Proj ' + SYS.proj + '</span><span class="grow"></span><span>v' + SYS.version + '</span></footer></div></div>';

    const shellB = () =>
        '<div class="b-app"><header class="b-top"><a class="b-brand" href="' + href('home') + '"><span class="b-mark">g</span>gView Server<em>v9</em></a>' +
        '<nav class="b-nav">' + NAV.slice(1).map(n => '<a class="' + (isActive(n[0]) ? 'active' : '') + '" href="' + href(n[0]) + '">' + n[2] + '</a>').join('') + '</nav>' +
        '<div class="b-right">' + themeSwitch() + '<div class="b-avatar" title="admin">AD</div><a class="b-icon" href="' + href('login') + '" title="Logout">' + ico('logout') + '</a></div></header>' +
        '<main class="b-content b-page-' + page + '">' + content() + '</main>' +
        '<footer class="b-foot">gView Server ' + SYS.version + ' · <a href="#">About</a></footer></div>';

    const shellC = () => {
        const tree = '<div class="c-tree-head">Services</div><ul class="c-tree">' +
            '<li class="open">' + ico('folder') + '(root)<ul>' + SERVICES[''].map(s => '<li><i class="dot ' + s.status + '"></i>' + s.name + '</li>').join('') + '</ul></li>' +
            FOLDERS.map(f => '<li>' + ico('folder') + f.name + (f.hasSecurity ? ico('lock', 'mini') : '') + '<ul>' + SERVICES[f.name].map(s => '<li><i class="dot ' + s.status + '"></i>' + s.name + '</li>').join('') + '</ul></li>').join('') +
            '</ul>';
        return '<div class="c-app"><nav class="c-rail"><a class="c-logo" href="' + href('home') + '">gV</a>' +
            NAV.map(n => '<a class="' + (isActive(n[0]) ? 'active' : '') + '" href="' + href(n[0]) + '" title="' + n[2] + '">' + ico(n[1]) + '</a>').join('') +
            '<span class="grow"></span>' + themeSwitch() + '<a href="' + href('login') + '" title="Logout">' + ico('logout') + '</a></nav>' +
            '<aside class="c-pane">' + tree + '</aside>' +
            '<div class="c-main"><header class="c-tabs"><span class="c-tab active">' + titleOf[page] + '</span><span class="grow"></span><label class="c-search">' + ico('search') + '<input placeholder="Jump to service…  (Ctrl K)"></label></header>' +
            '<main class="c-content">' + content() + '</main>' +
            '<footer class="c-status"><span><i class="dot running"></i>online</span><span>' + SYS.render + '</span><span>' + SYS.proj + '</span><span class="grow"></span><span>admin</span><span>gView Server ' + SYS.version + '</span></footer></div></div>';
    };

    // ------------------------------------------------------------------ render
    document.getElementById('variant-css').href = 'variant-' + variant.toLowerCase() + '.css';
    document.body.className = 'variant-' + variant.toLowerCase() + ' page-' + page;
    document.getElementById('app').innerHTML = variant === 'A' ? shellA() : variant === 'B' ? shellB() : shellC();
    applyTheme();

    // floating switcher (prototype only)
    const keys = Object.keys(VARIANTS);
    const go = (v, p) => { location.search = '?variant=' + v + '&page=' + (p || page) + (folder ? '&folder=' + folder : ''); };
    const bar = document.createElement('div');
    bar.className = 'proto-bar';
    bar.innerHTML = '<button data-d="-1" title="previous variant (←)">‹</button><span><b>' + variant + '</b> ' + VARIANTS[variant] + '</span><button data-d="1" title="next variant (→)">›</button>' +
        '<select title="page">' + PAGES.map(p => '<option ' + (p === page ? 'selected' : '') + '>' + p + '</option>').join('') + '</select>';
    const step = (d) => go(keys[(keys.indexOf(variant) + d + keys.length) % keys.length]);
    bar.querySelectorAll('button').forEach(b => b.onclick = () => step(+b.getAttribute('data-d')));
    bar.querySelector('select').onchange = (e) => go(variant, e.target.value);
    document.body.appendChild(bar);
    document.addEventListener('keydown', (e) => {
        const t = e.target;
        if (t.matches('input, textarea, select, [contenteditable]')) return;
        if (e.key === 'ArrowLeft') step(-1);
        if (e.key === 'ArrowRight') step(1);
    });

    // ------------------------------------------------------------------ boot the real manage.js
    if (page === 'manage' || page === 'security') {
        $.ajax = function (opts) {
            setTimeout(function () {
                const res = fakeApi(opts.url, (opts.type || 'get').toLowerCase(), opts.data);
                opts.success(res);
            }, 180);
        };
        window.gview.manage.setRootUrl('');
        if (page === 'manage') window.gview.manage.pageServices(); else window.gview.manage.pageSecurity();
    }
    window.alert = (m) => console.log('[alert]', m);
    window.confirm = () => true;
})();
