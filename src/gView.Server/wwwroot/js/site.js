if (!window.gview) window.gview = {};

window.gview.server = (function () {
    var rootUrl = '/';

    var setRootUrl = function (url) {
        rootUrl = url;
        if (rootUrl[rootUrl.length - 1] !== '/') {
            rootUrl += '/';
        }
    };

    //
    //  Get/Post
    //

    var get = function (options) {
        $.ajax({
            url:
                options.url.indexOf(rootUrl) === 0
                    ? options.url
                    : rootUrl + (options.url[0] === '/' ? options.url.substr(1) : options.url),
            type: options.type || 'get',
            data: options.data || null,
            success:
                options.success ||
                function (result) {
                    gview.server.alert(result);
                },
            error:
                options.error ||
                function (jqXHR, textStatus, errorThrown) {
                    $('.loading').removeClass('loading');
                    gview.server.alert('Error: ' + errorThrown + '(' + textStatus + ')');
                }
        });
    };

    var redirectToPath = function (path) {
        document.location.href = rootUrl + (path[0] === '/' ? path.substr(1) : path);
    };

    //
    //  Dialogs (same markup as manage.js modalDialog, no bootbox)
    //

    var dialog = function (options) {
        var $blocker = $('<div>').addClass('gview5-modal-blocker').appendTo($('body'));
        var $modal = $('<div>').addClass('modal-dialog modal-small').appendTo($blocker);
        var close = function (result) {
            $blocker.remove();
            if (options.callback) options.callback(result);
        };

        if (options.title) {
            $('<div>').addClass('modal-title').text(options.title).appendTo($modal);
        }
        $('<div>').addClass('modal-body').html(options.message || '').appendTo($modal);
        var $footer = $('<div>').addClass('modal-footer').appendTo($modal);

        if (options.cancelLabel) {
            $('<button>').addClass('button-close').text(options.cancelLabel).appendTo($footer)
                .click(function () { close(false); });
        }
        var $ok = $('<button>').addClass('button-ok' + (options.danger ? ' danger' : '')).text(options.okLabel || 'OK').appendTo($footer)
            .click(function () { close(true); });
        $ok.focus();

        $blocker.on('keydown', function (e) {
            if (e.key === 'Escape') close(false);
        });
    };

    var alert = function (msg) {
        dialog({ message: typeof msg === 'string' ? msg : JSON.stringify(msg) });
    };

    // options: { title, message, cancelLabel, okLabel, danger, callback(result) }
    var confirm = function (options) {
        dialog($.extend({ cancelLabel: 'Cancel' }, options));
    };

    //
    //  Color mode: light | dark | system (html[data-theme], set before paint in _Layout.cshtml)
    //

    var themeKey = 'gview-server-theme';
    var themeMedia = window.matchMedia ? window.matchMedia('(prefers-color-scheme: dark)') : null;

    var applyTheme = function (pref) {
        var dark = pref === 'dark' || (pref === 'system' && themeMedia && themeMedia.matches);
        document.documentElement.setAttribute('data-theme', dark ? 'dark' : 'light');
        document.documentElement.setAttribute('data-theme-pref', pref);
    };

    var getTheme = function () {
        return getStorageValue(themeKey) || 'system';
    };

    var setTheme = function (pref) {
        setStorageValue(themeKey, pref);
        applyTheme(pref);
    };

    var toggleDarkMode = function () {
        setTheme(document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark');
    };

    if (themeMedia && themeMedia.addEventListener) {
        themeMedia.addEventListener('change', function () {
            if (getTheme() === 'system') applyTheme('system');
        });
    }

    var getStorageValue = function (name) {
        try {
            return localStorage.getItem(name);
        } catch (e) {
            return null;
        }
    };

    var setStorageValue = function (name, value) {
        try {
            localStorage.setItem(name, value);
        } catch (e) { }
    };

    return {
        get: get,
        redirectToPath: redirectToPath,
        alert: alert,
        confirm: confirm,
        setRootUrl: setRootUrl,
        setTheme: setTheme,
        toggleDarkMode: toggleDarkMode
    };
})();
