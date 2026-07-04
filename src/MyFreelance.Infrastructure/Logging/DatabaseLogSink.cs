using Microsoft.Data.SqlClient;
using Serilog.Core;
using Serilog.Events;

namespace MyFreelance.Infrastructure.Logging;

public sealed class DatabaseLogSink(string connectionString) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        try
        {
            var type = logEvent.Level.ToString();
            var message = logEvent.RenderMessage();
            if (logEvent.Exception is not null)
                message = $"{message}{Environment.NewLine}{logEvent.Exception}";

            var functionName = logEvent.Properties.TryGetValue("SourceContext", out var source)
                ? source.ToString().Trim('"')
                : string.Empty;

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO ApplicationLogs (Id, Type, FunctionName, Message, CreatedAt)
                VALUES (@Id, @Type, @FunctionName, @Message, @CreatedAt)
                """;
            command.Parameters.AddWithValue("@Id", Guid.NewGuid());
            command.Parameters.AddWithValue("@Type", type);
            command.Parameters.AddWithValue("@FunctionName", functionName);
            command.Parameters.AddWithValue("@Message", message);
            command.Parameters.AddWithValue("@CreatedAt", logEvent.Timestamp.UtcDateTime);
            command.ExecuteNonQuery();
        }
        catch
        {
            // Logging must not crash the application.
        }
    }
}
