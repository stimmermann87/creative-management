namespace FakeAPI.IndexExchange.Models;

public enum IndexExchangeDealStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public sealed record IndexExchangeCountry(
    int CountryId,
    string Iso3CountryCode,
    DateTime CreatedAtUtc);

public sealed record IndexExchangeGetOrCreateCountryRequest(string Iso3CountryCode);

public sealed record IndexExchangeCreateDealRequest(string Name, string Html, int CountryId);

public sealed record IndexExchangeCreateDealResponse(string DealId, IndexExchangeDealStatus Status);

public sealed record IndexExchangeDeal(
    string DealId,
    string Name,
    string Html,
    int CountryId,
    DateTime CreatedAtUtc,
    IndexExchangeDealStatus Status);

public sealed record IndexExchangePublishMarketplaceRequest(string DealId, DateTime StartDate);

public sealed record IndexExchangePublishMarketplaceResponse(
    string DealId,
    DateTime StartDate,
    bool Success,
    string Message);
