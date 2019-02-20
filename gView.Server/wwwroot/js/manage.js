if (!window.gview)
    window.gview = {};

window.gview.manage = function () {
    var rootUrl = '';

    //
    //  Get/Post
    //

    var get = function (options) {
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
    };

    var postForm = function ($form, options) {
        var data = {};

        if ($form.find('.form-value').length === 0) {
            data["_emtpyform"] = true;   // One properterty needed!!!!
        } else {
            $form.find('.form-value').each(function (i, input) {
                data[$(input).attr('name')] = $(input).attr('type') === 'checkbox' ? $(input).prop('checked') : $(input).val();
            });
        }

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
        get(options);
    };

    //
    // UI Elements
    //

    var modalDialog = function (options) {
        var $blocker = $("<div>").addClass('gview5-modal-blocker')
            .appendTo($('body'))
            .click(function (e) {
                $(this).find('.button-close').trigger('click');
            });

        var $modal = $("<div>").addClass('modal-dialog')
            .appendTo($blocker)
            .click(function (e) {
                e.stopPropagation();
            });

        if (options.title) {
            $("<div>" + options.title + "</div>").addClass('modal-title').appendTo($modal);
        }
        var $body = $("<div>").addClass('modal-body').appendTo($modal);
        var $footer = $("<div>").addClass('modal-footer').appendTo($modal);

        $("<button>Close</button>")
            .addClass('button-close')
            .appendTo($footer)
            .click(function (e) {
                e.stopPropagation();
                if (options.onClose)
                    options.onClose($body);
                $(this).closest('.gview5-modal-blocker').remove();
            });
        if (options.onOk) {
            $("<button>OK</button>")
                .addClass('button-ok')
                .appendTo($footer)
                .click(function (e) {
                    e.stopPropagation();
                    if (options.onOk)
                        options.onOk($body);
                    $(this).closest('.gview5-modal-blocker').remove();
                });
        }

        if (options.onLoad)
            options.onLoad($body);
    };

    //
    // Page Services
    //
    var createServiceListItem = function ($services, service) {
        var $service = $services.children(".service[data-service='" + (service.folder ? service.folder + "/" : "") + service.name + "']");
        if ($service.length === 0) {
            $service = $("<li></li>")
                .attr('data-service', (service.folder ? service.folder + '/' : '') + service.name)
                .addClass('service')
                .appendTo($services);
        }
        $service.removeClass().addClass('service ' + service.status).empty();
        if (service.hasErrors)
            $service.addClass('has-errors');
       
        var $toolbar = $("<div>").addClass('toolbar').appendTo($service);

        $("<div>")
            .addClass('icon clickable settings')
            .appendTo($toolbar);
        $("<div>")
            .addClass('icon clickable log')
            .appendTo($toolbar)
            .click(function () {
                var serviceName = $(this).closest('.service').attr('data-service');

                var renderErrorLogs = function ($target, result) {
                    $target.empty();
                    var $ul = $("<ul>").appendTo($target);
                    $.each(result.errors, function (i, e) {
                        $("<li>").appendTo($ul).html(e.replace(/(?:\r\n|\r|\n)/g, '<br>'));
                    });
                    if (result.ticks) {
                        $("<button>older...</button>").appendTo($target)
                            .click(function () {
                                get({
                                    url: '/manage/serviceerrorlogs?service=' + serviceName + "&last=" + result.ticks,
                                    success: function (result) {
                                        renderErrorLogs($target, result);
                                    }
                                });
                            });
                    }
                };

                modalDialog({
                    title: service.name + " (Error Logs)",
                    onLoad: function ($body) {
                        $body.addClass('error-logs');
                        get({
                            url: '/manage/serviceerrorlogs?service=' + serviceName,
                            success: function (result) {
                                renderErrorLogs($body, result);
                            }
                        });
                    }
                });
            });
        $("<div>")
            .addClass('icon clickable security' + (service.hasSecurity === true ? '1' : '0'))
            .appendTo($toolbar)
            .click(function () {
                var serviceName = $(this).closest('.service').attr('data-service');

                var renderSecurityTableRow = function ($row, allTypes, rule) {
                    $("<td>✖</td>").addClass('remove').appendTo($row)
                        .click(function () {
                            $(this).closest('tr').remove();
                        });
                    $("<td>").addClass('username').attr('data-username', rule.username).html(rule.username).appendTo($row);

                    var allInterpreters = false;
                    $.each(allTypes, function (i, type) {
                        $cell = $("<td>").addClass('rule').appendTo($row);
                        var hasRule = $.inArray(type.toLowerCase(), rule.servicetypes) >= 0;
                        var $chkbox = $("<input type='checkbox' name='" + rule.username + "~" + type + "' />").addClass('form-value').prop('checked', hasRule).appendTo($cell);

                        if (type.indexOf('_') === 0) {
                            if (type === '_all') {
                                allInterpreters = hasRule;
                                $chkbox.click(function () {
                                    var val = $(this).prop('checked');
                                    $(this).closest('tr').find('input.interpreter').css('display', val === true ? 'none' : '');
                                });
                            } else {
                                $chkbox.addClass('interpreter').css('display', allInterpreters === true ? 'none' : '');
                            }
                        }
                    });
                };
                var renderSecurityTable = function($target, result) {
                    $target.empty();

                    var $tab = $("<table>").appendTo($target);
                    var $row = $("<tr>").appendTo($tab);

                    var $cell = $("<th>").appendTo($row);  // remove (X)
                    $("<th>").html('User').appendTo($row);

                    $.each(result.allTypes, function (i, type) {
                        var title = type;
                        if (title.indexOf("_") === 0)
                            title = "#" + title.substr(1).toUpperCase();
                        $("<th>").addClass('rule').html(title).appendTo($row);
                    });

                    $.each(result.accessRules, function (i, rule) {
                        $row = $("<tr>").appendTo($tab);
                        renderSecurityTableRow($row, result.allTypes, rule);
                    });

                    $row = $("<tr>").appendTo($tab);
                    $("<td>").appendTo($row);  // remove (X)
                    $cell = $("<td>").appendTo($row);

                    var $selectUser = $("<select>").appendTo($cell);
                    $("<option value=''></options>").appendTo($selectUser);
                    $("<option value='" + result.anonymousUsername + "'>" + result.anonymousUsername + "</option>").appendTo($selectUser);
                    $.each(result.allUsers, function (i, user) {
                        $("<option value='" + user + "'>").html(user).appendTo($selectUser);
                    });
                    $selectUser.click(function () {
                        val = $(this).val();
                        if (val && $(this).closest('table').find(".username[data-username='" + val + "']").length === 0) {
                            $row = $("<tr>").insertBefore($(this).closest('tr'));

                            var servicetypes = [];
                            $.each(result.allTypes, function (i, t) {
                                if (t.indexOf("_") !== 0 || t === '_all')
                                    servicetypes.push(t.toLowerCase());
                            });
                            renderSecurityTableRow($row, result.allTypes, { username: val, servicetypes: servicetypes });
                        }
                        $(this).val('');
                    });
                };

                modalDialog({
                    title: service.name + " (Security)",
                    onLoad: function ($body) {
                        $body.addClass('service-security');
                        get({
                            url: '/manage/servicesecurity?service=' + serviceName,
                            success: function (result) {
                                renderSecurityTable($body, result);
                            }
                        });
                    },
                    onOk: function ($body) {
                        postForm($body, {
                            url: '/manage/servicesecurity?service=' + serviceName,
                            success: function (result) {
                                console.log(result);
                                createServiceListItem($('.gview5-manage-body').find('.services'), result.service);
                            }
                        });
                    }
                });
            });
        $("<div>")
            .addClass('icon clickable start ' + (service.status === 'running' ? 'gray' : ''))
            .appendTo($toolbar)
            .click(function () { setServiceStatus($(this).closest('.service'), 'running'); });
        $("<div>")
            .addClass('icon clickable stop ' + (service.status === 'stopped' ? 'gray' : ''))
            .appendTo($toolbar)
            .click(function () { setServiceStatus($(this).closest('.service'), 'stopped'); });
        $("<div>")
            .addClass('icon clickable refresh')
            .appendTo($toolbar)
            .click(function () { setServiceStatus($(this).closest('.service'), 'refresh'); });

        $("<h4>" + service.name + "</h4>").appendTo($service);
        if (service.runningSince) {
            $("<div>Running since: " + service.runningSince + "</div>").appendTo($service);
        }
        return $service;
    };

    var folderServices = function (folder) {
        var $services = $(".services").empty();
        get({
            url: '/manage/services?folder=' + folder,
            success: function (result) {
                $.each(result.services, function (i, service) {
                    createServiceListItem($services, service);
                });
            }
        });
    };

    var pageServices = function () {
        var $body = $('.gview5-manage-body').empty().addClass('loading');

        get({ 
            url: '/manage/folders',
            success: function (result) {
                $body.removeClass('loading');

                var $folders = $("<ul>").addClass('folders').appendTo($body);
                $("<li>").addClass('folder selected').html('(root)').attr('data-folder','').appendTo($folders);
                $.each(result.folders, function (i, folder) {
                    if (folder) {
                        $("<li>").addClass('folder').html(folder).attr('data-folder', folder).appendTo($folders);
                    }
                });
                $folders.children('.folder')
                    .click(function () {
                        $(this).parent().children('.folder').removeClass('selected');
                        folderServices($(this)
                            .addClass('selected')
                            .attr('data-folder'));
                    });

                var $services = $("<ul>").addClass('services').appendTo($body);
                folderServices('');
                //$.each(result.services, function (i, service) {
                //    createServiceListItem($services, service);
                //});
            }
        });
    };

    var setServiceStatus = function ($service, status) {
        get({
            url: '/manage/setservicestatus?service=' + $service.attr('data-service') + "&status="+status,
            type: 'post',
            success(result) {
                createServiceListItem($service.closest('.services'), result.service);
            }
        });
    };

    //
    // Page Security
    //
    var appendFormInput = function ($form, name, type, label) {
        var $formInput = $("<div>").addClass('form-input').appendTo($form);
        $("<div>").addClass('label').html(label || name).appendTo($formInput);
        $("<br/>").appendTo($formInput);
        $("<input name='" + name + "' type='" + (type || 'text') + "' autocomplete='off' />").addClass('form-value').appendTo($formInput);
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
            appendFormInput($form, 'NewUsername');
            appendFormInput($form, 'NewPassword', 'password');

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

        get({
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