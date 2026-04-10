using FakeAPI.IndexExchange.Models;

namespace FakeAPI.IndexExchange.Services;

public interface IIndexExchangeMarketplaceService
{
    IndexExchangePublishMarketplaceResponse Publish(IndexExchangePublishMarketplaceRequest request);
}

public sealed class IndexExchangeMarketplaceService(IIndexExchangeDealService dealService) : IIndexExchangeMarketplaceService
{
    public IndexExchangePublishMarketplaceResponse Publish(IndexExchangePublishMarketplaceRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DealId);

        var normalizedDealId = request.DealId.Trim();
        var deal = dealService.GetById(normalizedDealId);
        if (deal is null)
        {
            return new IndexExchangePublishMarketplaceResponse(
                normalizedDealId,
                request.StartDate,
                false,
                $"Deal '{normalizedDealId}' does not exist.");
        }

        if (deal.Status != IndexExchangeDealStatus.Completed)
        {
            return new IndexExchangePublishMarketplaceResponse(
                normalizedDealId,
                request.StartDate,
                false,
                $"Deal '{normalizedDealId}' is {deal.Status} and cannot be published.");
        }

        return new IndexExchangePublishMarketplaceResponse(
            normalizedDealId,
            request.StartDate,
            true,
            "Deal published to fake Index Exchange marketplace.");
    }
}