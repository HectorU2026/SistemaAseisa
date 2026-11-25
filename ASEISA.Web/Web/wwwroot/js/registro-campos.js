// Reglas y máscaras compartidas por registro público, alta administrativa y edición.
(() => {
    const configuracion = tipo => tipo === 'Persona física' ? { longitud: 9, grupos: [1, 4, 4], ejemplo: '1-2345-6789' }
        : tipo === 'Persona jurídica' ? { longitud: 10, grupos: [1, 3, 6], ejemplo: '3-101-123456' } : null;
    const digitos = valor => String(valor || '').replace(/[^0-9]/g, '');
    function formatearIdentificacion(valor, tipo) {
        const config = configuracion(tipo);
        if (!config) return '';
        const numero = digitos(valor).slice(0, config.longitud);
        let inicio = 0;
        return config.grupos.map(longitud => {
            const parte = numero.slice(inicio, inicio + longitud);
            inicio += longitud;
            return parte;
        }).filter(Boolean).join('-');
    }
    function identificacionValida(valor, tipo) {
        const original = String(valor || '').trim();
        return tipo === 'Persona física'
            ? /^(?:[1-9][0-9]{8}|[1-9]-[0-9]{4}-[0-9]{4})$/.test(original)
            : tipo === 'Persona jurídica' && /^(?:3[0-9]{9}|3-[0-9]{3}-[0-9]{6})$/.test(original);
    }
    const formatearTelefono = valor => {
        const numero = digitos(valor).slice(0, 8);
        return numero.length > 4 ? numero.slice(0, 4) + '-' + numero.slice(4) : numero;
    };
    const telefonoValido = valor => !valor || /^(?:[0-9]{8}|[0-9]{4}-[0-9]{4})$/.test(valor);
    function vincularTelefono(input, mostrarError = () => {}) {
        if (!input) return;
        input.type = 'tel';
        input.inputMode = 'numeric';
        input.maxLength = 9;
        input.placeholder = 'XXXX-XXXX';
        input.pattern = '[0-9]{4}-[0-9]{4}';
        // Normalizar valores completos existentes sin ocultar datos inválidos al cargar.
        if (telefonoValido(input.value)) input.value = formatearTelefono(input.value);
        input.addEventListener('input', () => {
            input.value = formatearTelefono(input.value);
            const mensaje = telefonoValido(input.value) ? '' : 'El teléfono debe contener 8 dígitos (XXXX-XXXX).';
            input.setCustomValidity(mensaje);
            mostrarError(mensaje);
        });
    }
    const correoValido = valor => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(valor);
    globalThis.RegistroCampos = Object.freeze({ configuracion, formatearIdentificacion, identificacionValida, formatearTelefono, telefonoValido, vincularTelefono, correoValido });
})();