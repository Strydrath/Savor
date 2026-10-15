namespace Savor.Contracts.Common;

public sealed record HealthResponse(string Status, string Application, DateTimeOffset TimestampUtc);
