/*
 * PROTOTYPE — throwaway. Three structurally different variants of gView Carto / Explorer v9.
 *   A "Console": dark header, flat ribbon with group labels, flush panels
 *   B "Rail":    dark header, icon rail with the ribbon groups, one-row toolbar for the active group
 *   C "Cards":   header + tools merged into one dark bar, full-bleed map, floating panels
 * Tools, icons, targets and sort orders are the real ones from gView.Carto.Plugins / DataExplorer.Plugins.
 */
(function () {
    const params = new URLSearchParams(location.search);
    const VARIANTS = { A: 'Console', B: 'Rail', C: 'Cards' };
    const PAGES = ['carto', 'explorer', 'home', 'login'];
    const variant = VARIANTS[params.get('variant')] ? params.get('variant') : 'A';
    const page = PAGES.includes(params.get('page')) ? params.get('page') : 'carto';
    const dialog = params.get('dialog') === '1';
    const go = (o) => {
        const p = new URLSearchParams(location.search);
        Object.entries(o).forEach(([k, v]) => v === null ? p.delete(k) : p.set(k, v));
        location.search = p.toString();
    };

    // ------------------------------------------------------------ theme (light / dark / system)
    const THEME_KEY = 'gview-webapps-theme';
    const getTheme = () => { try { return localStorage.getItem(THEME_KEY) || 'system'; } catch (e) { return 'system'; } };
    const applyTheme = () => {
        const t = getTheme();
        const dark = t === 'dark' || (t === 'system' && matchMedia('(prefers-color-scheme: dark)').matches);
        document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
        document.querySelectorAll('[data-theme-label]').forEach(e => e.innerHTML = themeIcon(t));
    };
    const cycleTheme = () => {
        const next = { system: 'light', light: 'dark', dark: 'system' }[getTheme()];
        try { localStorage.setItem(THEME_KEY, next); } catch (e) { }
        applyTheme();
    };
    matchMedia('(prefers-color-scheme: dark)').addEventListener('change', applyTheme);

    // ------------------------------------------------------------ Material Outlined icons (chrome only)
    const MI = {
        close: 'M19 6.41 17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z',
        up: 'M4 12l1.41 1.41L11 7.83V20h2V7.83l5.58 5.59L20 12l-8-8-8 8z',
        back: 'M20 11H7.83l5.59-5.59L12 4l-8 8 8 8 1.41-1.41L7.83 13H20v-2z',
        search: 'M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z',
        open: 'M19 19H5V5h7V3H5c-1.11 0-2 .9-2 2v14c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2v-7h-2v7zM14 3v2h3.59l-9.83 9.83 1.41 1.41L19 6.41V10h2V3h-7z',
        chevron: 'M10 6 8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z',
        expand: 'M16.59 8.59 12 13.17 7.41 8.59 6 10l6 6 6-6z',
        person: 'M12 6c1.1 0 2 .9 2 2s-.9 2-2 2-2-.9-2-2 .9-2 2-2m0 10c2.7 0 5.8 1.29 6 2H6c.23-.72 3.31-2 6-2m0-12C9.79 4 8 5.79 8 8s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm0 10c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z',
        logout: 'M17 7l-1.41 1.41L18.17 11H8v2h10.17l-2.58 2.58L17 17l5-5zM4 5h8V3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h8v-2H4V5z',
        moon: 'M9.37 5.51c-.18.64-.27 1.31-.27 1.99 0 4.08 3.32 7.4 7.4 7.4.68 0 1.35-.09 1.99-.27C17.45 17.19 14.93 19 12 19c-3.86 0-7-3.14-7-7 0-2.93 1.81-5.45 4.37-6.49zM12 3c-4.97 0-9 4.03-9 9s4.03 9 9 9 9-4.03 9-9c0-.46-.04-.92-.1-1.36-.98 1.37-2.58 2.26-4.4 2.26-2.98 0-5.4-2.42-5.4-5.4 0-1.81.89-3.42 2.26-4.4-.44-.06-.9-.1-1.36-.1z',
        sun: 'M12 9c1.65 0 3 1.35 3 3s-1.35 3-3 3-3-1.35-3-3 1.35-3 3-3m0-2c-2.76 0-5 2.24-5 5s2.24 5 5 5 5-2.24 5-5-2.24-5-5-5zM2 13h2c.55 0 1-.45 1-1s-.45-1-1-1H2c-.55 0-1 .45-1 1s.45 1 1 1zm18 0h2c.55 0 1-.45 1-1s-.45-1-1-1h-2c-.55 0-1 .45-1 1s.45 1 1 1zM11 2v2c0 .55.45 1 1 1s1-.45 1-1V2c0-.55-.45-1-1-1s-1 .45-1 1zm0 18v2c0 .55.45 1 1 1s1-.45 1-1v-2c0-.55-.45-1-1-1s-1 .45-1 1zM5.99 4.58c-.39-.39-1.03-.39-1.41 0-.39.39-.39 1.03 0 1.41l1.06 1.06c.39.39 1.03.39 1.41 0s.39-1.03 0-1.41L5.99 4.58zm12.37 12.37c-.39-.39-1.03-.39-1.41 0-.39.39-.39 1.03 0 1.41l1.06 1.06c.39.39 1.03.39 1.41 0 .39-.39.39-1.03 0-1.41l-1.06-1.06zm1.06-10.96c.39-.39.39-1.03 0-1.41-.39-.39-1.03-.39-1.41 0l-1.06 1.06c-.39.39-.39 1.03 0 1.41s1.03.39 1.41 0l1.06-1.06zM7.05 18.36c.39-.39.39-1.03 0-1.41-.39-.39-1.03-.39-1.41 0l-1.06 1.06c-.39.39-.39 1.03 0 1.41s1.03.39 1.41 0l1.06-1.06z',
        auto: 'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18V4c4.41 0 8 3.59 8 8s-3.59 8-8 8z',
        folder: 'M9.17 6l2 2H20v10H4V6h5.17M10 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z',
        map: 'M20.5 3l-.16.03L15 5.1 9 3 3.36 4.9c-.21.07-.36.25-.36.48V20.5c0 .28.22.5.5.5l.16-.03L9 18.9l6 2.1 5.64-1.9c.21-.07.36-.25.36-.48V3.5c0-.28-.22-.5-.5-.5zM10 5.47l4 1.4v11.66l-4-1.4V5.47zm-5 .99l3-1.01v11.7l-3 1.16V6.46zm14 11.08l-3 1.01V6.86l3-1.16v11.84z',
        layers: 'M11.99 18.54l-7.37-5.73L3 14.07l9 7 9-7-1.63-1.27zM12 16l7.36-5.73L21 9l-9-7-9 7 1.63 1.27L12 16zm0-11.47L17.74 9 12 13.47 6.26 9 12 4.53z',
        build: 'M22.61 18.99l-9.08-9.08c.93-2.34.45-5.1-1.44-7C9.79.61 6.21.4 3.66 2.26L7.5 6.11 6.08 7.52 2.25 3.69C.39 6.23.6 9.82 2.9 12.11c1.86 1.86 4.57 2.35 6.89 1.48l9.11 9.11c.39.39 1.02.39 1.41 0l2.3-2.3c.4-.38.4-1.01 0-1.41zm-3 1.6l-9.46-9.46c-.61.45-1.29.72-2 .82-1.36.2-2.79-.21-3.83-1.25C3.37 9.76 2.93 8.5 3 7.26l3.09 3.09 4.24-4.24-3.09-3.09c1.24-.07 2.49.37 3.44 1.31 1.08 1.08 1.49 2.57 1.24 3.96-.12.71-.42 1.37-.88 1.96l9.45 9.45-.88.89z',
        table: 'M20 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h15c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 2v3H5V5h15zm-5 14h-5v-9h5v9zM5 10h3v9H5v-9zm12 9v-9h3v9h-3z',
        help: 'M11 18h2v-2h-2v2zm1-16C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm0-14c-2.21 0-4 1.79-4 4h2c0-1.1.9-2 2-2s2 .9 2 2c0 2-3 1.75-3 5h2c0-2.25 3-2.5 3-5 0-2.21-1.79-4-4-4z',
        memory: 'M15 9H9v6h6V9zm-2 4h-2v-2h2v2zm8-2V9h-2V7c0-1.1-.9-2-2-2h-2V3h-2v2h-2V3H9v2H7c-1.1 0-2 .9-2 2v2H3v2h2v2H3v2h2v2c0 1.1.9 2 2 2h2v2h2v-2h2v2h2v-2h2c1.1 0 2-.9 2-2v-2h2v-2h-2v-2h2zm-4 6H7V7h10v10z',
        filter: 'M10 18h4v-2h-4v2zM3 6v2h18V6H3zm3 7h12v-2H6v2z',
        more: 'M12 8c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zm0 2c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 6c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z',
        add: 'M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z',
        copy: 'M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12V1zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm0 16H8V7h11v14z',
        home: 'M12 5.69l5 4.5V18h-2v-6H9v6H7v-7.81l5-4.5M12 3L2 12h3v8h6v-6h2v6h6v-8h3L12 3z',
        info: 'M11 7h2v2h-2zm0 4h2v6h-2zm1-9C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z',
        dock: 'M20 3H4c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm0 16H4v-5h16v5z'
    };
    const mi = (n, cls) => '<svg class="mi ' + (cls || '') + '" viewBox="0 0 24 24"><path d="' + MI[n] + '"/></svg>';
    const themeIcon = (t) => mi(t === 'light' ? 'sun' : t === 'dark' ? 'moon' : 'auto') + '<span class="theme-txt">' +
        ({ light: 'Hell', dark: 'Dunkel', system: 'System' })[t] + '</span>';

    // sketchpen sprite icons, exactly like IconService.FromString
    const sp = (name, size) => {
        const [set, cls] = name.split(':');
        return '<span class="sp sketchpen-icon-' + set + '-bg-light-' + (size || 24) + ' ' + cls + '"></span>';
    };
    const esc = s => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;');

    // ------------------------------------------------------------ data (real plugin tools)
    const CARTO_GROUPS = [
        { key: 'File', label: 'Datei', mi: 'folder', tools: [
            { n: 'Open Map', i: 'basic:folder-white', s: 1 }, { n: 'Save Map', i: 'basic:disk-white', s: 3 },
            { n: 'New Map', i: 'basic:edit-plus', s: 12 }, { n: 'Save As ...', i: 'basic:disks-white', s: 14 }, { n: 'Publish Map', i: 'basic:cloud-upload', s: 99 }] },
        { key: 'Map', label: 'Karte', mi: 'map', tools: [
            { n: 'Zoom Back', i: 'basic:round-left', s: 0 }, { n: 'TOC Settings', i: 'basic:edit-text', s: 8 }, { n: 'Map Settings', i: 'basic:settings', s: 9 },
            { n: 'Clear Selection', i: 'basic:trashcan-x', s: 80 }, { n: 'Map Datasets', i: 'basic:database', s: 98 },
            { n: 'Module Settings', i: 'basic:screw-wrench', s: 990 }, { n: 'Match Service', i: 'basic:globe', s: 999 }] },
        { key: 'SelectedTocItem', label: 'Layer: Gebäude', mi: 'layers', selected: true, tools: [
            { n: 'Add Data', i: 'basic:database-plus', s: 0 }, { n: 'Data Table', i: 'basic:table', s: 1 }, { n: 'Layer Settings', i: 'basic:settings', s: 9, dialog: true },
            { n: 'Zoom To', i: 'webgis:extent', s: 90 }, { n: 'Joins', i: 'basic:merge', s: 95 }, { n: 'Copy Layer', i: 'basic:copy', s: 98 }] },
        { key: 'Tools', label: 'Werkzeuge', mi: 'build', tools: [
            { n: 'Identify', i: 'webgis:identify', s: 0, active: true }, { n: 'Zoom In', i: 'webgis:zoom-in', s: 2 }, { n: 'Zoom Out', i: 'webgis:zoom-out', s: 3 },
            { n: 'Network', i: 'webgis:construct-edge-intersect', s: 10 }, { n: 'Network Search', i: 'webgis:height-profile', s: 11 }, { n: 'Measure', i: 'webgis:measure-polyline', s: 20 }] }
    ];
    const EXPLORER_GROUPS = [
        { key: 'General', label: 'Allgemein', mi: 'home', tools: [
            { n: 'Refresh', i: 'basic:refresh', s: 0 }, { n: 'Map folder...', i: 'basic:open-in-window', s: 1 }, { n: 'Tools', i: 'basic:package', s: 5 }] },
        { key: 'Create', label: 'Neu in grundlagen.fdb', mi: 'add', selected: true, tools: [
            { n: 'Feature Class', i: 'webgis:layers', s: 0 }, { n: 'Raster Class', i: 'webgis:tiles', s: 1 }, { n: 'Network', i: 'webgis:construct-edge-intersect', s: 2 }] },
        { key: 'Context', label: 'Auswahl (2)', mi: 'copy', context: true, tools: [
            { n: 'Copy', i: 'basic:copy', s: 0 }, { n: 'Paste', i: 'basic:cut', s: 1 }, { n: 'Delete', i: 'basic:trashcan', s: 2 },
            { n: 'Rename', i: 'basic:edit', s: 3 }, { n: 'Info', i: 'webgis:identify', s: 4 }, { n: 'Accessability', i: 'basic:key', s: 5 }] }
    ];

    const TOC = [
        { t: 'group', n: 'Basiskarte', open: true, kids: [
            { n: 'Orthofoto 2024', on: true, sw: 'raster' }, { n: 'Gemeindegrenzen', on: true, sw: 'line-dash' }] },
        { t: 'group', n: 'Verkehr', open: true, kids: [
            { n: 'Straßen', on: true, legend: [['Autobahn', '#e8892b', 4], ['Hauptstraße', '#f2c94c', 3], ['Nebenstraße', '#ffffff', 2]] },
            { n: 'Bahnlinien', on: false, sw: 'rail' }] },
        { n: 'Gebäude', on: true, sw: 'poly', selected: true },
        { n: 'Gewässer', on: true, sw: 'water' },
        { n: 'Grünflächen', on: true, sw: 'green' }
    ];
    const ROWS = [
        [10231, 'Wohnen', 4, '412,6', 1978], [10232, 'Wohnen', 3, '298,1', 1984], [10233, 'Gewerbe', 2, '1.204,9', 2003],
        [10234, 'Öffentlich', 3, '2.088,0', 1961], [10235, 'Wohnen', 5, '615,3', 1995], [10236, 'Nebengebäude', 1, '38,2', 2011],
        [10237, 'Wohnen', 2, '176,4', 1957], [10238, 'Gewerbe', 1, '842,7', 2018], [10239, 'Wohnen', 6, '903,5', 2020]];

    const EX_TREE = [
        { n: 'Dateisystem', ic: 'basic:monitor', open: true, kids: [
            { n: 'G:\\daten\\gis', ic: 'basic:folder', open: true, kids: [
                { n: 'grundlagen.fdb', ic: 'basic:database', selected: true },
                { n: 'orthofotos', ic: 'basic:folder' }, { n: 'tilecache', ic: 'basic:folder' }] }] },
        { n: 'Datenbanken', ic: 'basic:database', open: true, kids: [
            { n: 'PostGIS gis@localhost', ic: 'basic:database' }, { n: 'SQL Server GEODB', ic: 'basic:database' }] },
        { n: 'Web Services', ic: 'basic:globe', kids: [] },
        { n: 'Netzwerk', ic: 'basic:share', kids: [] }
    ];
    const EX_ITEMS = [
        ['webgis:layers', 'gebaeude', 'Polygon Feature Class', '48.211', 'EPSG:31256', true],
        ['webgis:layers', 'strassen', 'Polyline Feature Class', '12.904', 'EPSG:31256', true],
        ['webgis:layers', 'adressen', 'Point Feature Class', '96.330', 'EPSG:31256'],
        ['webgis:layers', 'gewaesser', 'Polygon Feature Class', '1.208', 'EPSG:31256'],
        ['webgis:tiles', 'dhm_1m', 'Raster Class', '–', 'EPSG:31256'],
        ['webgis:construct-edge-intersect', 'kanal_netz', 'Network', '8.417', 'EPSG:31256'],
        ['basic:table', 'eigentuemer', 'Table', '33.071', '–']];

    // ------------------------------------------------------------ shared fragments
    const mapSvg = () => `
<svg class="mapsvg" viewBox="0 0 1200 800" preserveAspectRatio="xMidYMid slice">
  <rect width="1200" height="800" fill="#eeebe4"/>
  <path d="M0 560 C180 520 260 610 420 580 S700 470 860 520 1100 640 1200 600 V800 H0Z" fill="#a9cbe8"/>
  <path d="M0 585 C180 545 260 635 420 605 S700 495 860 545 1100 665 1200 625" fill="none" stroke="#8fb8de" stroke-width="3"/>
  <path d="M120 80 h220 v140 h-220z M760 90 h300 v180 h-300z M520 640 h180 v120 h-180z" fill="#cfe3bf"/>
  <g stroke="#c9c2b5" stroke-width="15" fill="none" stroke-linecap="round">
    <path d="M-20 300 C300 280 600 330 1220 260"/><path d="M450 -20 C470 250 430 420 520 820"/>
    <path d="M820 -20 L760 420 L900 820"/><path d="M-20 140 L1220 120"/><path d="M200 -20 L260 820"/></g>
  <g stroke="#ffffff" stroke-width="10" fill="none" stroke-linecap="round">
    <path d="M-20 140 L1220 120"/><path d="M200 -20 L260 820"/></g>
  <g stroke="#f2c94c" stroke-width="11" fill="none" stroke-linecap="round">
    <path d="M450 -20 C470 250 430 420 520 820"/><path d="M820 -20 L760 420 L900 820"/></g>
  <path d="M-20 300 C300 280 600 330 1220 260" stroke="#e8892b" stroke-width="12" fill="none"/>
  <g fill="#d9a7a0" stroke="#b5837b" stroke-width="1.5">
    ${[[300, 170], [340, 175], [300, 215], [560, 170], [600, 180], [640, 165], [560, 360], [610, 370], [300, 360], [345, 380], [300, 420],
        [620, 440], [660, 450], [900, 330], [950, 340], [1000, 320], [900, 400], [960, 410], [1040, 390], [680, 200], [120, 330], [150, 400], [100, 420]]
        .map(([x, y], i) => `<rect x="${x}" y="${y}" width="${28 + (i % 3) * 6}" height="${22 + (i % 4) * 4}" transform="rotate(${(i % 5) - 2} ${x} ${y})"/>`).join('')}
  </g>
  <rect x="598" y="178" width="34" height="26" fill="#2563eb55" stroke="#2563eb" stroke-width="3"/>
  <rect x="955" y="338" width="40" height="30" fill="#2563eb55" stroke="#2563eb" stroke-width="3"/>
</svg>`;

    const swatch = (k) => {
        const m = {
            raster: '<span class="sw sw-raster"></span>', 'line-dash': '<span class="sw sw-dash"></span>', rail: '<span class="sw sw-rail"></span>',
            poly: '<span class="sw" style="background:#d9a7a0;border-color:#b5837b"></span>', water: '<span class="sw" style="background:#a9cbe8;border-color:#8fb8de"></span>',
            green: '<span class="sw" style="background:#cfe3bf;border-color:#a7c995"></span>'
        };
        return m[k] || '';
    };
    const chk = (on) => '<span class="chk ' + (on ? 'on' : '') + '"></span>';
    const toc = () => {
        const node = (it) => {
            if (it.t === 'group') return `<li><div class="tn"><span class="tw">${mi(it.open ? 'expand' : 'chevron')}</span>${chk(true)}<span class="tn-grp">${esc(it.n)}</span></div><ul>${it.kids.map(node).join('')}</ul></li>`;
            const legend = it.legend ? `<ul class="legend">${it.legend.map(([n, c, w]) => `<li><span class="lg-line" style="border-top:${w}px solid ${c}"></span>${n}</li>`).join('')}</ul>` : '';
            return `<li><div class="tn ${it.selected ? 'sel' : ''}"><span class="tw">${it.legend ? mi('expand') : ''}</span>${chk(it.on)}${swatch(it.sw)}<span>${esc(it.n)}</span></div>${legend}</li>`;
        };
        return `<ul class="tree toc-tree"><li><div class="tn root"><span class="tw">${mi('expand')}</span>${mi('map', 'tn-mi')}<b>Stadtplan Graz</b></div><ul>${TOC.map(node).join('')}</ul></li></ul>`;
    };
    const exTree = () => {
        const node = (it) => `<li><div class="tn ${it.selected ? 'sel' : ''}"><span class="tw">${it.kids ? mi(it.open ? 'expand' : 'chevron') : ''}</span>${sp(it.ic, 16)}<span>${esc(it.n)}</span></div>${it.open && it.kids ? '<ul>' + it.kids.map(node).join('') + '</ul>' : ''}</li>`;
        return `<ul class="tree">${EX_TREE.map(node).join('')}</ul>`;
    };
    const dataTable = () => `
<table class="grid"><thead><tr><th></th><th>OBJECTID</th><th>NUTZUNG</th><th>GESCHOSSE</th><th class="r">FLAECHE_M2</th><th class="r">BAUJAHR</th></tr></thead>
<tbody>${ROWS.map((r, i) => `<tr class="${i === 1 || i === 7 ? 'sel' : ''}"><td class="ic">${sp('webgis:identify', 16)}</td><td>${r[0]}</td><td>${r[1]}</td><td>${r[2]}</td><td class="r">${r[3]}</td><td class="r">${r[4]}</td></tr>`).join('')}</tbody></table>`;
    const exTable = () => `
<table class="grid"><thead><tr><th></th><th>Name</th><th>Typ</th><th class="r">Objekte</th><th>Bezugssystem</th></tr></thead>
<tbody>${EX_ITEMS.map(r => `<tr class="${r[5] ? 'ctx' : ''}"><td class="ic">${sp(r[0], 16)}</td><td>${r[1]}</td><td class="muted">${r[2]}</td><td class="r">${r[3]}</td><td class="muted">${r[4]}</td></tr>`).join('')}</tbody></table>`;

    const tabs = (items, active) => `<div class="tabs">${items.map((t, i) => `<span class="tab ${i === (active || 0) ? 'on' : ''}">${t}</span>`).join('')}</div>`;

    const statusCarto = () => `
<span class="st-item"><span class="st-dot"></span>Bereit</span>
<span class="st-item">1:12.500</span>
<span class="st-item">X 2.345,12  Y 214.765,30</span>
<span class="st-item grow"></span>
<span class="st-item">EPSG:31256 · MGI / Austria GK M34</span>
<span class="st-item">2 selektiert</span>`;
    const statusExplorer = () => `
<span class="st-item"><span class="st-dot"></span>Bereit</span>
<span class="st-item">7 Objekte · 2 ausgewählt</span>
<span class="st-item grow"></span>
<span class="st-item">G:\\daten\\gis\\grundlagen.fdb</span>`;

    const headerRight = () => `
<button class="hb" data-theme-btn title="Theme: Hell / Dunkel / System"><span data-theme-label>${themeIcon(getTheme())}</span></button>
<span class="hb user">${mi('person')}<span>admin</span></span>
<button class="hb" title="Logout">${mi('logout')}</button>`;
    const brand = (app) => `<a class="brand" href="?variant=${variant}&page=home"><span class="logo"><svg viewBox="0 0 24 24"><path d="M12 2 3 7v10l9 5 9-5V7z" fill="none" stroke="currentColor" stroke-width="2"/><path d="M12 7v10M7.5 9.5l9 5M16.5 9.5l-9 5" stroke="currentColor" stroke-width="1.5"/></svg></span><span class="brand-name">gView</span>${app ? '<span class="brand-app">' + app + '</span>' : ''}</a>`;
    const appHeader = (app, center) => `<header class="appbar">${brand(app)}<div class="appbar-center">${center || ''}</div><div class="appbar-right">${headerRight()}</div></header>`;

    // ------------------------------------------------------------ dialog (ModalDialogForm look, same for all variants)
    const dialogHtml = () => `
<div class="dlg-backdrop">
  <div class="dlg" role="dialog">
    <div class="dlg-head"><span class="dlg-title">Layer Settings – Gebäude</span><button class="ib" data-close>${mi('close')}</button></div>
    ${tabs(['Allgemein', 'Darstellung', 'Beschriftung', 'Felder'])}
    <div class="dlg-body">
      <label class="fld"><span>Name</span><input value="Gebäude"></label>
      <div class="row2">
        <label class="fld"><span>Minimaler Maßstab 1:</span><input value="0"></label>
        <label class="fld"><span>Maximaler Maßstab 1:</span><input value="25000"></label>
      </div>
      <label class="fld"><span>Renderer</span><div class="sel">Unique Value Renderer (NUTZUNG) ${mi('expand')}</div></label>
      <label class="fld"><span>Transparenz – 20 %</span><input type="range" value="20"></label>
      <label class="cb">${chk(true)} Selektierbar</label>
      <label class="cb">${chk(false)} In Legende ausblenden</label>
      <div class="alert info">${mi('info')}<span>Änderungen werden erst nach „Bestätigen“ in der Karte übernommen.</span></div>
    </div>
    <div class="dlg-foot"><button class="btn text" data-close>Abbrechen</button><button class="btn primary" data-close>Bestätigen</button></div>
  </div>
</div>`;

    // ------------------------------------------------------------ Variant A: Console
    const ribbonA = (groups) => `<div class="ribbon">${groups.map(g => {
        const large = g.tools.filter(t => t.s <= 10).slice(0, 4), small = g.tools.filter(t => t.s > 10);
        const chunks = []; for (let i = 0; i < small.length; i += 3) chunks.push(small.slice(i, i + 3));
        return `<div class="rb-group ${g.selected ? 'is-selected' : ''} ${g.context ? 'is-context' : ''}">
          <div class="rb-tools">${large.map(t => `<button class="rb-tool ${t.active ? 'active' : ''}" ${t.dialog ? 'data-dialog' : ''} title="${t.n}">${sp(t.i)}<span>${t.n}</span></button>`).join('')}
          ${chunks.map(c => `<div class="rb-chunk">${c.map(t => `<button class="rb-small" title="${t.n}">${sp(t.i, 16)}<span>${t.n}</span></button>`).join('')}</div>`).join('')}</div>
          <div class="rb-label">${g.label}</div></div>`;
    }).join('')}<div class="rb-spacer"></div><div class="rb-group"><div class="rb-tools"><button class="rb-tool" title="About">${sp('basic:help')}<span>About</span></button></div><div class="rb-label">Hilfe</div></div></div>`;

    const A = {
        carto: () => `<div class="shell">
  ${appHeader('Carto', '<span class="doc">Stadtplan Graz</span><span class="doc-sub">G:\\maps\\stadtplan.mxl</span>')}
  ${ribbonA(CARTO_GROUPS)}
  <div class="work carto-work">
    <aside class="pane toc-pane"><div class="pane-head">Inhaltsverzeichnis<span class="grow"></span><button class="ib sm">${mi('more')}</button></div><div class="pane-body">${toc()}</div></aside>
    <div class="splitter v"></div>
    <section class="main">
      <div class="pane map-pane">${tabs(['Karte', 'Layout'])}<div class="map">${mapSvg()}<div class="map-ctl"><button>+</button><button>−</button></div></div></div>
      <div class="splitter h"></div>
      <div class="pane data-pane">${tabs(['Gebäude (9)', 'Straßen (124)'])}<div class="pane-body">${dataTable()}</div></div>
    </section>
  </div>
  <footer class="statusbar">${statusCarto()}</footer></div>`,
        explorer: () => `<div class="shell">
  ${appHeader('Explorer', '<span class="doc">grundlagen.fdb</span>')}
  ${ribbonA(EXPLORER_GROUPS)}
  <div class="navbar"><button class="ib">${mi('back')}</button><button class="ib">${mi('up')}</button><div class="path">${sp('basic:database', 16)}<span>G:\\daten\\gis\\grundlagen.fdb</span></div><div class="filter">${mi('filter')}<input placeholder="Filter"></div></div>
  <div class="work ex-work">
    <aside class="pane tree-pane"><div class="pane-body">${exTree()}</div></aside>
    <div class="splitter v"></div>
    <section class="pane content-pane">${tabs(['Inhalt', 'Daten', 'Vorschau', 'Metadaten'])}<div class="pane-body">${exTable()}</div></section>
  </div>
  <footer class="statusbar">${statusExplorer()}</footer></div>`
    };

    // ------------------------------------------------------------ Variant B: Rail
    const railB = (groups, activeKey) => `<nav class="rail">${groups.map(g => `<button class="rail-btn ${g.key === activeKey ? 'on' : ''} ${g.selected ? 'is-selected' : ''} ${g.context ? 'is-context' : ''}" data-rail="${g.key}" title="${g.label}">${mi(g.mi)}<span>${g.label.split(':')[0].split(' ')[0]}</span></button>`).join('')}
      <span class="grow"></span><button class="rail-btn" title="About">${mi('help')}<span>Hilfe</span></button></nav>`;
    const toolbarB = (g) => {
        let last = -1;
        return `<div class="tbar"><span class="tbar-title">${g.label}</span>${g.tools.map(t => {
            const dec = Math.floor(t.s / 10), sep = last >= 0 && dec > last ? '<span class="tbar-sep"></span>' : ''; last = dec;
            return sep + `<button class="tb ${t.active ? 'active' : ''}" ${t.dialog ? 'data-dialog' : ''} title="${t.n}">${sp(t.i)}<span>${t.n}</span></button>`;
        }).join('')}</div>`;
    };
    const B = {
        carto: (key) => {
            const g = CARTO_GROUPS.find(x => x.key === key) || CARTO_GROUPS[1];
            return `<div class="shell">
  ${appHeader('Carto', '<span class="doc">Stadtplan Graz</span><span class="doc-sub">G:\\maps\\stadtplan.mxl</span>')}
  <div class="body">${railB(CARTO_GROUPS, g.key)}
    <div class="col">${toolbarB(g)}
      <div class="work carto-work">
        <aside class="pane toc-pane"><div class="pane-head">Inhaltsverzeichnis<span class="grow"></span><button class="ib sm">${mi('more')}</button></div><div class="pane-body">${toc()}</div></aside>
        <div class="splitter v"></div>
        <section class="main">
          <div class="pane map-pane"><div class="map">${mapSvg()}<div class="map-ctl"><button>+</button><button>−</button></div></div></div>
          <div class="splitter h"></div>
          <div class="pane data-pane">${tabs(['Gebäude (9)', 'Straßen (124)'])}<div class="pane-body">${dataTable()}</div></div>
        </section>
      </div>
    </div></div>
  <footer class="statusbar">${statusCarto()}</footer></div>`;
        },
        explorer: (key) => {
            const g = EXPLORER_GROUPS.find(x => x.key === key) || EXPLORER_GROUPS[2];
            return `<div class="shell">
  ${appHeader('Explorer', '<span class="doc">grundlagen.fdb</span>')}
  <div class="body">${railB(EXPLORER_GROUPS, g.key)}
    <div class="col">${toolbarB(g)}
      <div class="navbar"><button class="ib">${mi('back')}</button><button class="ib">${mi('up')}</button><div class="path">${sp('basic:database', 16)}<span>G:\\daten\\gis\\grundlagen.fdb</span></div><div class="filter">${mi('filter')}<input placeholder="Filter"></div></div>
      <div class="work ex-work">
        <aside class="pane tree-pane"><div class="pane-body">${exTree()}</div></aside>
        <div class="splitter v"></div>
        <section class="pane content-pane">${tabs(['Inhalt', 'Daten', 'Vorschau', 'Metadaten'])}<div class="pane-body">${exTable()}</div></section>
      </div>
    </div></div>
  <footer class="statusbar">${statusExplorer()}</footer></div>`;
        }
    };

    // ------------------------------------------------------------ Variant C: Cards
    const barC = (app, groups, center) => `<header class="appbar cbar">${brand(app)}
      <div class="cbar-tools">${groups.map(g => `<div class="cg ${g.selected ? 'is-selected' : ''} ${g.context ? 'is-context' : ''}"><span class="cg-label">${g.label}</span><div class="cg-tools">${g.tools.map(t =>
        `<button class="ct ${t.active ? 'active' : ''}" ${t.dialog ? 'data-dialog' : ''} title="${t.n}">${sp(t.i)}<span class="tip">${t.n}</span></button>`).join('')}</div></div>`).join('')}</div>
      <div class="appbar-right">${center || ''}${headerRight()}</div></header>`;
    const C = {
        carto: () => `<div class="shell">
  ${barC('Carto', CARTO_GROUPS, '<span class="doc">Stadtplan Graz</span>')}
  <div class="stage">
    <div class="map full">${mapSvg()}</div>
    <aside class="card toc-card"><div class="card-head">${mi('layers')}<b>Stadtplan Graz</b><span class="grow"></span><button class="ib sm">${mi('more')}</button></div><div class="card-body">${toc()}</div></aside>
    <div class="map-ctl floating"><button>+</button><button>−</button><button title="Zoom Back">${sp('basic:round-left', 16)}</button></div>
    <section class="card data-card"><div class="card-head">${tabs(['Gebäude (9)', 'Straßen (124)'])}<span class="grow"></span><button class="ib sm" title="Andocken">${mi('dock')}</button><button class="ib sm">${mi('close')}</button></div><div class="card-body">${dataTable()}</div></section>
    <footer class="status-pill">${statusCarto()}</footer>
  </div></div>`,
        explorer: () => `<div class="shell">
  ${barC('Explorer', EXPLORER_GROUPS, '')}
  <div class="stage ex-stage">
    <aside class="card tree-card"><div class="card-head">${mi('folder')}<b>Katalog</b></div><div class="card-body">${exTree()}</div></aside>
    <section class="card content-card">
      <div class="card-head navbar"><button class="ib">${mi('back')}</button><button class="ib">${mi('up')}</button><div class="path">${sp('basic:database', 16)}<span>G:\\daten\\gis\\grundlagen.fdb</span></div><div class="filter">${mi('filter')}<input placeholder="Filter"></div></div>
      ${tabs(['Inhalt', 'Daten', 'Vorschau', 'Metadaten'])}
      <div class="card-body">${exTable()}</div>
      <footer class="card-foot">${statusExplorer()}</footer>
    </section>
  </div></div>`
    };

    // ------------------------------------------------------------ Home + Login (same content, header per variant)
    const home = () => `<div class="shell home">
  ${appHeader('', '')}
  <main class="home-main">
    <div class="home-hero"><h1>gView GIS</h1><p>Version 9.26.1 · angemeldet als <b>admin</b> (Administrator)</p></div>
    <div class="home-cards">
      <a class="hcard" href="?variant=${variant}&page=carto"><span class="hcard-ic">${mi('map')}</span><span class="hcard-t">gView Carto</span>
        <span class="hcard-d">Karten erstellen, Geodaten einbinden und kartographisch darstellen.</span><span class="hcard-open" title="In neuem Tab öffnen">${mi('open')}</span></a>
      <a class="hcard" href="?variant=${variant}&page=explorer"><span class="hcard-ic">${mi('folder')}</span><span class="hcard-t">gView Explorer</span>
        <span class="hcard-d">Geodaten durchsuchen, zwischen Datasets kopieren und Workloads ausführen.</span><span class="hcard-open" title="In neuem Tab öffnen">${mi('open')}</span></a>
      <a class="hcard custom" style="--tile-bg:#0f766e;--tile-fg:#fff"><span class="hcard-ic">${mi('open')}</span><span class="hcard-t">WebGIS Portal</span>
        <span class="hcard-d">Custom Tile aus der Konfiguration (eigene Farben).</span></a>
      <div class="hcard static"><span class="hcard-ic">${mi('memory')}</span><span class="hcard-t">Speicher</span>
        <span class="hcard-d"><b class="big">412 MB</b> belegt</span><button class="btn outline sm">Aufräumen</button></div>
    </div>
  </main></div>`;
    const login = () => `<div class="login-wrap"><div class="login-card">
  <div class="login-brand">${brand('')}</div>
  <h2>Anmelden</h2><p class="muted">gView Web – Carto &amp; Explorer</p>
  <label class="fld"><span>Benutzername</span><input value="admin"></label>
  <label class="fld"><span>Passwort</span><input type="password" value="xxxxxxxx"></label>
  <button class="btn primary block">Login</button>
  <div class="login-foot"><button class="hb" data-theme-btn><span data-theme-label>${themeIcon(getTheme())}</span></button><span class="muted">v9.26.1</span></div>
</div></div>`;

    // ------------------------------------------------------------ render
    const app = document.getElementById('app');
    let railKey = { carto: 'Map', explorer: 'Context' }[page];
    const render = () => {
        document.body.className = 'v-' + variant + ' p-' + page;
        const V = { A, B, C }[variant];
        app.innerHTML = (page === 'home' ? home() : page === 'login' ? login() : V[page](railKey)) + (dialog ? dialogHtml() : '') + switcher();
        wire();
    };
    const switcher = () => `<div class="proto-bar">
  <button data-v="-1">‹</button><span class="pv"><b>${variant}</b> ${VARIANTS[variant]}</span><button data-v="1">›</button>
  <span class="ps">|</span>${PAGES.map(p => `<button class="pp ${p === page ? 'on' : ''}" data-p="${p}">${p}</button>`).join('')}
  <span class="ps">|</span><button class="pp ${dialog ? 'on' : ''}" data-d>dialog</button>
  <span class="ps">|</span><button data-theme-btn><span data-theme-label>${themeIcon(getTheme())}</span></button></div>`;
    const wire = () => {
        const keys = Object.keys(VARIANTS);
        app.querySelectorAll('[data-v]').forEach(b => b.onclick = () => go({ variant: keys[(keys.indexOf(variant) + +b.dataset.v + keys.length) % keys.length] }));
        app.querySelectorAll('[data-p]').forEach(b => b.onclick = () => go({ page: b.dataset.p }));
        app.querySelectorAll('[data-d]').forEach(b => b.onclick = () => go({ dialog: dialog ? null : '1', page: page === 'explorer' ? 'explorer' : 'carto' }));
        app.querySelectorAll('[data-theme-btn]').forEach(b => b.onclick = cycleTheme);
        app.querySelectorAll('[data-dialog]').forEach(b => b.onclick = () => go({ dialog: '1' }));
        app.querySelectorAll('[data-close]').forEach(b => b.onclick = () => go({ dialog: null }));
        app.querySelectorAll('[data-rail]').forEach(b => b.onclick = () => { railKey = b.dataset.rail; render(); });
    };
    document.addEventListener('keydown', e => {
        if (e.target.closest('input,textarea,[contenteditable]')) return;
        const keys = Object.keys(VARIANTS);
        if (e.key === 'ArrowRight') go({ variant: keys[(keys.indexOf(variant) + 1) % keys.length] });
        if (e.key === 'ArrowLeft') go({ variant: keys[(keys.indexOf(variant) + keys.length - 1) % keys.length] });
    });
    render();
    applyTheme();
})();
