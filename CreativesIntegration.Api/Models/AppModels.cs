namespace CreativesIntegration.Api.Models;

public class Creative
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}

public class TrackingEvent
{
    public Guid Id { get; set; }

    public Guid CreativeId { get; set; }

    // "impression" or "click".
    public string EventType { get; set; } = string.Empty;

    // When the browser says the event happened.
    public DateTime ClientTimestampUtc { get; set; }

    // When the API actually received the event.
    public DateTime ServerReceivedAtUtc { get; set; }

    // When the creative entered / left the viewport (impressions only; null for clicks).
    public DateTime? InViewUtc { get; set; }

    public DateTime? OutOfViewUtc { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public int DwellMs { get; set; }

    public decimal Price { get; set; }
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