using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reglas;

namespace Web.Pages.Seguridad
{
    public class RegistroModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public Usuario Usuario { get; set; } = default!;

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        public RegistroModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPost()
        {
            var hash = Autenticacion.GenerarHash(Usuario.ContrasenaHash);
            Usuario.ContrasenaHash = Autenticacion.ObtenerHash(hash);

            Usuario.IdEmpresa = 1;
            Usuario.IdEstado = 1;
            Usuario.TipoCliente = "Persona física";

            Cliente.IdEmpresa = 1;
            Cliente.IdEstado = 1;
            Cliente.TipoCliente = "Persona física";
            Cliente.NombreRazonSocial = $"{Usuario.Nombre} {Usuario.PrimerApellido}";
            if (ModelState.IsValid)
            {
                var request = new
                {
                    Usuario = Usuario,
                    Cliente = Cliente
                };

                string endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "Registro");
                var client = new HttpClient();
                var respuesta = await client.PostAsJsonAsync(endpoint, request);

                if (respuesta.IsSuccessStatusCode)
                    return Redirect("/Seguridad/Login");

            }
            ModelState.AddModelError("", "Asegúrese de rellenar todos los campos obligatorios");
            return Page();
        }
    }
}
