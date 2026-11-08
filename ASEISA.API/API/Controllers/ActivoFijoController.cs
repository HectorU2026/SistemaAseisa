using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivoFijoController : Controller
    {
        private IActivoFijoFlujo _activoFijoFlujo;

        public ActivoFijoController(IActivoFijoFlujo activoFijoFlujo)
        {
            _activoFijoFlujo = activoFijoFlujo;
        }

        [HttpPost("RegistrarActivoFijo")]
        public async Task<ActionResult> RegistrarActivoFijo(ActivoFijoRequest activo)
        {
            var resultado = await _activoFijoFlujo.RegistrarActivoFijo(activo);

            if (resultado == 0)
            {
                return BadRequest("Complete todos los campos obligatorios primero.");
            }

            if (resultado == 2)
            {
                return BadRequest("El costo, el valor residual y la vida útil deben ser valores positivos.");
            }

            if (resultado == 4)
            {
                return BadRequest("Las cuentas contables no existen o no pertenecen a la empresa.");
            }

            if (resultado != 1)
            {
                return BadRequest("Ocurrió un error al registrar el activo.");
            }

            return Ok("Activo registrado con éxito.");
        }

        [HttpPost("CalcularDepreciacion")]
        public async Task<ActionResult> CalcularDepreciacion(CalcularDepreciacionRequest request)
        {
            var resultado = await _activoFijoFlujo.CalcularDepreciacionActivo(request);

            if (resultado == 0)
            {
                return BadRequest("Complete todos los campos obligatorios primero.");
            }
            if (resultado == 3)
            {
                return NotFound("El activo no existe o no pertenece a la empresa.");
            }
            if (resultado == 4)
            {
                return BadRequest("El activo tiene una vida útil inválida para ser depreciado.");
            }
            if (resultado == 5)
            {
                return BadRequest("La depreciación de este activo para este periodo ya fue calculada previamente.");
            }
            if (resultado == 6)
            {
                return BadRequest("El activo ya se encuentra totalmente depreciado hasta su valor residual.");
            }
            if (resultado != 1)
            {
                return BadRequest("Ocurrió un error al calcular la depreciación.");
            }

            return Ok("Depreciación calculada con éxito.");
        }

        [HttpGet("ObtenerHistorialDepreciacion/empresa/{idEmpresa}/activo/{idActivoFijo}")]
        public async Task<ActionResult<IEnumerable<DepreciacionHistorialResponse>>> ObtenerHistorialDepreciacion(int idEmpresa, int idActivoFijo)
        {
            if (idEmpresa <= 0 || idActivoFijo <= 0)
            {
                return BadRequest("Los identificadores de empresa y activo fijo son inválidos.");
            }

            var resultado = await _activoFijoFlujo.ObtenerHistorialDepreciacion(idEmpresa, idActivoFijo);

            return Ok(resultado);
        }
    }
}