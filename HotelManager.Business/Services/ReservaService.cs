using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Business.Interfaces;
using HotelManager.Data.Interfaces;
using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;

namespace HotelManager.Business.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _reservas;
        private readonly IHabitacionRepository _habitaciones;
        private readonly IHuespedRepository _huespedes;
        private readonly ITarifaRepository _tarifas;
        private readonly IHistorialReservaRepository _historial;
        private readonly INotificacionService _notificaciones;

        public ReservaService(
            IReservaRepository reservas,
            IHabitacionRepository habitaciones,
            IHuespedRepository huespedes,
            ITarifaRepository tarifas,
            IHistorialReservaRepository historial,
            INotificacionService notificaciones)
        {
            _reservas = reservas;
            _habitaciones = habitaciones;
            _huespedes = huespedes;
            _tarifas = tarifas;
            _historial = historial;
            _notificaciones = notificaciones;
        }

       
        public ReservaDto Crear(CrearReservaDto dto)
        {
            return Registrar(dto, EstadoReserva.Confirmada);
        }

        /// <summary>
        /// ACT-003: el huesped solicita una reserva. Se valida igual que una reserva normal y
        /// se reserva la habitacion, pero queda Pendiente hasta que recepcion la confirma
        /// (al confirmarla se envia el correo de confirmacion).
        /// </summary>
        public ReservaDto Solicitar(CrearReservaDto dto)
        {
            return Registrar(dto, EstadoReserva.Pendiente);
        }

        private ReservaDto Registrar(CrearReservaDto dto, EstadoReserva estado)
        {
            ValidarFechas(dto.FechaEntrada, dto.FechaSalida);

            if (_huespedes.GetById(dto.HuespedId) == null)
                throw new BusinessException("El huésped no está registrado.");

            var habitacion = ObtenerHabitacionReservable(dto.HabitacionId, dto.CantidadHuespedes);

            if (_reservas.ExisteSolapamiento(dto.HabitacionId, dto.FechaEntrada, dto.FechaSalida))
                throw new BusinessException("La habitación ya tiene una reserva activa en esas fechas."); 

            var reserva = new Reserva
            {
                IdHuesped = dto.HuespedId,
                IdHabitacion = dto.HabitacionId,
                Entrada = dto.FechaEntrada.Date,
                Salida = dto.FechaSalida.Date,
                Huespedes = dto.CantidadHuespedes,
                CostoTotal = CalcularCosto(habitacion, dto.FechaEntrada, dto.FechaSalida),        
                Estado = estado
            };

            // El historial (creacion) lo registra el trigger tr_reservas_historial.
            reserva.IdReserva = _reservas.Add(reserva);
            if (estado == EstadoReserva.Confirmada)
                NotificarSinFallar(reserva.IdReserva, TipoNotificacion.Confirmacion);
            return Mapear(reserva);
        }

        
        public ReservaDto Modificar(ModificarReservaDto dto)
        {
            var reserva = _reservas.GetById(dto.ReservaId);
            if (reserva == null) throw new BusinessException("La reserva no existe.");
            if (reserva.Estado == EstadoReserva.Cancelada)
                throw new BusinessException("No se puede modificar una reserva cancelada.");

            ValidarFechas(dto.FechaEntrada, dto.FechaSalida);
            var habitacion = ObtenerHabitacionReservable(dto.HabitacionId, dto.CantidadHuespedes);

            if (_reservas.ExisteSolapamiento(dto.HabitacionId, dto.FechaEntrada, dto.FechaSalida, dto.ReservaId))
                throw new BusinessException("La habitación ya tiene una reserva activa en esas fechas.");

            reserva.IdHabitacion = dto.HabitacionId;
            reserva.Entrada = dto.FechaEntrada.Date;
            reserva.Salida = dto.FechaSalida.Date;
            reserva.Huespedes = dto.CantidadHuespedes;
            reserva.CostoTotal = CalcularCosto(habitacion, dto.FechaEntrada, dto.FechaSalida);
            reserva.Estado = EstadoReserva.Modificada;

            if (!_reservas.Update(reserva))
                throw new BusinessException("No se pudo actualizar la reserva.");

            NotificarSinFallar(reserva.IdReserva, TipoNotificacion.Modificacion);
            return Mapear(reserva);
        }

        // CU-009
        public void Cancelar(int idReserva)
        {
            var reserva = _reservas.GetById(idReserva);
            if (reserva == null) throw new BusinessException("La reserva no existe.");
            if (reserva.Estado == EstadoReserva.Cancelada)
                throw new BusinessException("La reserva ya está cancelada.");

            reserva.Estado = EstadoReserva.Cancelada;                                

            if (!_reservas.Update(reserva))
                throw new BusinessException("No se pudo cancelar la reserva.");

            NotificarSinFallar(reserva.IdReserva, TipoNotificacion.Cancelacion);
        }

        public ReservaDto ObtenerPorId(int idReserva)
        {
            var reserva = _reservas.GetById(idReserva);
            if (reserva == null) throw new BusinessException("La reserva no existe.");
            return Mapear(reserva);
        }

        // CU-010 / RF-013
        public IEnumerable<ReservaDto> ObtenerHistorialPorHuesped(int idHuesped)
        {
            return _reservas.GetByHuesped(idHuesped).Select(Mapear).ToList();
        }

        public IEnumerable<ReservaDto> Listar()
        {
            return _reservas.GetAll()
                .OrderByDescending(r => r.Entrada)
                .ThenByDescending(r => r.IdReserva)
                .Select(Mapear)
                .ToList();
        }

        // Pendiente -> Confirmada
        public void Confirmar(int idReserva)
        {
            var reserva = _reservas.GetById(idReserva);
            if (reserva == null) throw new BusinessException("La reserva no existe.");
            if (reserva.Estado != EstadoReserva.Pendiente)
                throw new BusinessException("Solo se pueden confirmar reservas pendientes.");

            reserva.Estado = EstadoReserva.Confirmada;
            if (!_reservas.Update(reserva))
                throw new BusinessException("No se pudo confirmar la reserva.");

            NotificarSinFallar(reserva.IdReserva, TipoNotificacion.Confirmacion);
        }

        // Check-out: una reserva activa pasa a Finalizada.
        public void Finalizar(int idReserva)
        {
            var reserva = _reservas.GetById(idReserva);
            if (reserva == null) throw new BusinessException("La reserva no existe.");
            if (reserva.Estado != EstadoReserva.Confirmada && reserva.Estado != EstadoReserva.Modificada)
                throw new BusinessException("Solo se pueden finalizar reservas confirmadas o modificadas.");
            if (reserva.Entrada.Date > DateTime.Today)
                throw new BusinessException("No se puede finalizar una reserva cuya estadía aún no ha comenzado.");

            reserva.Estado = EstadoReserva.Finalizada;
            if (!_reservas.Update(reserva))
                throw new BusinessException("No se pudo finalizar la reserva.");
        }

        public IEnumerable<ReservaDto> ListarActivas()
        {
            var hoy = DateTime.Today;
            return _reservas.GetAll()
                .Where(r => r.Estado == EstadoReserva.Pendiente
                         || r.Estado == EstadoReserva.Confirmada
                         || r.Estado == EstadoReserva.Modificada)
                .Where(r => r.Salida.Date > hoy)
                .OrderBy(r => r.Entrada)
                .Select(Mapear)
                .ToList();
        }

       

        private static void ValidarFechas(DateTime entrada, DateTime salida)
        {
            if (salida.Date <= entrada.Date)
                throw new BusinessException("La fecha de salida debe ser posterior a la de entrada.");  
            if (entrada.Date < DateTime.Today)
                throw new BusinessException("No se permiten reservas en fechas pasadas.");              
        }

        private Habitacion ObtenerHabitacionReservable(int idHabitacion, int huespedes)
        {
            var habitacion = _habitaciones.GetById(idHabitacion);
            if (habitacion == null) throw new BusinessException("La habitación no existe.");
            if (!EsReservable(habitacion))
                throw new BusinessException("La habitación no está disponible para reservas.");         
            if (huespedes <= 0 || huespedes > habitacion.Capacidad)
                throw new BusinessException("La cantidad de huéspedes excede la capacidad de la habitación."); 
            return habitacion;
        }

       
        private static bool EsReservable(Habitacion h)
        {
            return h.Estado == EstadoHabitacion.Disponible;
        }

        
        private decimal CalcularCosto(Habitacion habitacion, DateTime entrada, DateTime salida)
        {
            var tarifas = _tarifas.GetByHabitacion(habitacion.IdHabitacion).ToList();
            decimal total = 0;
            for (var noche = entrada.Date; noche < salida.Date; noche = noche.AddDays(1))
            {
                var fecha = noche;
                var tarifa = tarifas.FirstOrDefault(t => t.AplicaEn(fecha));
                total += tarifa != null ? tarifa.PrecioPorNoche : habitacion.Tarifa;
            }
            return total;
        }

        // El historial de reservas (creacion, modificacion, cancelacion) lo registra
        // automaticamente el trigger tr_reservas_historial de la base de datos.

        
        private void NotificarSinFallar(int idReserva, TipoNotificacion tipo)
        {
            try { _notificaciones.Notificar(idReserva, tipo); }
            catch (Exception ex)
            {
                Trace.TraceError("Fallo al notificar reserva " + idReserva + ": " + ex.Message);
            }
        }

        private static ReservaDto Mapear(Reserva r)
        {
            return new ReservaDto
            {
                Id = r.IdReserva,
                HuespedId = r.IdHuesped,
                HabitacionId = r.IdHabitacion,
                FechaEntrada = r.Entrada,
                FechaSalida = r.Salida,
                CantidadHuespedes = r.Huespedes,
                CostoTotal = r.CostoTotal,
                Estado = r.Estado.ToString()
            };
        }
    }
}