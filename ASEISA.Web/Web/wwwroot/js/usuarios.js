(() => {
    const modal = document.getElementById('confirmarAcceso');
    if (!modal) return;
    modal.addEventListener('show.bs.modal', event => {
        const source = event.relatedTarget;
        if (!source) return;
        const selector = source.dataset.selector ? document.getElementById(source.dataset.selector) : null;
        if (selector && !selector.value) { event.preventDefault(); selector.reportValidity(); return; }
        document.getElementById('descripcionAcceso').textContent = (source.dataset.descripcion || '') + (selector ? ' Rol: ' + selector.options[selector.selectedIndex].text : '');
        const confirmar = modal.querySelector('.modal-footer button[type="submit"], .modal-footer form button');
        const destructiva = source.dataset.accion === 'retirar' ||
            (source.dataset.accion === 'estado' && source.dataset.valor === 'false') ||
            (source.dataset.accion === 'aprobacion' && source.dataset.valor === 'false');
        confirmar.classList.toggle('btn-contacto-primary', !destructiva);
        confirmar.classList.toggle('btn-outline-danger', destructiva);
        document.getElementById('accionAcceso').value = source.dataset.accion || '';
        document.getElementById('valorAcceso').value = source.dataset.valor || '';
        document.getElementById('rolAcceso').value = selector ? selector.value : (source.dataset.rol || '');
    });
})();