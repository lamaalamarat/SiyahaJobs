// Public site behaviours
(function () {
    // Auto-dismiss toasts after 5s
    document.querySelectorAll('.toast').forEach(function (t) {
        setTimeout(function () { t.style.opacity = '0'; setTimeout(function () { t.remove(); }, 300); }, 5000);
    });

    // Drawer close on link click
    document.querySelectorAll('.drawer a').forEach(function (a) {
        a.addEventListener('click', function () {
            document.querySelector('.drawer')?.classList.remove('open');
        });
    });
})();
