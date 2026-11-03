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
    }
}