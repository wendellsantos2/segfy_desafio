using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Clientes;
using Sinistros.Domain.Enums;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros;

namespace Sinistros.Infrastructure.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (context.Clientes.Any())
                return; // Já populado

            var randomSeed = new Random(42);

            // 1. Criar 10 Clientes
            var nomes = new[]
            {
                "Wendell Santos", "Arthur Pendragon", "Guinevere Slytherin", "Lancelot Du Lac", "Merlin Ambrosius",
                "Galahad Pure", "Tristan Lyonesse", "Isolde Ireland", "Percival Wales", "Gawain Orkney"
            };

            var clientes = new List<Cliente>();
            for (int i = 0; i < 10; i++)
            {
                var cpf = GerarCpfValido(randomSeed);
                var cliente = Cliente.Criar(nomes[i], new Documento(cpf));
                clientes.Add(cliente);
            }
            await context.Clientes.AddRangeAsync(clientes);

            // 2. Criar 20 Apólices (misturando status e ramos)
            var ramos = new[] { Ramo.Auto, Ramo.Vida, Ramo.Residencial, Ramo.Saude, Ramo.Empresarial };
            var apolices = new List<Apolice>();

            for (int i = 0; i < 20; i++)
            {
                var cliente = clientes[i % 10];
                var ramo = ramos[i % 5];
                var numeroStr = $"2026-{(i + 1):D6}";
                
                var vigencia = new PeriodoVigencia(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddMonths(6));
                var apolice = Apolice.Emitir(new NumeroApolice(numeroStr), cliente.Id, ramo, vigencia);

                if (i >= 12 && i < 16)
                {
                    apolice.Suspender();
                }
                else if (i >= 16)
                {
                    apolice.Cancelar();
                }

                apolices.Add(apolice);
            }
            await context.Apolices.AddRangeAsync(apolices);

            // 3. Criar ~60 Sinistros
            var sinistros = new List<Sinistro>();
            var random = new Random(42);

            for (int i = 0; i < 60; i++)
            {
                var apolice = apolices[i % apolices.Count];

                var mesesAtras = random.Next(0, 8);
                var diasAtras = random.Next(1, 28);
                var dataAbertura = DateTime.UtcNow.AddMonths(-mesesAtras).AddDays(-diasAtras);
                var dataOcorrencia = dataAbertura.AddDays(-random.Next(1, 5));

                var valorEstimado = new Dinheiro(random.Next(1000, 15000));
                var descricao = $"Sinistro de teste referente ao sinistro número {i + 1} ocorrendo no ramo {apolice.Ramo}.";

                var sinistro = Sinistro.Abrir(apolice.Id, dataOcorrencia, descricao, valorEstimado);

                SetPrivateField(sinistro, "<DataAbertura>k__BackingField", dataAbertura);

                string usuario = "Analista.Seed";

                if (i >= 15 && i < 30)
                {
                    sinistro.EnviarParaAnalise(usuario);
                }
                else if (i >= 30 && i < 45)
                {
                    sinistro.EnviarParaAnalise(usuario);
                    sinistro.Aprovar(usuario);
                }
                else if (i >= 45 && i < 53)
                {
                    sinistro.EnviarParaAnalise(usuario);
                    var motivo = new MotivoNegativa($"Recusa por falta de cobertura para a ocorrência do tipo {i}.");
                    sinistro.Negar(motivo, usuario);
                }
                else if (i >= 53)
                {
                    sinistro.EnviarParaAnalise(usuario);
                    sinistro.Aprovar(usuario);
                    
                    var valorAprovado = new Dinheiro(valorEstimado.Valor * (decimal)(0.8 + random.NextDouble() * 0.2));
                    sinistro.Encerrar(valorAprovado, usuario);

                    var dataEncerramento = dataAbertura.AddDays(random.Next(5, 15));
                    SetPrivateField(sinistro, "<DataEncerramento>k__BackingField", dataEncerramento);
                }

                foreach (var hist in sinistro.HistoricoSinistros)
                {
                    SetPrivateField(hist, "<DataAlteracao>k__BackingField", dataAbertura.AddMinutes(random.Next(5, 60)));
                }

                sinistros.Add(sinistro);
            }

            await context.Sinistros.AddRangeAsync(sinistros);
            
            await context.SaveChangesAsync();
        }

        private static string GerarCpfValido(Random random)
        {
            var n = new int[9];
            for (int i = 0; i < 9; i++)
            {
                n[i] = random.Next(0, 10);
            }

            var soma1 = 0;
            for (int i = 0; i < 9; i++)
            {
                soma1 += n[i] * (10 - i);
            }
            var resto1 = soma1 % 11;
            var d1 = resto1 < 2 ? 0 : 11 - resto1;

            var temp = n.ToList();
            temp.Add(d1);

            var soma2 = 0;
            for (int i = 0; i < 10; i++)
            {
                soma2 += temp[i] * (11 - i);
            }
            var resto2 = soma2 % 11;
            var d2 = resto2 < 2 ? 0 : 11 - resto2;
            temp.Add(d2);

            return string.Join("", temp);
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                var baseType = obj.GetType().BaseType;
                while (baseType != null)
                {
                    field = baseType.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (field != null)
                    {
                        field.SetValue(obj, value);
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }
    }
}
