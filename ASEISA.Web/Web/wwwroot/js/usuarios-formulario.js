(() => {
    const reglas = globalThis.RegistroCampos;
    if (!reglas) return;
    document.querySelectorAll('[data-formulario-usuario]').forEach(form => {
        const campo = nombre => form.querySelector(`[name$=".${nombre}"]`);
        const tipo = campo('TipoCliente'), identificacion = campo('Identificacion'), telefono = campo('Telefono'), correo = campo('Correo');
        function validar(input, mensaje) {
            if (!input) return true;
            input.setCustomValidity(mensaje);
            const feedback = form.querySelector(`[data-valmsg-for="${input.name}"]`);
            if (feedback) {
                feedback.textContent = mensaje;
                feedback.classList.toggle('field-validation-error', !!mensaje);
                feedback.classList.toggle('field-validation-valid', !mensaje);
            }
            input.setAttribute('aria-invalid', mensaje ? 'true' : 'false');
            return !mensaje;
        }
        function validarId() {
            return !identificacion || validar(identificacion, reglas.identificacionValida(identificacion.value, tipo?.value)
                ? '' : tipo?.value === 'Persona jurídica' ? 'La cédula jurídica debe contener 10 dígitos y comenzar con 3.' : 'La cédula física debe contener 9 dígitos y comenzar del 1 al 9.');
        }
        function actualizarTipo(limpiar) {
            if (!tipo || !identificacion) return;
            const config = reglas.configuracion(tipo.value);
            identificacion.placeholder = config?.ejemplo || 'Seleccione el tipo de identificación';
            identificacion.maxLength = config ? config.longitud + 2 : 12;
            identificacion.disabled = !config;
            if (limpiar) { identificacion.value = ''; validar(identificacion, ''); }
            else identificacion.value = reglas.formatearIdentificacion(identificacion.value, tipo.value);
            const razon = campo('NombreRazonSocial');
            if (razon) { razon.required = tipo.value === 'Persona jurídica'; if (limpiar) validar(razon, ''); }
        }
        function validarRazon() {
            const razon = campo('NombreRazonSocial');
            if (razon) validar(razon, tipo?.value === 'Persona jurídica' && !razon.value.trim() ? 'La razón social es obligatoria para personas jurídicas.' : '');
        }
        campo('NombreRazonSocial')?.addEventListener('input', validarRazon);
        tipo?.addEventListener('change', () => actualizarTipo(true));
        identificacion?.addEventListener('input', () => {
            identificacion.value = reglas.formatearIdentificacion(identificacion.value, tipo.value);
            validarId();
        });
        reglas.vincularTelefono(telefono, mensaje => validar(telefono, mensaje));
        correo?.addEventListener('input', () => validar(correo, reglas.correoValido(correo.value) ? '' : 'Ingrese un correo electrónico válido.'));
        form.addEventListener('submit', evento => {
            validarId();
            validarRazon();
            if (correo) validar(correo, reglas.correoValido(correo.value) ? '' : 'Ingrese un correo electrónico válido.');
            if (telefono) validar(telefono, reglas.telefonoValido(telefono.value) ? '' : 'El teléfono debe contener 8 dígitos.');
            if (!form.checkValidity()) { evento.preventDefault(); form.reportValidity(); }
        });
        actualizarTipo(false);
    });
})();