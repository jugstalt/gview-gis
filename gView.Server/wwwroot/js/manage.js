if (!window.gview)
    window.gview = {};

window.gview.manage = function () {

    var folderServices = function (folder) {
        var $services=$(".services").empty();
        $.ajax({
            url: '/geoservices/rest/services' + (folder ? '/' + folder : '')+'?f=pjson',
            type: 'get',
            success: function (result) {
                if (result && result.services) {
                    for (var s in result.services) {
                        var service = result.services[s];
                        var $service = $("<li></li>").addClass('service').appendTo($services);
                        $("<h4>" + service.name + "</h4>").appendTo($service);
                    }
                }
            }

        });
    };

    return {
        folderServices: folderServices
    };
}();