
document.addEventListener("DOMContentLoaded", () => {

    const toggle = document.getElementById("erpMenuToggle");
    const overlay = document.getElementById("erpSidebarOverlay");
    const sidebar = document.getElementById("erpSidebar");

    function closeMenu() {
        document.body.classList.remove("erp-menu-open");
        toggle?.setAttribute("aria-expanded", "false");
    }

    toggle?.addEventListener("click", () => {
        const isOpen =
            document.body.classList.toggle("erp-menu-open");

        toggle.setAttribute("aria-expanded", String(isOpen));
    });

    overlay?.addEventListener("click", closeMenu);

    document.addEventListener("keydown", event => {
        if (event.key === "Escape") closeMenu();
    });

    sidebar?.querySelectorAll(".erp-nav-link").forEach(link => {
        link.addEventListener("click", () => {
            if (window.innerWidth <= 991) closeMenu();
        });
    });

    // Marcar la navegación activa
    const currentPath = window.location.pathname.toLowerCase();
    const currentHash = window.location.hash.toLowerCase();

    const links = document.querySelectorAll(".erp-nav-link");

    links.forEach(link => {
        link.classList.remove("active");

        const url = new URL(link.href, window.location.origin);

        if (
            url.pathname.toLowerCase() === currentPath &&
            url.hash.toLowerCase() === currentHash
        ) {
            link.classList.add("active");
            link.setAttribute("aria-current", "page");
        } else {
            link.removeAttribute("aria-current");
        }
    });

    window.addEventListener("hashchange", () => {
        const hash = window.location.hash.toLowerCase();

        links.forEach(link => {
            const url = new URL(link.href, window.location.origin);
            const active =
                url.pathname.toLowerCase() ===
                    window.location.pathname.toLowerCase() &&
                url.hash.toLowerCase() === hash;

            link.classList.toggle("active", active);

            if (active) {
                link.setAttribute("aria-current", "page");
            } else {
                link.removeAttribute("aria-current");
            }
        });
    });

});
