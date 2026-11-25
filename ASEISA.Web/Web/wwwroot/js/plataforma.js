(() => {
    const modal = document.getElementById('accionAdministrador');
    modal?.addEventListener('show.bs.modal', event => {
        const source = event.relatedTarget;
        document.getElementById('descripcionAdministrador').textContent = source.dataset.descripcion || '';
        document.getElementById('accionAdministradorValor').value = source.dataset.accion || '';
        document.getElementById('estadoAdministradorValor').value = source.dataset.valor || '';
        const button = modal.querySelector('.modal-footer form button');
        const destructivo = source.dataset.accion === 'estado' && source.dataset.valor === 'false';
        button.classList.toggle('btn-outline-danger', destructivo); button.classList.toggle('btn-contacto-primary', !destructivo);
    });
})();