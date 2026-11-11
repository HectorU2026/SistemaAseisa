namespace Abstracciones.Modelos
{
    public class CuentaBancariaRequest
    {
        public int IdEmpresa { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public int IdTipoCuenta { get; set; }
        public string Moneda { get; set; }
        public decimal SaldoActual { get; set; }
        public int IdCuentaContable { get; set; }
    }

    public class CuentaBancariaResponse
    {
        public int IdCuentaBancaria { get; set; }
        public int IdEmpresa { get; set; }
        public string NumeroCuenta { get; set; } = string.Empty;
        public int IdTipoCuenta { get; set; }
        public string TipoCuenta { get; set; } = string.Empty; 
        public string Moneda { get; set; } = string.Empty;
        public decimal SaldoActual { get; set; }
        public int IdCuentaContable { get; set; }
        public string NombreCuentaContable { get; set; } = string.Empty; 
        public int IdEstado { get; set; }
    }

    public class TipoCuentaResponse
    {
        public int IdTipoCuenta { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}