using System;
using System.Collections.Generic;
using Sinistros.Domain.Enums;
using Sinistros.Domain.Exceptions;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros.Events;

namespace Sinistros.Domain.Sinistros
{
    public class Sinistro : AggregateRoot
    {
        private readonly List<HistoricoSinistro> _historicoSinistros = new();

        public Guid ApoliceId { get; private set; }
        public DateTime DataOcorrencia { get; private set; }
        public DateTime DataAbertura { get; private set; }
        public string Descricao { get; private set; }
        public Dinheiro ValorEstimado { get; private set; }
        private decimal? _valorAprovadoQuantia;
        private string? _valorAprovadoMoeda;
        private string? _motivoNegativaTexto;

        public Dinheiro? ValorAprovado 
        {
            get => _valorAprovadoQuantia.HasValue ? new Dinheiro(_valorAprovadoQuantia.Value, _valorAprovadoMoeda ?? "BRL") : null;
            private set 
            {
                _valorAprovadoQuantia = value?.Valor;
                _valorAprovadoMoeda = value?.Moeda;
            }
        }

        public MotivoNegativa? Motivo 
        {
            get => string.IsNullOrEmpty(_motivoNegativaTexto) ? null : new MotivoNegativa(_motivoNegativaTexto);
            private set
            {
                _motivoNegativaTexto = value?.Texto;
            }
        }
        public StatusSinistro Status { get; private set; }
        public DateTime? DataEncerramento { get; private set; }

        public IReadOnlyCollection<HistoricoSinistro> HistoricoSinistros => _historicoSinistros.AsReadOnly();

        private static readonly Dictionary<StatusSinistro, List<StatusSinistro>> TransicoesPermitidas = new()
        {
            { StatusSinistro.Aberto, new() { StatusSinistro.EmAnalise, StatusSinistro.Negado } },
            { StatusSinistro.EmAnalise, new() { StatusSinistro.Aprovado, StatusSinistro.Negado } },
            { StatusSinistro.Aprovado, new() { StatusSinistro.Encerrado } },
            { StatusSinistro.Negado, new() },
            { StatusSinistro.Encerrado, new() }
        };

        private Sinistro()
        {
            Descricao = null!;
            ValorEstimado = null!;
        }

        private Sinistro(Guid apoliceId, DateTime dataOcorrencia, string descricao, Dinheiro valorEstimado, string usuario)
        {
            if (apoliceId == Guid.Empty)
                throw new RegraNegocioException("O ApoliceId é obrigatório.");

            if (dataOcorrencia > DateTime.UtcNow)
                throw new RegraNegocioException("A data de ocorrência não pode ser no futuro.");

            if (string.IsNullOrWhiteSpace(descricao))
                throw new RegraNegocioException("A descrição do sinistro é obrigatória.");

            ApoliceId = apoliceId;
            DataOcorrencia = dataOcorrencia;
            DataAbertura = DateTime.UtcNow;
            Descricao = descricao.Trim();
            ValorEstimado = valorEstimado ?? throw new RegraNegocioException("O valor estimado é obrigatório.");
            
            Status = StatusSinistro.Aberto;
            _historicoSinistros.Add(new HistoricoSinistro(Id, null, StatusSinistro.Aberto, "Abertura do sinistro", usuario));
            AdicionarEvento(new SinistroAbertoEvent(Id, ApoliceId));
        }

        public static Sinistro Abrir(Guid apoliceId, DateTime dataOcorrencia, string descricao, Dinheiro valorEstimado, string usuario = "Sistema")
        {
            if (string.IsNullOrWhiteSpace(usuario))
                throw new RegraNegocioException("O usuário é obrigatório.");

            return new Sinistro(apoliceId, dataOcorrencia, descricao, valorEstimado, usuario);
        }

        public void EnviarParaAnalise(string usuario)
        {
            AplicarTransicao(StatusSinistro.EmAnalise, "Envio para análise", usuario);
        }

        public void Aprovar(string usuario)
        {
            AplicarTransicao(StatusSinistro.Aprovado, "Sinistro aprovado", usuario);
        }

        public void Negar(MotivoNegativa motivo, string usuario)
        {
            if (motivo == null)
                throw new RegraNegocioException("O motivo da negativa é obrigatório.");

            Motivo = motivo;
            AplicarTransicao(StatusSinistro.Negado, motivo.Texto, usuario);
        }

        public void Encerrar(Dinheiro valorAprovado, string usuario)
        {
            if (valorAprovado == null)
                throw new RegraNegocioException("O valor aprovado é obrigatório.");

            if (valorAprovado.Valor <= 0)
                throw new RegraNegocioException("O valor aprovado deve ser maior que zero.");

            if (valorAprovado.Moeda != ValorEstimado.Moeda)
                throw new RegraNegocioException("A moeda do valor aprovado deve ser a mesma do valor estimado.");

            ValorAprovado = valorAprovado;
            DataEncerramento = DateTime.UtcNow;

            AplicarTransicao(StatusSinistro.Encerrado, "Sinistro encerrado", usuario);
        }

        private void AplicarTransicao(StatusSinistro novoStatus, string? motivo, string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                throw new RegraNegocioException("O usuário é obrigatório.");

            if (!TransicoesPermitidas.ContainsKey(Status) || !TransicoesPermitidas[Status].Contains(novoStatus))
            {
                throw new RegraNegocioException($"Transição inválida de {Status} para {novoStatus}.");
            }

            var statusAnterior = Status;
            Status = novoStatus;

            _historicoSinistros.Add(new HistoricoSinistro(Id, statusAnterior, novoStatus, motivo, usuario));

            IDomainEvent evento = novoStatus switch
            {
                StatusSinistro.EmAnalise => new SinistroEnviadoParaAnaliseEvent(Id, ApoliceId),
                StatusSinistro.Aprovado => new SinistroAprovadoEvent(Id, ApoliceId),
                StatusSinistro.Negado => new SinistroNegadoEvent(Id, ApoliceId, Motivo ?? throw new InvalidOperationException("Motivo da negativa não informado.")),
                StatusSinistro.Encerrado => new SinistroEncerradoEvent(Id, ApoliceId, ValorAprovado ?? throw new InvalidOperationException("Valor aprovado não informado.")),
                _ => throw new InvalidOperationException($"Nenhum evento configurado para o status {novoStatus}.")
            };

            AdicionarEvento(evento);
        }
    }
}
