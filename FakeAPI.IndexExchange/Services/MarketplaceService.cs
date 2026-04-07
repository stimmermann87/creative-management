using FakeAPI.IndexExchange.Models;

namespace FakeAPI.IndexExchange.Services;

public interface IMarketplaceService
{
    PublishMarketplaceResponse Publish(PublishMarketplaceRequest request);
}

public sealed class MarketplaceService(IDealService dealService) : IMarketplaceService
{
    public PublishMarketplaceResponse Publish(PublishMarketplaceRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DealId);

        var normalizedDealId = request.DealId.Trim();
        var deal = dealService.GetById(normalizedDealId);
        if (deal is null)
        {
            return new PublishMarketplaceResponse(
                normalizedDealId,
                request.StartDate,
                false,
                $"Deal '{normalizedDealId}' does not exist.");
        }

        if (deal.Status != IndexExchangeDealStatus.Completed)
        {
            return new PublishMarketplaceResponse(
                normalizedDealId,
                request.StartDate,
                false,
                $"Deal '{normalizedDealId}' is {deal.Status} and cannot be published.");
        }

        return new PublishMarketplaceResponse(
            normalizedDealId,
            request.StartDate,
            true,
            "Deal published to fake Index Exchange marketplace.");
    }
}