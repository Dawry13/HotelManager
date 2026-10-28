using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using HotelManager.Data.Base;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;

namespace HotelManager.Data.Repositories
{
    public class TarifaRepository : BaseRepository<Tarifa>, ITarifaRepository
    {
        protected override string TableName => "tarifas";
        protected override string KeyColumn => "id_tarifa";

        protected override Tarifa Map(IDataRecord r) => new Tarifa
        {
            IdTarifa = (int)r["id_tarifa"],
            IdHabitacion = (int)r["id_habitacion"],
            PrecioPorNoche = (decimal)r["valor_noche"],
            FechaInicio = (DateTime)r["vigencia_desde"],
            FechaFin = r["vigencia_hasta"] as DateTime?
        };

        public override int Add(Tarifa entity)
        {
            var id = Scalar(
                @"INSERT INTO tarifas (id_habitacion, valor_noche, vigencia_desde, vigencia_hasta)
                  VALUES (@idHabitacion, @valor, @desde, @hasta);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new SqlParameter("@idHabitacion", entity.IdHabitacion),
                new SqlParameter("@valor", entity.PrecioPorNoche),
                new SqlParameter("@desde", entity.FechaInicio.Date),
                new SqlParameter("@hasta", DbVal(entity.FechaFin?.Date)));
            return Convert.ToInt32(id);
        }

        public override bool Update(Tarifa entity)
            => Execute(
                @"UPDATE tarifas
                  SET id_habitacion = @idHabitacion, valor_noche = @valor,
                      vigencia_desde = @desde, vigencia_hasta = @hasta
                  WHERE id_tarifa = @id",
                new SqlParameter("@id", entity.IdTarifa),
                new SqlParameter("@idHabitacion", entity.IdHabitacion),
                new SqlParameter("@valor", entity.PrecioPorNoche),
                new SqlParameter("@desde", entity.FechaInicio.Date),
                new SqlParameter("@hasta", DbVal(entity.FechaFin?.Date))) > 0;

        // Misma regla que fn_calcular_costo (RN-013). Devuelve null si no hay tarifa vigente;
        // en ese caso quien llame debe usar habitaciones.tarifa (tarifa base).
        public Tarifa GetVigente(int idHabitacion, DateTime fecha)
        {
            return Query(
                @"SELECT TOP 1 * FROM tarifas
                  WHERE id_habitacion = @idHabitacion
                    AND vigencia_desde <= @fecha
                    AND (vigencia_hasta IS NULL OR vigencia_hasta >= @fecha)
                  ORDER BY vigencia_desde DESC",
                new SqlParameter("@idHabitacion", idHabitacion),
                new SqlParameter("@fecha", fecha.Date))
                .FirstOrDefault();
        }

        public IEnumerable<Tarifa> GetByHabitacion(int idHabitacion)
        {
            return Query(
                @"SELECT * FROM tarifas
                  WHERE id_habitacion = @idHabitacion
                  ORDER BY vigencia_desde DESC",
                new SqlParameter("@idHabitacion", idHabitacion));
        }
    }
}