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

    var alert = function (msg) {
        bootbox.alert(msg);
    };

    var toggleDarkMode = function () {
        let $body = $('body');

        if ($body.hasClass('light')) {
            $body.removeClass('light');
            setStorageValue('colormode', 'dark');
        } else {
            $body.addClass('light');
            setStorageValue('colormode', 'light');
        }
    };

    var getStorageValue = function(name) {
        if (typeof localStorage !== 'undefined') {
            return localStorage.getItem(name);
        }
    };

    var setStorageValue = function(name, value) {
        if (typeof localStorage !== 'undefined') {
            localStorage.setItem(name, value);
        }
    }

    $(function () {
        $('body').addClass(getStorageValue('colormode'));
    });

    return {
        get: get,
        redirectToPath: redirectToPath,
        alert: alert,
        setRootUrl: setRootUrl,
        toggleDarkMode: toggleDarkMode
    };
})();
