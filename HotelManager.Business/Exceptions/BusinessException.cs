using System;

namespace HotelManager.Business.Exceptions
{
    /// <summary>Se lanza cuando se viola una regla de negocio (RN-xxx).</summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}