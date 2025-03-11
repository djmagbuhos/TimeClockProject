$(document).ready(function () {
    $(".navmore-container").hide(); // Ensure it's hidden initially

    $(".show-more-ia").click(function () {
        $(".navmore-container").slideToggle(300); // Slide up/down on click
    });

    $(".close-nmcontainer").click(function () {
        $(".navmore-container").slideUp(300); // Slide up when clicking close
    });

});