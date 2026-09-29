using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Sinerfin.Config;
using Sinerfin.Models;

namespace Sinerfin.Repositories
{
    public class MovimientoRepository
    {
        private readonly DbConnectionFactory _factory;

        public MovimientoRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public class CuentaInfo
        {
            public string  NumeroCuenta { get; set; }
            public string  Cedula       { get; set; }
            public string  Nombre       { get; set; }
            public decimal Saldo        { get; set; }
        }

        public string ObtenerOCrearCuenta(DbProvider p, string cedula, string nombre)
        {
            using (var conn = _factory.GetConnection(p))
            {
                var t = DbUtil.T(p, "cuentas");
                var existing = conn.QueryFirstOrDefault<string>(
                    "SELECT numero_cuenta FROM " + t + " WHERE cedula = @cedula",
                    new { cedula });

                if (existing != null) return existing;

                string num =
                    "10" + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 100_000_000L)
                              .ToString().PadLeft(8, '0');

                conn.Execute(
                    "INSERT INTO " + t + " (numero_cuenta, cedula, nombre, saldo) VALUES (@num, @ced, @nom, 0)",
                    new { num, ced = cedula, nom = nombre });

                return num;
            }
        }

        public CuentaInfo ObtenerCuentaPorCedula(DbProvider p, string cedula)
        {
            using (var conn = _factory.GetConnection(p))
            {
                var t   = DbUtil.T(p, "cuentas");
                var row = conn.QueryFirstOrDefault(
                    "SELECT numero_cuenta, nombre, saldo FROM " + t + " WHERE cedula = @cedula",
                    new { cedula });

                if (row == null) return null;
                return new CuentaInfo
                {
                    NumeroCuenta = Convert.ToString(row.numero_cuenta),
                    Cedula       = cedula,
                    Nombre       = (string)row.nombre,
                    Saldo        = (decimal)row.saldo
                };
            }
        }

        public void Guardar(DbProvider p, Movimiento m)
        {
            using (var conn = _factory.GetConnection(p))
            {
                var tC = DbUtil.T(p, "cuentas");
                var tM = DbUtil.T(p, "movimientos");

                using (var tx = conn.BeginTransaction(IsolationLevel.RepeatableRead))
                {
                    try
                    {
                        string qSaldo;
                        switch (p)
                        {
                            case DbProvider.SQLSERVER:
                            case DbProvider.SQLSERVER_DEV:
                                qSaldo = "SELECT saldo FROM cuentas WITH (UPDLOCK) WHERE numero_cuenta = @num";
                                break;
                            default:
                                qSaldo = "SELECT saldo FROM " + tC + " WHERE numero_cuenta = @num FOR UPDATE";
                                break;
                        }

                        var saldo = conn.QueryFirstOrDefault<decimal?>(qSaldo, new { num = m.NumeroCuenta }, tx);
                        if (saldo == null)
                            throw new InvalidOperationException("Cuenta no encontrada: " + m.NumeroCuenta);

                        var nuevo = m.TipoMovimiento == "DEPOSITO"
                            ? saldo.Value + m.Valor
                            : saldo.Value - m.Valor;

                        if (nuevo < 0)
                            throw new InvalidOperationException(
                                string.Format("Saldo insuficiente. Saldo actual: ${0:F2}", saldo.Value));

                        conn.Execute(
                            "UPDATE " + tC + " SET saldo = @saldo WHERE numero_cuenta = @num",
                            new { saldo = nuevo, num = m.NumeroCuenta }, tx);

                        conn.Execute(
                            "INSERT INTO " + tM + " (cedula, nombre, numero_cuenta, tipo_movimiento, valor) " +
                            "VALUES (@cedula, @nombre, @numeroCuenta, @tipo, @valor)",
                            new { cedula = m.Cedula, nombre = m.Nombre,
                                  numeroCuenta = m.NumeroCuenta, tipo = m.TipoMovimiento,
                                  valor = m.Valor }, tx);

                        tx.Commit();
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        public IEnumerable<Movimiento> BuscarPorCedula(DbProvider p, string cedula)
        {
            using (var conn = _factory.GetConnection(p))
            {
                var t = DbUtil.T(p, "movimientos");
                return conn.Query<Movimiento>(
                    "SELECT cedula AS Cedula, nombre AS Nombre, numero_cuenta AS NumeroCuenta, " +
                    "tipo_movimiento AS TipoMovimiento, valor AS Valor, fecha AS Fecha " +
                    "FROM " + t + " WHERE cedula = @cedula ORDER BY id DESC",
                    new { cedula });
            }
        }
    }
}
