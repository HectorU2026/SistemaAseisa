using Abstracciones.Modelos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abstracciones.Interfaces.DA
{
    public interface IBitacoraDA
    {
        Task RegistrarBitacora(Bitacora bitacora);
    }
}
