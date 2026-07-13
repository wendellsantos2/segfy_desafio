using System;

namespace Sinistros.Domain.Exceptions
{
    public class RegraNegocioException : DomainException
    {
        public RegraNegocioException(string message) : base(message)
        {
        }

        public RegraNegocioException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
