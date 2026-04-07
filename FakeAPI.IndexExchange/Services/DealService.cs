using System.Threading;
using FakeAPI.IndexExchange.Models;

namespace FakeAPI.IndexExchange.Services;

public interface IDealService
{
    CreateDealResponse Create(CreateDealRequest request);

    IndexExchangeDeal? GetById(string dealId);
}

public sealed class DealService(ICountryService countryService) : IDealService
{
    private static int _nextDealNumber;

    public CreateDealResponse Create(CreateDealRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Html);

        if (countryService.GetById(request.CountryId) is null)
        {
            throw new InvalidOperationException($"Country '{request.CountryId}' does not exist.");
        }

        var dealId = $"ix-deal-{Interlocked.Increment(ref _nextDealNumber):D4}";
        var deal = new IndexExchangeDeal(
            dealId,
            request.Name.Trim(),
            request.Html.Trim(),
            request.CountryId,
            DateTime.UtcNow,
            IndexExchangeDealStatus.Pending);

        IndexExchangeStore.Deals[dealId] = deal;

        return new CreateDealResponse(dealId, deal.Status);
    }

    public IndexExchangeDeal? GetById(string dealId)
    {
        if (string.IsNullOrWhiteSpace(dealId))
        {
            return null;
        }

        if (!IndexExchangeStore.Deals.TryGetValue(dealId.Trim(), out var storedDeal))
        {
            return null;
        }

        var status = ResolveStatus(storedDeal);
        if (status != storedDeal.Status)
        {
            storedDeal = storedDeal with { Status = status };
            IndexExchangeStore.Deals[storedDeal.DealId] = storedDeal;
        }

        return storedDeal;
    }

    private static IndexExchangeDealStatus ResolveStatus(IndexExchangeDeal deal)
    {
        var age = DateTime.UtcNow - deal.CreatedAtUtc;
        if (age < TimeSpan.FromSeconds(1))
        {
            return IndexExchangeDealStatus.Pending;
        }

        if (age < TimeSpan.FromSeconds(3))
        {
            return IndexExchangeDealStatus.InProgress;
        }

        if (deal.Html.Contains("<script", StringComparison.OrdinalIgnoreCase)
            || deal.Html.Length > 1500)
        {
            return IndexExchangeDealStatus.Failed;
        }

        return IndexExchangeDealStatus.Completed;
    }
}