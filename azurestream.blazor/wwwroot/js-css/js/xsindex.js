jQuery(document).ready(function($){
    "use strict";

   
    //-------- 01 home Phone Carousel Active -------//

    $('.phone-carousel').owlCarousel({
        loop: true,
        dots: true,
        items: 1,
        autoplay: true,
        autoplayTimeout: 2000,
        autoplayHoverPause: true,
        animateOut: 'fadeOutLeft'
    });
    //-------- 02 home Phone Carousel Active -------//

    $('.phone-carousel-2').owlCarousel({
        loop: true,
        dots: true,
        items: 1,
        autoplay: true,
        autoplayTimeout: 2000,
        autoplayHoverPause: true,
        animateOut: 'fadeOutRight'
    });
    //-------- 01 home Phone Carousel Active -------//

    $('.how-work-carousel').owlCarousel({
        loop: true,
        dots: true,
        items: 1,
        autoplay: true,
        autoplayTimeout: 2000,
        autoplayHoverPause: true,
        animateOut: 'fadeOut',
        animateIn: 'fadeInRight'
    });

    //-------- 01 home Testimonial Active -------//

    $('.active-testimonial-carousel').owlCarousel({
        loop: true,
        dots: true,
        items: 1,
        autoplay: true,
        autoplayTimeout: 2000,
        autoplayHoverPause: true,
        animateOut: 'flipInX'

    });
    //-------- 01 home Screen Carousel Active -------//

    $('.active-screen-carousel').owlCarousel({
        loop: true,
        dots: true,
        items: 4,
        margin: 30

    });
    //-------- Blog Carousel Active -------//

    $('.active-blog-slider').owlCarousel({
        loop: true,
        dots: true,
        items: 1,
        autoplay: true,
        autoplayTimeout: 2000,
        smartSpeed: 1000,
        animateOut: 'fadeOut'
    });
    $('.active-article-carousel').owlCarousel({
        loop: true,
        dots: true,
        items: 1,
        autoplay: true,
        autoplayTimeout: 2000,
        smartSpeed: 1000,
        animateOut: 'fadeOut'
    });



    //--------- Accordion Icon Change ---------//

    $('.collapse').on('shown.bs.collapse', function () {
        $(this).parent().find(".lnr-plus-circle").removeClass("lnr-plus-circle").addClass("lnr-circle-minus");
    }).on('hidden.bs.collapse', function () {
        $(this).parent().find(".lnr-circle-minus").removeClass("lnr-circle-minus").addClass("lnr-plus-circle");
    });
});