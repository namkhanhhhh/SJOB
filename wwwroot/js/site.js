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

    // Improved hover functionality for user dropdown
    $('.nav-item.dropdown').each(function () {
        var $dropdown = $(this);
        var $menu = $dropdown.find('.dropdown-menu');
        var timeout;

        // Show dropdown on hover
        $dropdown.hover(
            function () {
                // On mouse enter, clear any existing timeout and show dropdown
                clearTimeout(timeout);
                $('.dropdown-menu.show').not($menu).removeClass('show'); // Close other dropdowns
                $menu.addClass('show');
            },
            function () {
                // On mouse leave, set a timeout to hide dropdown
                // This gives time to move to the dropdown menu
                timeout = setTimeout(function () {
                    if (!$menu.is(':hover')) {
                        $menu.removeClass('show');
                    }
                }, 200); // 200ms delay before hiding
            }
        );

        // Handle mouse enter/leave on the dropdown menu itself
        $menu.hover(
            function () {
                // On mouse enter menu, clear the timeout to prevent hiding
                clearTimeout(timeout);
            },
            function () {
                // On mouse leave menu, hide after a short delay
                timeout = setTimeout(function () {
                    $menu.removeClass('show');
                }, 200); // 200ms delay before hiding
            }
        );
    });

    // Prevent the dropdown from closing when clicking inside it
    $('.dropdown-menu').on('click', function (e) {
        // Don't stop propagation for form submissions
        if (!$(e.target).closest('form').length) {
            e.stopPropagation();
        }
    });

    // Make sure logout form submits correctly
    $('#logoutForm button[type="submit"]').on('click', function (e) {
        e.preventDefault();
        $('#logoutForm').submit();
    });
});