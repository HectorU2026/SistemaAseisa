using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresaController : Controller
    {
        private IEmpresaFlujo _empresaFlujo;

        public EmpresaController(IEmpresaFlujo empresaFlujo)
        {
            _empresaFlujo = empresaFlujo;
        }

        [HttpGet("ObtenerNombreEmpresa")]
        public async Task<IActionResult> ObtenerNombreEmpresa()
        {
            var empresa = await _empresaFlujo.ObtenerNombreEmpresa();
            return Ok(empresa);
        }

    }
}
