using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.HotelManager.Domain.Enums
{
    public enum EstadoHabitacion
    {
        Disponible = 1,
        Ocupada = 2,
        Mantenimiento = 3
    }

    public enum EstadoReserva
    {
        Pendiente = 1,
        Confirmada = 2,
        Cancelada = 3,
        Finalizada = 4
    }
}
