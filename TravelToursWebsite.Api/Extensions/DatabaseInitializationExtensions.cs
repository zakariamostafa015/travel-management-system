using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Features.Auth;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Api.Extensions;

internal static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        var migrateOnStartup = app.Configuration.GetValue<bool>("Database:MigrateOnStartup");
        var seedOnStartup = app.Configuration.GetValue<bool>("Database:SeedOnStartup");
        var recreateWhenSchemaMissing = app.Configuration.GetValue<bool>("Database:RecreateWhenSchemaMissing");

        if (!migrateOnStartup && !seedOnStartup)
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (migrateOnStartup)
        {
            if (await BaseSchemaExistsAsync(context))
            {
                if (!await MigrationHistoryHasRowsAsync(context))
                {
                    await EnsureMigrationHistoryBaselineAsync(context, leaveLatestMigrationPending: true);
                }

                await context.Database.MigrateAsync();
            }
            else
            {
                if (!recreateWhenSchemaMissing && await DatabaseHasAnyUserTablesAsync(context))
                {
                    throw new InvalidOperationException(
                        "The database exists but the base TravelToursWebsite schema is missing. " +
                        "Enable Database:RecreateWhenSchemaMissing for local Docker development, or restore/apply the initial schema manually.");
                }

                if (recreateWhenSchemaMissing && await DatabaseHasAnyUserTablesAsync(context))
                {
                    await context.Database.EnsureDeletedAsync();
                }

                await context.Database.MigrateAsync();
            }
        }

        if (seedOnStartup)
        {
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            await DatabaseSeeder.SeedAsync(
                context,
                passwordHasher,
                CreateAdminSeedOptions(app.Configuration));
        }
    }

    private static async Task<bool> BaseSchemaExistsAsync(ApplicationDbContext context)
    {
        return await TableExistsAsync(context, "Languages");
    }

    private static async Task<bool> DatabaseHasAnyUserTablesAsync(ApplicationDbContext context)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME <> '__EFMigrationsHistory'";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
    private static async Task<bool> MigrationHistoryHasRowsAsync(ApplicationDbContext context)
    {
        if (!await TableExistsAsync(context, "__EFMigrationsHistory"))
        {
            return false;
        }

        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM [dbo].[__EFMigrationsHistory]";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<bool> TableExistsAsync(ApplicationDbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;

        if (shouldClose)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            command.CommandText = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @tableName";
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task EnsureMigrationHistoryBaselineAsync(ApplicationDbContext context, bool leaveLatestMigrationPending = false)
    {
        var migrations = context.Database.GetMigrations().ToArray();
        if (leaveLatestMigrationPending && migrations.Length > 0)
        {
            migrations = migrations[..^1];
        }
        if (migrations.Length == 0)
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync(
            "IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL " +
            "BEGIN " +
            "CREATE TABLE [dbo].[__EFMigrationsHistory] (" +
            "[MigrationId] nvarchar(150) NOT NULL, " +
            "[ProductVersion] nvarchar(32) NOT NULL, " +
            "CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])) " +
            "END");

        var productVersion = GetEfProductVersion();
        foreach (var migration in migrations)
        {
            await context.Database.ExecuteSqlInterpolatedAsync(
                $"IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = {migration}) INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({migration}, {productVersion})");
        }
    }

    private static string GetEfProductVersion()
    {
        return typeof(DbContext).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            .Split('+', StringSplitOptions.RemoveEmptyEntries)[0]
            ?? "8.0.0";
    }

    private static AdminSeedOptions? CreateAdminSeedOptions(IConfiguration configuration)
    {
        var username = configuration["SeedAdmin:Username"];
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        var updatePassword = configuration.GetValue<bool>("SeedAdmin:UpdatePassword");

        return string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            ? null
            : new AdminSeedOptions(username, email, password, updatePassword);
    }
}
