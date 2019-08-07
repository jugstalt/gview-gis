if (jQuery) {
    $('body').addClass('sidebar-collapsed');

    var webPortalLayout = function () {
        var refreshSidebar = function () {
            $(".webportal-layout-sidebar-item").each(function (i, item) {
                var $item = $(item);
                if ($item.hasClass('webportal-click-added') == false) {
                    $item
                        .addClass('webportal-click-added')
                        .click(function (e) {
                            if ($(window).width() <= 768) {
                                $('body').addClass('sidebar-collapsed');
                            }

                            var $a = $(this).find('a:first');
                            
                            if (!e.originalEvent || $a.get(0) === e.originalEvent.target)
                                return;

                            e.stopPropagation();
                            var href = $a.attr('href');
                            if (href) {
                                document.location = href;
                            } else {
                                $a.trigger('click');
                            }

                           
                        });
                }
                var $letter = $item.children('.webportal-layout-sidebar-item-firstletter');
                if ($letter.length !== 0) {
                    var firstLetter = $item.children('a').html()[0];
                    $letter.html(firstLetter);
                }
            });
        };

        return {
            refreshSidebar: refreshSidebar
        }
    }();

    (function($) {
        $(document).ready(function () {
            var onResize = function (e) {
                if ($(window).width() <= 768) {
                    $('body').addClass('sidebar-collapsed');
                } else {
                    $('body').removeClass('sidebar-collapsed');
                }
            };

            $(window).resize(onResize);
            onResize();

            if ($('.webportal-layout-sidebar .collapse-button').length === 0) {
                $("<div class='collapse-button'></div>")
                    .prependTo($('.webportal-layout-sidebar'));
            }

            webPortalLayout.refreshSidebar();

            $('.navbar-brand')
                .click(function () {
                    $('body').removeClass('sidebar-collapsed');
                });

            $('.webportal-layout-sidebar .collapse-button')
                .click(function () {
                    $('body').toggleClass('sidebar-collapsed');
                });
           

            $(".webportal-layout-main")
                .data('prev-scrollTop', 0)
                .scroll(function (e) {
                    var $this = $(this);

                    var scrollTop = $this.scrollTop();
                    var prevScrollTop = $this.data('prev-scrollTop');
                    var direction = scrollTop - prevScrollTop;

                    console.log(direction);

                    var $body = $('body');

                    if (direction > 60) {
                        $body.addClass('hide-header');
                        $this.data('prev-scrollTop', scrollTop);
                    } else if (direction < -60) {
                        $body.removeClass('hide-header');
                        $this.data('prev-scrollTop', scrollTop);
                    }
                    else if ((direction > 0 && $body.hasClass('hide-header')) ||
                             (direction < 0 && !$body.hasClass('hide-header'))) {
                        $this.data('prev-scrollTop', scrollTop);
                    }

                });
        });
    }) (jQuery);
}