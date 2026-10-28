
document.addEventListener("DOMContentLoaded", () => {

    const newPassword =
        document.getElementById("newPassword");

    const confirmPassword =
        document.getElementById("confirmPassword");

    const strengthText =
        document.getElementById("strengthText");

    const bars =
        document.querySelectorAll(".strength-bars span");

    const matchMessage =
        document.getElementById("matchMessage");

    const form =
        document.getElementById("passwordForm");

    const submitButton =
        document.getElementById("submitButton");

    // Mostrar u ocultar contraseñas
    document.querySelectorAll(".toggle-password")
        .forEach(button => {

            button.addEventListener("click", () => {

                const input =
                    document.getElementById(button.dataset.target);

                const icon = button.querySelector("i");

                const visible = input.type === "text";

                input.type = visible ? "password" : "text";

                icon.className = visible
                    ? "bi bi-eye"
                    : "bi bi-eye-slash";

                button.setAttribute(
                    "aria-label",
                    visible ? "Mostrar contraseña" : "Ocultar contraseña"
                );
            });
        });

    // Indicador visual de seguridad
    newPassword.addEventListener("input", () => {

        const password = newPassword.value;

        let score = 0;

        if (password.length >= 8) score++;
        if (password.length >= 12) score++;
        if (/[A-Z]/.test(password) &&
            /[a-z]/.test(password)) score++;
        if (/\d/.test(password) &&
            /[^A-Za-z0-9]/.test(password)) score++;

        const colors = [
            "#E9EDF2",
            "#E74C3C",
            "#F39C12",
            "#3498DB",
            "#27AE60"
        ];

        const labels = [
            "Seguridad de la contraseña",
            "Muy débil",
            "Débil",
            "Aceptable",
            "Fuerte"
        ];

        bars.forEach((bar, index) => {
            bar.style.background =
                index < score ? colors[score] : "#E9EDF2";
        });

        strengthText.textContent = labels[score];
        strengthText.style.color =
            score === 0 ? "#77808E" : colors[score];

        checkMatch();
    });

    // Comprobar coincidencia
    function checkMatch() {

        if (!confirmPassword.value) {
            matchMessage.textContent = "";
            return;
        }

        const match =
            newPassword.value === confirmPassword.value;

        matchMessage.textContent = match
            ? "Las contraseñas coinciden"
            : "Las contraseñas no coinciden";

        matchMessage.style.color =
            match ? "#27AE60" : "#E74C3C";
    }

    confirmPassword.addEventListener("input", checkMatch);

    // Evitar enviar contraseñas diferentes
    form.addEventListener("submit", event => {

        if (newPassword.value !== confirmPassword.value) {
            event.preventDefault();

            matchMessage.textContent =
                "Las contraseñas deben coincidir";

            matchMessage.style.color = "#E74C3C";

            return;
        }

        if (!form.checkValidity()) {
            return;
        }

        submitButton.disabled = true;
        submitButton.innerHTML =
            '<span class="spinner-border spinner-border-sm me-2"></span>Guardando...';
    });

});
