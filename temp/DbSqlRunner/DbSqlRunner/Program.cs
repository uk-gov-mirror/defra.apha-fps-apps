using System.Text.Json;
using Npgsql;

if (args.Length < 1)
{
    Console.WriteLine("Usage: DbSqlRunner <sql-file-path-or-inline-sql>");
    return 1;
}

var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");
if (!File.Exists(configPath))
{
    Console.WriteLine($"ERROR: {configPath} not found. Create it with:");
    Console.WriteLine("""{ "ConnectionStrings": { "BatchJobTesting": "Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true" } }""");
    return 1;
}

using var configDoc = JsonDocument.Parse(File.ReadAllText(configPath));
var connString = configDoc.RootElement.GetProperty("ConnectionStrings").GetProperty("BatchJobTesting").GetString()
    ?? throw new InvalidOperationException("ConnectionStrings:BatchJobTesting missing in appsettings.local.json");

var sql = File.Exists(args[0]) ? File.ReadAllText(args[0]) : args[0];

await using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();

await using var cmd = new NpgsqlCommand(sql, conn);

try
{
    await using var reader = await cmd.ExecuteReaderAsync();
    do
    {
        if (reader.FieldCount == 0)
        {
            Console.WriteLine($"(OK — {reader.RecordsAffected} row(s) affected)");
            continue;
        }

        var headers = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName);
        Console.WriteLine(string.Join(" | ", headers));

        var rowCount = 0;
        while (await reader.ReadAsync())
        {
            rowCount++;
            var values = Enumerable.Range(0, reader.FieldCount)
                .Select(i => reader.IsDBNull(i) ? "<null>" : reader.GetValue(i)?.ToString());
            Console.WriteLine(string.Join(" | ", values));
        }

        Console.WriteLine($"({rowCount} row(s))");
        Console.WriteLine();
    } while (await reader.NextResultAsync());

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
    return 1;
}
