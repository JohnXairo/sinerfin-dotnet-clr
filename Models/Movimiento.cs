using System;

namespace Sinerfin.Models
{
    public class Movimiento
    {
        public string   Cedula         { get; set; }
        public string   Nombre         { get; set; }
        public long     NumeroCuenta   { get; set; }
        public string   TipoMovimiento { get; set; }
        public decimal  Valor          { get; set; }
        public DateTime? Fecha         { get; set; }
    }
}
