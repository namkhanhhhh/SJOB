$(document).ready(function () {
    // Ensure dropdown menus work properly
    $('.dropdown-toggle').dropdown();

    // Additional check to ensure user dropdown works
    $('#userDropdown').on('click', function (e) {
        e.preventDefault();
        $(this).dropdown('toggle');
    });

    // Force initialize all dropdowns
    $('[data-toggle="dropdown"]').dropdown();
});