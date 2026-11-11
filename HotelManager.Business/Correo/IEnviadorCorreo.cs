namespace HotelManager.Business.Correo
{
    /// <summary>Mensaje de correo listo para enviar.</summary>
    public class MensajeCorreo
    {
        public string Para { get; set; }
        public string NombrePara { get; set; }
        public string Asunto { get; set; }
        public string CuerpoHtml { get; set; }
        public string CuerpoTexto { get; set; }
    }

    public interface IEnviadorCorreo
    {
        ConfiguracionCorreo Configuracion { get; }

        /// <summary>Envia el mensaje. Lanza una excepcion si el envio falla.</summary>
        void Enviar(MensajeCorreo mensaje);
    }
}
