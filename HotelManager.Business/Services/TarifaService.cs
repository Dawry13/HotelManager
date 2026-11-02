using System;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;

namespace HotelManager.Business.Services
{
    public class TarifaService : ITarifaService
    {
        private readonly ITarifaRepository _tarifaRepository;
        private readonly IHabitacionRepository _habitacionRepository;

        public TarifaService(ITarifaRepository tarifaRepository, IHabitacionRepository habitacionRepository)
        {
            _tarifaRepository = tarifaRepository;
            _habitacionRepository = habitacionRepository;
        }

        public decimal ObtenerTarifaAplicable(int idHabitacion, DateTime fecha)
        {
            var habitacion = _habitacionRepository.GetById(idHabitacion);
            if (habitacion == null)
                throw new BusinessException("La habitación no existe.");

            // Tarifa vigente para esa fecha; si no hay, se usa la tarifa base de la habitación
            var tarifa = _tarifaRepository.GetVigente(idHabitacion, fecha.Date);
            return tarifa != null ? tarifa.PrecioPorNoche : habitacion.Tarifa;
        }

        public decimal CalcularCostoTotal(int idHabitacion, DateTime entrada, DateTime salida)
        {
            if (salida.Date <= entrada.Date)
                throw new BusinessException("La fecha de salida debe ser posterior a la de entrada.");

            decimal total = 0m;
            for (var noche = entrada.Date; noche < salida.Date; noche = noche.AddDays(1))
                total += ObtenerTarifaAplicable(idHabitacion, noche);

            return total;
        }
    }
}