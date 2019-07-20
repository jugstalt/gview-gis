if (!window.gview)
    window.gview = {};

window.gview.server = function () {
    var rootUrl = '/';

    var setRootUrl = function (url) {
        rootUrl = url;
    };

    //
    //  Get/Post
    //

    var get = function (options) {
        $.ajax({
            url: options.url.indexOf(rootUrl) === 0 ? options.url : rootUrl + options.url,
            type: options.type || 'get',
            data: options.data || null,
            success: options.success || function (result) { gview.server.alert(result); },
            error: options.error || function (jqXHR, textStatus, errorThrown) {
                $('.loading').removeClass('loading');
                gview.server.alert("Error: " + errorThrown + "(" + textStatus + ")");
            }
        });
    };

    var alert = function (msg) {
        bootbox.alert(msg);
    };

    return {
        get: get,
        alert: alert,
        setRootUrl: setRootUrl
    };
}();