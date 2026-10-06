using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Reglas;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Web.Pages.Seguridad
{
    public class RegistroModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        [BindProperty]
        public Usuario Usuario { get; set; } = default!;

        [BindProperty]
        public Cliente Cliente { get; set; } = default!;

        public List<EmpresaResponseNombres> Empresas { get; set; } = new();
        [Required(ErrorMessage = "Debe seleccionar una empresa")]
        public int? IdEmpresa { get; set; }
        public RegistroResponse registro { get; set; }
        public RegistroModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        public async Task OnGet() 
        {
            await CargarEmpresas();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var hash = Autenticacion.GenerarHash(Usuario.ContrasenaHash);
                Usuario.ContrasenaHash = Autenticacion.ObtenerHash(hash);
                Usuario.IdEstado = 1;

                Cliente.IdEmpresa = Usuario.IdEmpresa;
                Cliente.IdEstado = 1;
                Cliente.Correo = Usuario.Correo;
                Cliente.TipoCliente = Usuario.TipoCliente;
                Cliente.NombreRazonSocial = $"{Usuario.Nombre} {Usuario.PrimerApellido}";

                var request = new
                {
                    Usuario = Usuario,
                    Cliente = Cliente
                };

                string endpoint = _configuracion.ObtenerMetodo("ApiEndPointsSeguridad", "Registro");
                var client = new HttpClient();
                var respuesta = await client.PostAsJsonAsync(endpoint, request);

                var contenido = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                registro = JsonSerializer.Deserialize<RegistroResponse>(contenido, opciones);

                if (respuesta.IsSuccessStatusCode && registro.IdUsuario != null )
                {
                    Usuario.IdUsuario = registro.IdUsuario;
                    await RegistrarBitacora(Usuario, Cliente);
                    return RedirectToPage("/Seguridad/Login");
                }
                else { 
                    ModelState.AddModelError("", registro?.mensaje);
                    return Page();
                }
            }
            ModelState.AddModelError("", "Asegúrese de rellenar todos los campos obligatorios");
            return Page();
        }

        public async Task CargarEmpresas()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerNombreEmpresa");
            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            var resultado = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true };
            Empresas = JsonSerializer.Deserialize<List<EmpresaResponseNombres>>
                (resultado, opciones);
        }

        private async Task RegistrarBitacora(Usuario usuario, Cliente cliente)
        {
            var bitacora = new Bitacora
            {
                IdUsuario = usuario.IdUsuario.Value,
                IdEmpresa = usuario.IdEmpresa.Value,
                TablaAfectada = "Usuario, Cliente",
                Accion = "RegistrarUsuarioCliente",
                RegistroId = usuario.IdUsuario.ToString(),
                ValorAnterior = (string?)null,
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    usuario = new
                    {
                        usuario.IdEmpresa,
                        usuario.Nombre,
                        usuario.PrimerApellido,
                        usuario.SegundoApellido,
                        usuario.Correo,
                        usuario.NombreUsuario,
                        usuario.IdEstado
                    },
                    cliente = new
                    {
                        cliente.IdEmpresa,
                        cliente.TipoCliente,
                        cliente.NombreRazonSocial,
                        cliente.Identificacion,
                        cliente.Correo,
                        cliente.Telefono,
                        cliente.Direccion,
                        cliente.IdEstado
                    },
                    fecha = DateTime.UtcNow
                })
            };

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "RegistrarBitacora");
            var client = new HttpClient();
            var respuesta = await client.PostAsJsonAsync(endpoint, bitacora);

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadAsStringAsync();
                Console.WriteLine($"Error bitácora: {error}");
            }
        }
    }
}
