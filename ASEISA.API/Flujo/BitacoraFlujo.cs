using Abstracciones.Interfaces.DA;
using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flujo
{
    public class BitacoraFlujo
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
