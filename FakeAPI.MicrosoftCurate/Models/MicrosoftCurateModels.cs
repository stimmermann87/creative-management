namespace FakeAPI.MicrosoftCurate.Models;

public sealed record CreateDealRequest(string Name, string Html);

public sealed record CreateDealResponse(string DealId);

public sealed record CreateInsertionOrderRequest(string DealId, DateTime StartDate);

public sealed record CreateInsertionOrderResponse(string InsertionOrderId);

public sealed record MicrosoftCurateDeal(
    string DealId,
    string Name,
    string Html,
    DateTime CreatedAtUtc);

public sealed record MicrosoftCurateInsertionOrder(
    string InsertionOrderId,
    string DealId,
    DateTime StartDate,
    DateTime CreatedAtUtc);