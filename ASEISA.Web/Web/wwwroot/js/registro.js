document.addEventListener("DOMContentLoaded", () => {

    let pasoActual = 0;

    const $ = id => document.getElementById(id);

    const pasos = document.querySelectorAll(".registration-step");
    const indicadores = document.querySelectorAll(".step-item");

    const tipoRadios = document.querySelectorAll(
        'input[name="Formulario.TipoCliente"]'
    );

    function tipoSeleccionado() {
        return document.querySelector(
            'input[name="Formulario.TipoCliente"]:checked'
        )?.value || "";
    }

    function mostrarMensaje(id, mensaje, valido = false) {
        const elemento = $(id);
        if (!elemento) return;

        elemento.textContent = mensaje;
        elemento.className = "feedback-text " +
            (mensaje ? (valido ? "text-success" : "text-danger") : "");
    }
    function mostrarPaso(numero) {
        pasoActual = numero;

        pasos.forEach((paso, indice) => {
            paso.classList.toggle("active", indice === numero);
        });

        indicadores.forEach((indicador, indice) => {
            indicador.classList.toggle("active", indice === numero);
            indicador.classList.toggle("completed", indice < numero);
        });

        $("stepProgress").style.width =
            ((numero + 1) / 3 * 100) + "%";

        $("btnAnterior").hidden = numero === 0;
        $("btnSiguiente").hidden = numero === 2;
        $("btnRegistrar").hidden = numero !== 2;
    }
    function actualizarTipoCliente(limpiarIdentificacion = true) {
        const juridica = tipoSeleccionado() === "Persona jurídica";

        $("razonSocialContainer").hidden = !juridica;

        $("identificacionLabel").innerHTML = juridica
            ? 'Cédula jurídica <span class="text-danger">*</span>'
            : 'Cédula de identidad <span class="text-danger">*</span>';

        $("Formulario_Identificacion").placeholder = juridica
            ? "3-101-123456"
            : "1-1234-5678";

        if (limpiarIdentificacion) {
            $("Formulario_Identificacion").value = "";
        }

        $("personalTitle").textContent = juridica
            ? "Datos del representante"
            : "Datos personales";

        $("personalDescription").textContent = juridica
            ? "Ingrese los datos de la persona que accederá al sistema en representación de la empresa."
            : "Ingrese sus datos personales y de contacto.";

        mostrarMensaje("identificacionFeedback", "");
        mostrarMensaje("razonSocialFeedback", "");

        actualizarUsuario();
    }
    function formatearCedula() {
        const juridica = tipoSeleccionado() === "Persona jurídica";

        const digitos = $("Formulario_Identificacion").value
            .replace(/\D/g, "")
            .slice(0, juridica ? 10 : 9);

        const partes = juridica
            ? [
                digitos.slice(0, 1),
                digitos.slice(1, 4),
                digitos.slice(4, 10)
            ]
            : [
                digitos.slice(0, 1),
                digitos.slice(1, 5),
                digitos.slice(5, 9)
            ];

        $("Formulario_Identificacion").value =
            partes.filter(Boolean).join("-");

        actualizarUsuario();

        if (digitos.length > 0) {
            validarIdentificacion();
        } else {
            mostrarMensaje("identificacionFeedback", "");
        }
    }

    function validarIdentificacion() {
        const digitos = $("Formulario_Identificacion").value
            .replace(/\D/g, "");

        const juridica = tipoSeleccionado() === "Persona jurídica";

        const valido = juridica
            ? /^3\d{9}$/.test(digitos)
            : /^[1-9]\d{8}$/.test(digitos);

        mostrarMensaje(
            "identificacionFeedback",
            valido
                ? "La identificación tiene el formato esperado."
                : juridica
                    ? "La cédula jurídica debe contener 10 dígitos y comenzar con 3."
                    : "La cédula física debe contener 9 dígitos y comenzar con un número del 1 al 9.",
            valido
        );

        return valido;
    }
    function normalizar(texto) {
        return texto
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .toLowerCase()
            .replace(/[^a-z0-9]/g, "");
    }

    function actualizarUsuario() {
        const nombre = $("Formulario_Nombre").value.trim();
        const apellido = $("Formulario_PrimerApellido").value.trim();

        const digitos = $("Formulario_Identificacion").value
            .replace(/\D/g, "");

        const longitud = tipoSeleccionado() === "Persona jurídica"
            ? 10 : 9;

        if (!nombre || !apellido || digitos.length !== longitud) {
            $("Formulario_NombreUsuario").value = "";
            return;
        }

        const inicial = normalizar(nombre.charAt(0));
        const primerApellido = normalizar(apellido.split(/\s+/)[0]);

        $("Formulario_NombreUsuario").value =
            `${inicial}${primerApellido}.${digitos.slice(-4)}`;
    }
    function formatearTelefono() {
        const digitos = $("Formulario_Telefono").value
            .replace(/\D/g, "")
            .slice(0, 8);

        $("Formulario_Telefono").value = digitos.length > 4
            ? digitos.slice(0, 4) + "-" + digitos.slice(4)
            : digitos;
    }
    function validarPaso1() {
        let valido = true;

        if (!tipoSeleccionado()) {
            mostrarMensaje(
                "tipoFeedback",
                "Seleccione un tipo de cliente."
            );
            valido = false;
        } else {
            mostrarMensaje("tipoFeedback", "");
        }

        if (!validarIdentificacion()) {
            valido = false;
        }

        if (tipoSeleccionado() === "Persona jurídica") {
            const razon = $("Formulario_RazonSocial").value.trim();

            if (!razon) {
                mostrarMensaje(
                    "razonSocialFeedback",
                    "La razón social es obligatoria."
                );
                valido = false;
            } else {
                mostrarMensaje("razonSocialFeedback", "");
            }
        }

        if (!$("Formulario_IdEmpresa").value) {
            mostrarMensaje(
                "empresaFeedback",
                "Debe seleccionar una empresa."
            );
            valido = false;
        } else {
            mostrarMensaje("empresaFeedback", "");
        }

        return valido;
    }
    function validarPaso2() {
        let valido = true;

        const nombre = $("Formulario_Nombre").value.trim();
        const apellido = $("Formulario_PrimerApellido").value.trim();

        if (!nombre) {
            mostrarMensaje(
                "nombreFeedback",
                "El nombre es obligatorio."
            );
            valido = false;
        } else {
            mostrarMensaje("nombreFeedback", "");
        }

        if (!apellido) {
            mostrarMensaje(
                "apellidoFeedback",
                "El primer apellido es obligatorio."
            );
            valido = false;
        } else {
            mostrarMensaje("apellidoFeedback", "");
        }

        const telefono = $("Formulario_Telefono").value;

        if (telefono && !/^\d{4}-\d{4}$/.test(telefono)) {
            mostrarMensaje(
                "telefonoFeedback",
                "El teléfono debe contener 8 dígitos."
            );
            valido = false;
        } else {
            mostrarMensaje("telefonoFeedback", "");
        }

        return valido;
    }

    function validarPaso3() {
        let valido = true;

        const correo = $("Formulario_Correo").value.trim();
        const contrasena = $("Formulario_Contrasena").value;
        const confirmar = $("Formulario_ConfirmarContrasena").value;

        const correoValido =
            /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo);

        mostrarMensaje(
            "correoFeedback",
            correoValido
                ? "El formato del correo es válido."
                : "Ingrese un correo electrónico válido.",
            correoValido
        );

        if (!correoValido) valido = false;

        const passwordValida =
            contrasena.length >= 8 &&
            /[A-Z]/.test(contrasena) &&
            /\d/.test(contrasena);

        mostrarMensaje(
            "passwordFeedback",
            passwordValida
                ? "La contraseña cumple los requisitos."
                : "Debe contener al menos 8 caracteres, una mayúscula y un número.",
            passwordValida
        );

        if (!passwordValida) valido = false;

        const coincide =
            confirmar.length > 0 &&
            confirmar === contrasena;

        mostrarMensaje(
            "confirmarFeedback",
            coincide
                ? "Las contraseñas coinciden."
                : "Las contraseñas no coinciden.",
            coincide
        );

        if (!coincide) valido = false;

        return valido;
    }

    tipoRadios.forEach(radio => {
        radio.addEventListener("change", () => {
            actualizarTipoCliente(true);
        });
    });

    $("Formulario_Identificacion")
        .addEventListener("input", formatearCedula);

    $("Formulario_Nombre")
        .addEventListener("input", actualizarUsuario);

    $("Formulario_PrimerApellido")
        .addEventListener("input", actualizarUsuario);

    $("Formulario_Telefono")
        .addEventListener("input", formatearTelefono);

    // Correo en tiempo real
    $("Formulario_Correo").addEventListener("input", () => {
        const correo = $("Formulario_Correo").value.trim();

        if (!correo) {
            mostrarMensaje("correoFeedback", "");
            return;
        }

        const valido =
            /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo);

        mostrarMensaje(
            "correoFeedback",
            valido
                ? "El formato del correo es válido."
                : "Ingrese un correo electrónico válido.",
            valido
        );
    });

    // Contraseña en tiempo real
    $("Formulario_Contrasena").addEventListener("input", () => {
        const valor = $("Formulario_Contrasena").value;

        if (!valor) {
            mostrarMensaje("passwordFeedback", "");
            return;
        }

        const valido =
            valor.length >= 8 &&
            /[A-Z]/.test(valor) &&
            /\d/.test(valor);

        mostrarMensaje(
            "passwordFeedback",
            valido
                ? "La contraseña cumple los requisitos."
                : "Debe contener al menos 8 caracteres, una mayúscula y un número.",
            valido
        );
    });

    // Confirmación en tiempo real
    $("Formulario_ConfirmarContrasena")
        .addEventListener("input", () => {

            const confirmar =
                $("Formulario_ConfirmarContrasena").value;

            if (!confirmar) {
                mostrarMensaje("confirmarFeedback", "");
                return;
            }

            const coincide =
                confirmar === $("Formulario_Contrasena").value;

            mostrarMensaje(
                "confirmarFeedback",
                coincide
                    ? "Las contraseñas coinciden."
                    : "Las contraseñas no coinciden.",
                coincide
            );
        });

    // Mostrar / ocultar contraseña
    document.querySelectorAll("[data-toggle-password]")
        .forEach(boton => {

            boton.addEventListener("click", () => {
                const input = $(boton.dataset.togglePassword);

                input.type = input.type === "password"
                    ? "text"
                    : "password";

                boton.querySelector("i").className =
                    input.type === "password"
                        ? "bi bi-eye"
                        : "bi bi-eye-slash";
            });

        });

    // Siguiente
    $("btnSiguiente").addEventListener("click", () => {
        const valido = pasoActual === 0
            ? validarPaso1()
            : validarPaso2();

        if (valido) {
            mostrarPaso(pasoActual + 1);
        }
    });

    // Anterior
    $("btnAnterior").addEventListener("click", () => {
        mostrarPaso(pasoActual - 1);
    });

    // Envío final
    $("registroForm").addEventListener("submit", evento => {
        const paso1Valido = validarPaso1();
        const paso2Valido = validarPaso2();
        const paso3Valido = validarPaso3();

        if (!paso1Valido || !paso2Valido || !paso3Valido) {
            evento.preventDefault();

            if (!paso1Valido) {
                mostrarPaso(0);
            } else if (!paso2Valido) {
                mostrarPaso(1);
            } else {
                mostrarPaso(2);
            }
        }
    });

    actualizarTipoCliente(false);
    actualizarUsuario();
    mostrarPaso(0);

});
