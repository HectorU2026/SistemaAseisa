using Abstracciones.Interfaces.DA;
using Abstracciones.Interfaces.Flujo;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class BitacoraFlujo : IBitacoraFlujo
    {
        private IBitacoraDA _bitacoraDA;

        public BitacoraFlujo(IBitacoraDA bitacoraDA)
        {
            _bitacoraDA = bitacoraDA;
        }

        public async Task RegistrarBitacora(Bitacora bitacora) { 
            await _bitacoraDA.RegistrarBitacora(bitacora);
        }
    }
}
