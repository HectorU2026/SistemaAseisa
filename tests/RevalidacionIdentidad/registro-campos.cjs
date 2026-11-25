const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const raiz = path.resolve(__dirname, '../../ASEISA.Web/Web/wwwroot/js');
vm.runInThisContext(fs.readFileSync(path.join(raiz, 'registro-campos.js'), 'utf8'));
const r = globalThis.RegistroCampos;
let casos = 0;
function caso(nombre, prueba) { prueba(); console.log('OK ' + nombre); casos++; }
caso('Máscara física progresiva', () => {
    assert.equal(r.formatearIdentificacion('12345', 'Persona física'), '1-2345');
    assert.equal(r.formatearIdentificacion('123456789', 'Persona física'), '1-2345-6789');
});
caso('Máscara jurídica progresiva', () => assert.equal(r.formatearIdentificacion('3101123456', 'Persona jurídica'), '3-101-123456'));
caso('Pegado elimina letras y limita nueve dígitos', () => assert.equal(r.formatearIdentificacion('A123456789999!', 'Persona física'), '1-2345-6789'));
caso('Limita diez dígitos jurídicos', () => assert.equal(r.formatearIdentificacion('310112345699', 'Persona jurídica'), '3-101-123456'));
caso('Valida cédulas compactas y con máscara', () => {
    assert(r.identificacionValida('123456789', 'Persona física'));
    assert(r.identificacionValida('3-101-123456', 'Persona jurídica'));
});
caso('Rechaza prefijo inválido y separadores mal colocados', () => {
    assert(!r.identificacionValida('0-2345-6789', 'Persona física'));
    assert(!r.identificacionValida('1--2345-6789', 'Persona física'));
    assert(!r.identificacionValida('3-101-12345', 'Persona jurídica'));
});
caso('No inventa tipos adicionales', () => assert.equal(r.formatearIdentificacion('123456789', 'Pasaporte'), ''));
caso('Teléfono con ocho dígitos y sin letras', () => assert.equal(r.formatearTelefono('12A345678999'), '1234-5678'));
caso('Teléfono opcional y longitud válida', () => { assert(r.telefonoValido('')); assert(r.telefonoValido('1234-5678')); assert(!r.telefonoValido('123')); });
caso('Correo validado como formulario público', () => { assert(r.correoValido('ficticio@example.invalid')); assert(!r.correoValido('ficticio@localhost')); assert(!r.correoValido('a b@example.invalid')); });
// DOM mínimo: verifica el cableado del formulario sin navegador, HTTP ni base de datos.
function campo(name, value = '') { return {name: 'Formulario.' + name, value, handlers: {}, addEventListener(n, f) {this.handlers[n] = f;}, setCustomValidity(v) {this.error = v;}, setAttribute() {}}; }
const campos = {TipoCliente: campo('TipoCliente', 'Persona física'), Identificacion: campo('Identificacion', '123456789'), Telefono: campo('Telefono'), Correo: campo('Correo'), NombreRazonSocial: campo('NombreRazonSocial')};
const feedback = {textContent: 'Error del servidor', classList: {toggle() {}}};
const form = {handlers: {}, querySelector(s) {if (s.startsWith('[data-valmsg')) return feedback; return campos[Object.keys(campos).find(n => s.includes('.' + n + '"'))] || null;}, addEventListener(n, f) {this.handlers[n] = f;}};
globalThis.document = {querySelectorAll: () => [form]};
vm.runInThisContext(fs.readFileSync(path.join(raiz, 'usuarios-formulario.js'), 'utf8'));
caso('Carga conserva identificación y errores existentes', () => {assert.equal(campos.Identificacion.value, '1-2345-6789'); assert.equal(feedback.textContent, 'Error del servidor');});
caso('Cambio de tipo limpia identificación y ajusta máscara', () => {
    campos.TipoCliente.value = 'Persona jurídica'; campos.TipoCliente.handlers.change();
    assert.equal(campos.Identificacion.value, ''); assert.equal(campos.Identificacion.placeholder, '3-101-123456'); assert.equal(campos.Identificacion.maxLength, 12); assert(campos.NombreRazonSocial.required);
});
caso('Entrada jurídica usa máscara compartida', () => {campos.Identificacion.value = '3101123456'; campos.Identificacion.handlers.input(); assert.equal(campos.Identificacion.value, '3-101-123456'); assert.equal(campos.Identificacion.error, '');});
caso('Correo inválido genera validación por campo', () => {campos.Correo.value = 'invalido'; campos.Correo.handlers.input(); assert(campos.Correo.error);});
caso('Teléfono administrativo elimina letras y exceso de dígitos', () => {
    campos.Telefono.value = '12A345678999'; campos.Telefono.handlers.input();
    assert.equal(campos.Telefono.value, '1234-5678'); assert.equal(campos.Telefono.error, '');
});
caso('Teléfono incompleto muestra error y vacío conserva opcionalidad', () => {
    campos.Telefono.value = '12345'; campos.Telefono.handlers.input();
    assert.equal(campos.Telefono.value, '1234-5'); assert(campos.Telefono.error);
    campos.Telefono.value = ''; campos.Telefono.handlers.input(); assert.equal(campos.Telefono.error, '');
});
caso('Presentación normaliza ocho dígitos almacenados sin cambiar obligatoriedad', () => {
    const input = campo('Telefono', '12345678'); input.required = true;
    r.vincularTelefono(input);
    assert.equal(input.value, '1234-5678'); assert.equal(input.maxLength, 9); assert.equal(input.placeholder, 'XXXX-XXXX'); assert.equal(input.required, true);
});
console.log('TOTAL=' + casos + '; sin HTTP, SQL ni correos.');