if (jQuery) {
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

            $('.navbar-brand')
                .click(function () {
                    $('body').removeClass('sidebar-collapsed');
                });

            $('.webportal-layout-sidebar .collapse-button')
                .click(function () {
                    $('body').toggleClass('sidebar-collapsed')
                });
            $(".webportal-layout-sidebar-item")
                .click(function (e) {
                    e.stopPropagation();
                    var href = $(this).find('a:first').attr('href');
                    if (href) {
                        document.location = href;
                    }
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