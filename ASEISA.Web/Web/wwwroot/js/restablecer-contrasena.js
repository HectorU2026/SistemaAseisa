
document.addEventListener("DOMContentLoaded", () => {

    const form = document.getElementById("recoveryForm");
    const submitButton = document.getElementById("submitButton");

    if (!form || !submitButton) return;

    form.addEventListener("submit", () => {

        if (!form.checkValidity()) {
            return;
        }

        submitButton.disabled = true;

        submitButton.innerHTML = `
            <span class="spinner-border spinner-border-sm me-2"
                  role="status"
                  aria-hidden="true"></span>
            Enviando enlace...
        `;
    });

});
