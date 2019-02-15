if (!window.gview)
    window.gview = {};

window.gview.manage = function () {
    var rootUrl = '';

    var ajax = function (options) {
        $.ajax({
            url: rootUrl + options.url,
            type: options.type || 'get',
            data: options.data || null,
            success: options.success || function (result) { alert(result) },
            error: options.error || function (jqXHR, textStatus, errorThrown) {
                $('.loading').removeClass('loading');
                alert("Error: " + errorThrown + "(" + textStatus + ")");
            }
        });
    }

    var postForm = function ($form, options) {
        var data = {};
        $form.find('.form-value').each(function (i, input) {
            data[$(input).attr('name')] = $(input).val();
        });

        options.type = 'post';
        options.data = data;

        var onSuccess = options.success;
        options.success = function (result) {
            if (!result.success) {
                $("<div>").addClass('form-error')
                    .html(result.error)
                    .prependTo($form);
            } else {
                $form.find('.form-error').remove();

                if (onSuccess)
                    onSuccess(result);
            }
        };
        ajax(options);
    };

    //
    // Page Services
    //
    var createServiceListItem = function ($services, service) {
        var $service = $("<li></li>").addClass('service').appendTo($services);
        $("<h4>" + service.name + "</h4>").appendTo($service);
    };

    var folderServices = function (folder) {
        var $services = $(".services").empty();
        ajax({
            url: '/geoservices/rest/services' + (folder ? '/' + folder : '') + '?f=pjson',
            success: function (result) {
                $.each(result.services, function (i, service) {
                    createServiceListItem($services, service);
                });
            }
        });
    };

    var pageServices = function () {
        var $body = $('.gview5-manage-body').empty().addClass('loading');

        ajax({ 
            url: '/geoservices/rest/services?f=pjson',
            success: function (result) {
                console.log(result);
                $body.removeClass('loading');

                var $folders = $("<ul>").addClass('folders').appendTo($body);
                $("<li>").addClass('folder selected').html('(root)').attr('data-folder','').appendTo($folders);
                $.each(result.folders, function (i, folder) {
                    $("<li>").addClass('folder').html(folder).attr('data-folder', folder).appendTo($folders);
                });
                $folders.children('.folder')
                    .click(function () {
                        $(this).parent().children('.folder').removeClass('selected');
                        folderServices($(this)
                            .addClass('selected')
                            .attr('data-folder'));
                    });

                var $services = $("<ul>").addClass('services').appendTo($body);
                $.each(result.services, function (i, service) {
                    createServiceListItem($services, service);
                });
            }
        })
    };

    //
    // Page Security
    //
    var appendFormInput = function ($form, name, type, label) {
        var $formInput = $("<div>").addClass('form-input').appendTo($form);
        $("<div>").addClass('label').html(label || name).appendTo($formInput);
        $("<br/>").appendTo($formInput);
        $("<input name='" + name + "' type='" + (type || 'text') + "' />").addClass('form-value').appendTo($formInput);
    };
    var appendFormHidden = function ($form, name, val) {
        $("<input type='hidden' name='" + name + "' />")
            .val(val)
            .addClass('form-value')
            .appendTo($form);
    };

    var pageUser = function (user) {
        var $page = $(".user-properties").empty();

        var $form = $("<div>").addClass('form').appendTo($page);
        if (user === '') {
            appendFormInput($form, 'Username');
            appendFormInput($form, 'Password', 'password');

            $("<button>Create</button>")
                .appendTo($form)
                .click(function () {
                    postForm($form, {
                        url: 'manage/createtokenuser',
                        success: function () {
                            pageSecurity();
                        }
                    });
                });
        } else {

            appendFormHidden($form, 'Username', user);
            appendFormInput($form, 'NewPassword', 'password', 'New password');

            $("<button>Change</button>")
                .appendTo($form)
                .click(function () {
                    postForm($form, {
                        url: 'manage/changetokenuserpassword',
                        success: function () {
                            pageSecurity();
                        }
                    });
                });
        }
    };

    var pageSecurity = function () {
        var $body = $('.gview5-manage-body').empty().addClass('loading');

        ajax({
            url: '/manage/tokenusers',
            success: function (result) {
                var $users = $("<ul>").addClass('users').appendTo($body);

                $("<li>New...</li>").addClass('user new selected').appendTo($users)
                    .click(function () {
                        $(this).parent().children('.user').removeClass('selected');
                        $(this).addClass('selected');
                        pageUser('');
                    });

                $.each(result.users, function (i, user) {
                    $("<li>").addClass('user').attr('data-user', user).html(user)
                        .appendTo($users)
                        .click(function () {
                            $(this).parent().children('.user').removeClass('selected');
                            $(this).addClass('selected');
                            pageUser($(this).attr('data-user'));
                        });
                });

                $("<div>").addClass('user-properties').appendTo($body);

                $users.children('.user.new').trigger('click');
            }
        });
    };

    return {
        pageServices: pageServices,
        pageSecurity: pageSecurity
    };
}();