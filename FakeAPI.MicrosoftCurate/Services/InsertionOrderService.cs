using System.Collections.Concurrent;
using System.Threading;
using FakeAPI.MicrosoftCurate.Models;

namespace FakeAPI.MicrosoftCurate.Services;

public interface IMicrosoftCurateInsertionOrderService
{
    MicrosoftCurateCreateInsertionOrderResponse Create(MicrosoftCurateCreateInsertionOrderRequest request);

    MicrosoftCurateInsertionOrder? GetById(string insertionOrderId);
}

public sealed class MicrosoftCurateInsertionOrderService(IMicrosoftCurateDealService dealService) : IMicrosoftCurateInsertionOrderService
{
    private static readonly ConcurrentDictionary<string, MicrosoftCurateInsertionOrder> InsertionOrders = new();
    private static int _nextInsertionOrderNumber;

    public MicrosoftCurateCreateInsertionOrderResponse Create(MicrosoftCurateCreateInsertionOrderRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DealId);

        var normalizedDealId = request.DealId.Trim();
        if (!dealService.Exists(normalizedDealId))
        {
            throw new InvalidOperationException($"Deal '{normalizedDealId}' does not exist.");
        }

        var insertionOrderId = $"mc-io-{Interlocked.Increment(ref _nextInsertionOrderNumber):D4}";
        var insertionOrder = new MicrosoftCurateInsertionOrder(
            insertionOrderId,
            normalizedDealId,
            request.StartDate,
            DateTime.UtcNow);

        InsertionOrders[insertionOrderId] = insertionOrder;

        return new MicrosoftCurateCreateInsertionOrderResponse(insertionOrderId);
    }

    public MicrosoftCurateInsertionOrder? GetById(string insertionOrderId)
    {
        if (string.IsNullOrWhiteSpace(insertionOrderId))
        {
            return null;
        }

        InsertionOrders.TryGetValue(insertionOrderId.Trim(), out var insertionOrder);
        return insertionOrder;
    }
}