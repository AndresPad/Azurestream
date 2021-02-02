
$(document).ready(function () {
    "use strict";

    var window_width = $(window).width(),
        window_height = window.innerHeight,
        header_height = $(".default-header").height(),
        header_height_static = $(".site-header.static").outerHeight(),
        fitscreen = window_height - header_height;

    $(".fullscreen").css("height", window_height);
    $(".fitscreen").css("height", fitscreen);

    //-------- Active Sticky Js ----------//
    $(".default-header").sticky({ topSpacing: 0 });

    //------- Add smooth scrolling to all links
    //---- Toggle Menu Bar

    $('.toggle-btn').on('click', function (e) {
        e.preventDefault();
        $('body').toggleClass('overflow-hidden');
        $('.side-menubar').toggleClass('open-menubar');
        $("span", this).toggleClass("lnr-menu lnr-cross");

    });
    $('.side-menubar nav ul li a').on('click', function (e) {
        e.preventDefault();
        $('.side-menubar').toggleClass('open-menubar');
        $(".toggle-btn span").toggleClass("lnr-menu lnr-cross");
        $('body').removeClass('overflow-hidden');
    });

    // Add smooth scrolling to Menu links
    $(".main-menu nav ul li a, .side-menubar nav ul li a").on('click', function (event) {
        if (this.hash !== "") {
            event.preventDefault();
            var hash = this.hash;
            $('html, body').animate({
                scrollTop: $(hash).offset().top - (-10)
            }, 600, function () {

                window.location.hash = hash;
            });
        }
    });

    $(".main-menu nav ul li a").on('click', function (e) {
        $(".main-menu nav ul li").removeClass("active");
        $(this).addClass("active");
    });

    $(window).on('scroll', function (event) {
        var scrollPos = $(document).scrollTop();
        $(".main-menu nav ul li a, .side-menubar nav ul li a").each(function () {
            var currLink = $(this);
            var refElement = $(currLink.attr("href"));

            if (refElement.position().top <= scrollPos && refElement.position().top + refElement.height() > scrollPos) {
                currLink.parent().addClass("active").siblings().removeClass("active");
                return;
            }
            else {
                currLink.parent().removeClass("active");
            }
        });
    });

    //--------- Accordion Icon Change ---------//

    $('.collapse').on('shown.bs.collapse', function () {
        $(this).parent().find(".lnr-plus-circle").removeClass("lnr-plus-circle").addClass("lnr-circle-minus");
    }).on('hidden.bs.collapse', function () {
        $(this).parent().find(".lnr-circle-minus").removeClass("lnr-circle-minus").addClass("lnr-plus-circle");
    });
});