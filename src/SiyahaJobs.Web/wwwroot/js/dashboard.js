// Dashboard behaviours
(function () {
    // Close sidebar on mobile link click
    document.querySelectorAll('.sidebar-nav a').forEach(function (a) {
        a.addEventListener('click', function () {
            if (window.innerWidth <= 900) {
                document.body.classList.remove('sidebar-open');
            }
        });
    });

    // Confirm any form with data-confirm
    document.querySelectorAll('form[data-confirm]').forEach(function (f) {
        f.addEventListener('submit', function (e) {
            if (!confirm(f.getAttribute('data-confirm'))) e.preventDefault();
        });
    });
})();
