using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace MH.Capstone.Domain.DataAccess
{
    // Picks the EF Core provider from config ("Database:Provider") so the app can run on
    // SQL Server or Postgres without code changes. Each provider has its own migrations
    // assembly (MH.Capstone.Migrations.<Provider>) generated from the same model.
    public static class DatabaseProviderExtensions
    {
        public const string ProviderConfigKey = "Database:Provider";
        public const string SqlServer = "SqlServer";
        public const string Postgres = "Postgres";

        public static string GetDatabaseProvider(this IConfiguration config)
            => config[ProviderConfigKey] ?? SqlServer;

        public static DbContextOptionsBuilder UseConfiguredDatabase(
            this DbContextOptionsBuilder options, IConfiguration config, string? connectionString,
            string historyTable, int? commandTimeoutSeconds = null)
        {
            var provider = config.GetDatabaseProvider();
            var migrationsAssembly = $"MH.Capstone.Migrations.{provider}";

            return provider switch
            {
                SqlServer => options.UseSqlServer(connectionString, sql =>
                {
                    sql.EnableRetryOnFailure()
                        .MigrationsHistoryTable(historyTable)
                        .MigrationsAssembly(migrationsAssembly);
                    if (commandTimeoutSeconds is { } t) sql.CommandTimeout(t);
                }),
                Postgres => options.UseNpgsql(connectionString, pg =>
                {
                    pg.EnableRetryOnFailure()
                        .MigrationsHistoryTable(historyTable)
                        .MigrationsAssembly(migrationsAssembly);
                    if (commandTimeoutSeconds is { } t) pg.CommandTimeout(t);
                }),
                _ => throw new InvalidOperationException(
                    $"Unknown {ProviderConfigKey} '{provider}'. Expected '{SqlServer}' or '{Postgres}'.")
            };
        }

        // Npgsql only accepts DateTimeOffset values with offset 0 for timestamptz columns,
        // so normalise to UTC on Postgres. SQL Server keeps the original offset.
        internal static void UseUtcDateTimeOffsetsOnPostgres(this ModelConfigurationBuilder builder, DbContext context)
        {
            if (!context.Database.IsNpgsql()) return;
            builder.Properties<DateTimeOffset>().HaveConversion<UtcDateTimeOffsetConverter>();
            builder.Properties<DateTimeOffset?>().HaveConversion<UtcDateTimeOffsetConverter>();
        }

        private sealed class UtcDateTimeOffsetConverter()
            : ValueConverter<DateTimeOffset, DateTimeOffset>(v => v.ToUniversalTime(), v => v);
    }
}
