using DbUp;
using DbUp.Engine;
using System.Reflection;

namespace DigitalStokvel.DbMigrator;

/// <summary>
/// Database migration runner using DbUp to apply versioned SQL migrations.
/// Supports both forward migrations (Vxxx__*.sql) and rollback scripts (Uxxx__*.sql).
/// </summary>
class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("=======================================================");
        Console.WriteLine("Digital Stokvel - Database Migration Runner");
        Console.WriteLine("=======================================================");
        Console.WriteLine();

        // Parse command line arguments
        var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "migrate";
        var connectionString = GetConnectionString(args);

        if (string.IsNullOrEmpty(connectionString))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: Connection string not provided.");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  DigitalStokvel.DbMigrator migrate <connection-string>");
            Console.WriteLine("  DigitalStokvel.DbMigrator status <connection-string>");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  DigitalStokvel.DbMigrator migrate \"Host=localhost;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=Dev_Password_2026!\"");
            Console.WriteLine();
            Console.WriteLine("Or use environment variable:");
            Console.WriteLine("  set POSTGRES_CONNECTION_STRING=...");
            Console.WriteLine("  DigitalStokvel.DbMigrator migrate");
            return 1;
        }

        return mode switch
        {
            "migrate" => RunMigrations(connectionString),
            "status" => ShowMigrationStatus(connectionString),
            _ => ShowHelp()
        };
    }

    /// <summary>
    /// Get connection string from command line arguments or environment variable.
    /// </summary>
    private static string? GetConnectionString(string[] args)
    {
        // Check command line argument
        if (args.Length >= 2)
        {
            return args[1];
        }

        // Check environment variable
        var envConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        // Check for individual environment variables (docker-compose style)
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "digitalstokvel";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "stokvel_admin";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (!string.IsNullOrEmpty(password))
        {
            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        return null;
    }

    /// <summary>
    /// Run all pending migrations.
    /// </summary>
    private static int RunMigrations(string connectionString)
    {
        Console.WriteLine($"Connecting to database...");
        Console.WriteLine();

        // Ensure database exists
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        // Configure DbUp to use SQL scripts from the Migrations folder
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsFromFileSystem(Path.Combine(AppContext.BaseDirectory, "Migrations"))
            .WithScriptNameComparer(new NaturalOrderComparer())
            .LogToConsole()
            .Build();

        Console.WriteLine();

        // Check if migrations are needed
        var scriptsToExecute = upgrader.GetScriptsToExecute();
        if (!scriptsToExecute.Any())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Database is up to date. No migrations to run.");
            Console.ResetColor();
            return 0;
        }

        Console.WriteLine($"Found {scriptsToExecute.Count} migration(s) to execute:");
        foreach (var script in scriptsToExecute)
        {
            Console.WriteLine($"  - {script.Name}");
        }
        Console.WriteLine();

        // Execute migrations
        Console.WriteLine("Executing migrations...");
        var result = upgrader.PerformUpgrade();

        // Display result
        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ MIGRATION FAILED: {result.Error}");
            Console.ResetColor();
            return 1;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ SUCCESS! Database migrations completed successfully.");
        Console.ResetColor();
        
        foreach (var script in result.Scripts)
        {
            Console.WriteLine($"  ✓ {script.Name}");
        }

        return 0;
    }

    /// <summary>
    /// Show status of migrations (which have been applied, which are pending).
    /// </summary>
    private static int ShowMigrationStatus(string connectionString)
    {
        Console.WriteLine($"Checking migration status...");
        Console.WriteLine();

        try
        {
            var upgrader = DeployChanges.To
                .PostgresqlDatabase(connectionString)
                .WithScriptsFromFileSystem(Path.Combine(AppContext.BaseDirectory, "Migrations"))
                .WithScriptNameComparer(new NaturalOrderComparer())
                .LogScriptOutput()
                .Build();

            var executedScripts = upgrader.GetExecutedScripts();
            var scriptsToExecute = upgrader.GetScriptsToExecute();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Applied Migrations ({executedScripts.Count}):");
            Console.ResetColor();
            foreach (var script in executedScripts)
            {
                Console.WriteLine($"  ✓ {script}");
            }
            Console.WriteLine();

            if (scriptsToExecute.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Pending Migrations ({scriptsToExecute.Count}):");
                Console.ResetColor();
                foreach (var script in scriptsToExecute)
                {
                    Console.WriteLine($"  ⧗ {script.Name}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Database is up to date. No pending migrations.");
                Console.ResetColor();
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ ERROR: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
    }

    /// <summary>
    /// Show help information.
    /// </summary>
    private static int ShowHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  DigitalStokvel.DbMigrator <command> [connection-string]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  migrate    Run all pending migrations");
        Console.WriteLine("  status     Show which migrations have been applied");
        Console.WriteLine();
        Console.WriteLine("Connection String:");
        Console.WriteLine("  Provide as second argument or set POSTGRES_CONNECTION_STRING environment variable");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  DigitalStokvel.DbMigrator migrate \"Host=localhost;Port=5432;Database=digitalstokvel;Username=stokvel_admin;Password=***\"");
        Console.WriteLine("  DigitalStokvel.DbMigrator status");
        Console.WriteLine();
        return 1;
    }

    /// <summary>
    /// Natural order comparer for script names to ensure V001, V002, ..., V010 sort correctly.
    /// </summary>
    private class NaturalOrderComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            // Extract version numbers from Vxxx__*.sql format
            var xVersion = ExtractVersionNumber(x);
            var yVersion = ExtractVersionNumber(y);

            if (xVersion.HasValue && yVersion.HasValue)
            {
                return xVersion.Value.CompareTo(yVersion.Value);
            }

            // Fallback to string comparison
            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }

        private int? ExtractVersionNumber(string scriptName)
        {
            // Extract number from Vxxx__*.sql format
            if (scriptName.StartsWith("V", StringComparison.OrdinalIgnoreCase))
            {
                var parts = scriptName.Substring(1).Split(new[] { '_', '.' }, 2);
                if (parts.Length > 0 && int.TryParse(parts[0], out var version))
                {
                    return version;
                }
            }
            return null;
        }
    }
}
