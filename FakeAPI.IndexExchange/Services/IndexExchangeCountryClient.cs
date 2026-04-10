using System.Threading;
using FakeAPI.IndexExchange.Models;

namespace FakeAPI.IndexExchange.Services;

public interface IIndexExchangeCountryClient
{
    IndexExchangeCountry GetOrCreate(IndexExchangeGetOrCreateCountryRequest request);

    IndexExchangeCountry? GetById(int countryId);
}

public sealed class IndexExchangeCountryClient : IIndexExchangeCountryClient
{
    private static int _nextCountryId;

    public IndexExchangeCountry GetOrCreate(IndexExchangeGetOrCreateCountryRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Iso3CountryCode);

        var normalizedCode = request.Iso3CountryCode.Trim().ToUpperInvariant();
        var existing = IndexExchangeStore.Countries.Values.FirstOrDefault(country =>
            country.Iso3CountryCode == normalizedCode);

        if (existing is not null)
        {
            return existing;
        }

        var country = new IndexExchangeCountry(
            Interlocked.Increment(ref _nextCountryId),
            normalizedCode,
            DateTime.UtcNow);

        IndexExchangeStore.Countries[country.CountryId] = country;

        return country;
    }

    public IndexExchangeCountry? GetById(int countryId)
    {
        IndexExchangeStore.Countries.TryGetValue(countryId, out var country);
        return country;
    }
}