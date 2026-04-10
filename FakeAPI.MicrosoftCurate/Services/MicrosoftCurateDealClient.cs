using System.Collections.Concurrent;
using System.Threading;
using FakeAPI.MicrosoftCurate.Models;

namespace FakeAPI.MicrosoftCurate.Services;

public interface IMicrosoftCurateDealClient
{
    MicrosoftCurateCreateDealResponse Create(MicrosoftCurateCreateDealRequest request);

    bool Exists(string dealId);

    MicrosoftCurateDeal? GetById(string dealId);
}

public sealed class MicrosoftCurateDealClient : IMicrosoftCurateDealClient
{
    private static readonly ConcurrentDictionary<string, MicrosoftCurateDeal> Deals = new();
    private static int _nextDealNumber;

    public MicrosoftCurateCreateDealResponse Create(MicrosoftCurateCreateDealRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Html);

        var dealId = $"mc-deal-{Interlocked.Increment(ref _nextDealNumber):D4}";
        var deal = new MicrosoftCurateDeal(
            dealId,
            request.Name.Trim(),
            request.Html.Trim(),
            DateTime.UtcNow);

        Deals[dealId] = deal;

        return new MicrosoftCurateCreateDealResponse(dealId);
    }

    public bool Exists(string dealId) =>
        !string.IsNullOrWhiteSpace(dealId) && Deals.ContainsKey(dealId.Trim());

    public MicrosoftCurateDeal? GetById(string dealId)
    {
        if (string.IsNullOrWhiteSpace(dealId))
        {
            return null;
        }

        Deals.TryGetValue(dealId.Trim(), out var deal);
        return deal;
    }
}