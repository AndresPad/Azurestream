/* ------------------------------------------------------------------------
	jQuery NO CONFLICT ( use $j( ) instead $( ) )	
* ------------------------------------------------------------------------- */
var $j = jQuery.noConflict();

(function ($j) {

    'use strict';

    var Core = {
        initialized: false,
        initialize: function () {
            if (this.initialized) return;
            this.initialized = true;
            this.build();
        },

        /*-------------------------------------------------*/
        /* =  build
        /*-------------------------------------------------*/
        build: function () {
            // Adds browser version on html class.
            $j.browserSelector(); //calls xssmoothscroll.js

            // Adds window smooth scroll on chrome.
            if ($j("html").hasClass("chrome")) {
                $j.smoothScroll();  //calls xssmoothscroll.js
            }
        }
        /*-------------------------------------------------*/
    };
    Core.initialize();

    function initLoad() {
        $j(window).on("load", function () {
            $j(".loader-wrapper").delay(500).fadeOut();
            $j(".dot").delay(1000).fadeOut("slow");
        });
    }

    function initGeneral() {
        /*Performs scrollUp.*/
        $j(function () {
            $j.scrollUp({
                scrollName: 'scrollUp',      // Element ID
                scrollDistance: 300,         // Distance from top/bottom before showing element (px)
                scrollFrom: 'top',           // 'top' or 'bottom'
                scrollSpeed: 500,            // Speed back to top (ms)
                easingType: 'linear',        // Scroll to top easing (see http://easings.net/)
                animation: 'fade',           // Fade, slide, none
                animationSpeed: 400,         // Animation speed (ms)
                scrollTrigger: false,        // Set a custom triggering element. Can be an HTML string or jQuery object
                scrollTarget: false,         // Set a custom target element for scrolling to. Can be element or number
                scrollText: '',              // Text for element, can contain HTML
                scrollTitle: false,          // Set a custom <a> title if required.
                scrollImg: false,            // Set true to use image
                activeOverlay: false,        // Set CSS color to display scrollUp active point, e.g '#00FFFF'
                zIndex: 2147483647           // Z-Index for the overlay
            });
        });

        $j('.bg-img, .thumb-placeholder').each(function (index, el) {
            var image = $j(el).attr('src');
            $j(el).parent().css('background-image', 'url(' + image + ')');
            $j(el).remove();
        });

        /*Performs a smooth page scroll to an anchor on the same page.*/
        $j(document).ready(function () {
            // Add scrollspy to <body>
            $j('body').scrollspy({ target: ".navbar", offset: 200 });

            // Add smooth scrolling on all links inside the navbar
            $j("#navigation a").on('click', function (event) {
                // Make sure this.hash has a value before overriding default behavior
                if (this.hash !== "") {
                    // Prevent default anchor click behavior
                    event.preventDefault();

                    // Store hash
                    var hash = this.hash;

                    // Using jQuery's animate() method to add smooth page scroll
                    // The optional number (800) specifies the number of milliseconds it takes to scroll to the specified area
                    $j('html, body').animate({
                        scrollTop: $j(hash).offset().top - 35
                    }, 1200, function () {

                        // Add hash (#) to URL when done scrolling (default click behavior)
                        window.location.hash = hash;
                    });
                }  // End if
            });
        });


        /*This is causing a problem with the Gallery Pop up window so it had to be commented out*/
        //$j(function () {
        //    $j('a[href*="#"]:not([href="#"])').click(function () {
        //        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
        //            var target = $j(this.hash);
        //            target = target.length ? target : $j('[name=' + this.hash.slice(1) + ']');
        //            if (target.length) {
        //                $j('html, body').animate({
        //                    scrollTop: target.offset().top
        //                }, 1000);
        //                return false;
        //            }
        //        }
        //    });
        //});

        $j(document).ready(function () {
            $j('[data-toggle="popover"]').popover({ html: true });
        });

        function toggleIcon(e) {
            $j(e.target)
                .prev('.panel-heading')
                .find(".more-less")
                .toggleClass('fa-plus fa-minus');
        }
        $j('.panel-group').on('hidden.bs.collapse', toggleIcon);
        $j('.panel-group').on('shown.bs.collapse', toggleIcon);

        /* ------------------------------------------------------------------------
	       WOW -- calls wow.min.js
        * ------------------------------------------------------------------------- */
        $j(function () {
            new WOW().init();
        });
    }

    function initNavbar() {
        $j(window).scroll(function () {
            if ($j('section:first').is('.parallax, #home, .splash, #contactsubpage, .subpage')) {
                if ($j(window).scrollTop() >= 100) {
                    $j('#topnav').addClass('scroll');

                } else {
                    $j('#topnav').removeClass('scroll');
                }
            }
        }).trigger('scroll');

        $j(document).ready(function () {
            $j(".navbar-toggle").on("click", function () {
                $j(this).toggleClass("active");
            });
        });

        $j('.navbar-collapse a').click(function () {
            $j(".navbar-collapse").collapse('hide');
            $j(".navbar-header").find(".active").removeClass("active");
        });
    }

    function initCounters() {
    }

    function initCustom() {
    }

    function init() {
        initLoad();
        initGeneral();
        initNavbar();
        initCounters();
        initCustom();
    }

    init();

})(jQuery);
