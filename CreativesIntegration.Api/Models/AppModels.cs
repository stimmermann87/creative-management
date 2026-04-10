namespace CreativesIntegration.Api.Models;

public class Creative
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token);

public record CreateCreativeRequest(string Name, string HtmlContent, string Status);

public record UpdateCreativeRequest(string Name, string HtmlContent, string Status);

public record LaunchCreativeResponse(Creative Creative, string Message);

public record CreativeAnalyticsPoint(DateOnly Date, int Impressions, decimal Price);

public record CreativeAnalyticsResponse(
    Guid CreativeId,
    DateOnly FromDate,
    DateOnly ToDate,
    IReadOnlyList<CreativeAnalyticsPoint> DailyPoints);

public record CreativeCommandResult(Creative? Creative, Dictionary<string, string[]>? Errors = null, bool NotFound = false);