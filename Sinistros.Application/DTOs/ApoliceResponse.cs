using System;
using Sinistros.Domain.Apolices;

namespace Sinistros.Application.DTOs
{
    public class ApoliceResponse
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public Guid ClienteId { get; set; }
        public string Ramo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }

        public static ApoliceResponse Mapear(Apolice apolice)
        {
            return new ApoliceResponse
            {
                Id = apolice.Id,
                Numero = apolice.Numero.Valor,
                ClienteId = apolice.ClienteId,
                Ramo = apolice.Ramo.ToString(),
                Status = apolice.Status.ToString(),
                Inicio = apolice.Vigencia.Inicio,
                Fim = apolice.Vigencia.Fim
            };
        }
    }
}
