using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.Reglas
{
    public interface IAutenticacionBC
    {
        Task<Token> LoginAync(LoginBase login);
    }
}
