using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelManager.Business.Dtos;
using HotelManager.Business.Exceptions;
using HotelManager.Web.Helpers;

namespace HotelManager.Web.Controllers
{
    /// <summary>
    /// Funcionalidad comun de la capa de presentacion: mensajes al usuario,
    /// manejo de excepciones de negocio y armado de listas desplegables.
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected void Exito(string mensaje) { TempData["Exito"] = mensaje; }
        protected void Error(string mensaje) { TempData["Error"] = mensaje; }

        /// <summary>
        /// Ejecuta una operacion de negocio. Si viola una regla (BusinessException)
        /// agrega el mensaje al ModelState y devuelve false.
        /// </summary>
        protected bool Ejecutar(Action accion)
        {
            try
            {
                accion();
                return true;
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return false;
            }
            catch (System.Data.SqlClient.SqlException ex) when (EsReglaDeBaseDatos(ex))
            {
                ModelState.AddModelError(string.Empty, MensajeBaseDatos(ex));
                return false;
            }
        }

        /// <summary>
        /// Los triggers de la base (raiserror con severidad 16) devuelven el error 50000:
        /// son reglas de negocio y se muestran al usuario como tales.
        /// </summary>
        protected static bool EsReglaDeBaseDatos(System.Data.SqlClient.SqlException ex)
        {
            return ex.Number == 50000 || ex.Number == 2627 || ex.Number == 2601 || ex.Number == 547;
        }

        protected static string MensajeBaseDatos(System.Data.SqlClient.SqlException ex)
        {
            switch (ex.Number)
            {
                case 2627:
                case 2601: return "Ya existe un registro con esos datos (valor duplicado).";
                case 547: return "La operación no es válida: el registro está relacionado con otros datos o no cumple una restricción.";
                default:
                    var msg = ex.Errors.Count > 0 ? ex.Errors[0].Message : ex.Message;
                    return msg.Length > 0 ? char.ToUpper(msg[0]) + msg.Substring(1) : msg;
            }
        }

        /// <summary>Igual que Ejecutar, pero muestra el error como mensaje temporal (para acciones POST rapidas).</summary>
        protected void EjecutarConMensaje(Action accion, string mensajeExito)
        {
            try
            {
                accion();
                Exito(mensajeExito);
            }
            catch (BusinessException ex)
            {
                Error(ex.Message);
            }
            catch (System.Data.SqlClient.SqlException ex) when (EsReglaDeBaseDatos(ex))
            {
                Error(MensajeBaseDatos(ex));
            }
        }

        protected static bool EsActivo(string estado)
        {
            return string.Equals(estado, "Activo", StringComparison.OrdinalIgnoreCase);
        }

        protected static SelectList HotelesSelect(IEnumerable<HotelDto> hoteles, int? seleccionado, bool soloActivos)
        {
            var items = hoteles
                .Where(h => !soloActivos || EsActivo(h.Estado) || h.IdHotel == seleccionado)
                .OrderBy(h => h.Nombre)
                .Select(h => new { h.IdHotel, Nombre = TextoHelper.Titulo(h.Nombre) });
            return new SelectList(items, "IdHotel", "Nombre", seleccionado);
        }

        protected static string EtiquetaPiso(PisoDto piso, IDictionary<int, BloqueDto> bloques)
        {
            BloqueDto bloque;
            if (piso.IdBloque.HasValue && bloques.TryGetValue(piso.IdBloque.Value, out bloque))
                return TextoHelper.Titulo(bloque.Nombre) + " - Piso " + piso.Numero;
            return "Piso " + piso.Numero;
        }
    }
}
