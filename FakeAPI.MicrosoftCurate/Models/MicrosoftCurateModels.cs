namespace FakeAPI.MicrosoftCurate.Models;

public sealed record MicrosoftCurateCreateDealRequest(string Name, string Html);

public sealed record MicrosoftCurateCreateDealResponse(string DealId);

public sealed record MicrosoftCurateCreateInsertionOrderRequest(string DealId, DateTime StartDate);

public sealed record MicrosoftCurateCreateInsertionOrderResponse(string InsertionOrderId);

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