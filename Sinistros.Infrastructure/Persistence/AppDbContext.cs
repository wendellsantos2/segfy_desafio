using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sinistros.Domain.Apolices;
using Sinistros.Domain.Clientes;
using Sinistros.Domain.SeedWork;
using Sinistros.Domain.Sinistros;

namespace Sinistros.Infrastructure.Persistence
{
    public class AppDbContext : DbContext, IUnitOfWork
    {
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Apolice> Apolices { get; set; }
        public DbSet<Sinistro> Sinistros { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            return await SaveChangesAsync(cancellationToken) > 0;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<HistoricoSinistro>(h =>
            {
                h.ToTable("HistoricoSinistros");
                h.HasKey(x => x.Id);
                h.Property(x => x.StatusAnterior).HasConversion<string>().HasMaxLength(20);
                h.Property(x => x.StatusNovo).HasConversion<string>().HasMaxLength(20).IsRequired();
                h.Property(x => x.Motivo).HasMaxLength(500);
                h.Property(x => x.Usuario).IsRequired().HasMaxLength(100);
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(AggregateRoot).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).Ignore(nameof(AggregateRoot.DomainEvents));
                }
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    var columnName = ConvertToSnakeCase(property.Name);
                    property.SetColumnName(columnName);

                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        if (property.GetColumnType() == null)
                        {
                            property.SetColumnType("decimal(18,2)");
                        }
                    }
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(ConvertToSnakeCase(key.GetName() ?? ""));
                }

                foreach (var fk in entity.GetForeignKeys())
                {
                    fk.SetConstraintName(ConvertToSnakeCase(fk.GetConstraintName() ?? ""));
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(ConvertToSnakeCase(index.GetDatabaseName() ?? ""));
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        private static string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var startUnderscore = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2");
            return startUnderscore.ToLowerInvariant();
        }
    }
}
