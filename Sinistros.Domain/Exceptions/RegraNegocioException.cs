using System;

namespace Sinistros.Domain.Exceptions
{
    public class RegraNegocioException : Exception
    {
        public RegraNegocioException(string message) : base(message)
        {
        }

        public RegraNegocioException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
