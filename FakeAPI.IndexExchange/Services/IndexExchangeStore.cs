using System.Collections.Concurrent;
using FakeAPI.IndexExchange.Models;

namespace FakeAPI.IndexExchange.Services;

internal static class IndexExchangeStore
{
    internal static ConcurrentDictionary<int, IndexExchangeCountry> Countries { get; } = new();

    internal static ConcurrentDictionary<string, IndexExchangeDeal> Deals { get; } = new();
}