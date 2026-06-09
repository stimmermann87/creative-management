using System.Globalization;
using CreativesIntegration.Api.Data;
using CreativesIntegration.Api.Models;
using FakeAPI.MicrosoftCurate.Models;
using FakeAPI.MicrosoftCurate.Services;
using Microsoft.EntityFrameworkCore;

namespace CreativesIntegration.Api.Services;

public interface ICreativeService
{
    Task<IReadOnlyList<Creative>> GetAllAsync();

    Task<Creative?> GetByIdAsync(Guid id);

    Task<CreativeAnalyticsResponse?> GetAnalyticsAsync(Guid id, int days);

    Task<IReadOnlyList<TrackingEvent>?> GetEventsAsync(Guid id, DateOnly? date, string? eventType);

    Task<CreativeCommandResult> CreateAsync(CreateCreativeRequest request);

    Task<CreativeCommandResult> UpdateAsync(Guid id, UpdateCreativeRequest request);

    Task<LaunchCreativeResponse?> LaunchAsync(Guid id);
}

public class CreativeService(
    AppDbContext dbContext,
    IMicrosoftCurateDealClient dealClient,
    IMicrosoftCurateInsertionOrderClient insertionOrderClient) : ICreativeService
{
    public async Task<IReadOnlyList<Creative>> GetAllAsync() =>
        await dbContext.Creatives
            .OrderByDescending(creative => creative.CreatedAtUtc)
            .ToListAsync();

    public async Task<Creative?> GetByIdAsync(Guid id) =>
        await dbContext.Creatives.FirstOrDefaultAsync(creative => creative.Id == id);

    public async Task<CreativeAnalyticsResponse?> GetAnalyticsAsync(Guid id, int days)
    {
        var creativeExists = await dbContext.Creatives.AnyAsync(creative => creative.Id == id);
        if (!creativeExists)
        {
            return null;
        }

        var events = await dbContext.TrackingEvents
            .Where(trackingEvent => trackingEvent.CreativeId == id)
            .ToListAsync();

        return BuildAnalyticsResponse(id, days, events);
    }

    public async Task<IReadOnlyList<TrackingEvent>?> GetEventsAsync(Guid id, DateOnly? date, string? eventType)
    {
        var creativeExists = await dbContext.Creatives.AnyAsync(creative => creative.Id == id);
        if (!creativeExists)
        {
            return null;
        }

        IEnumerable<TrackingEvent> events = await dbContext.TrackingEvents
            .Where(trackingEvent => trackingEvent.CreativeId == id)
            .ToListAsync();

        if (date is not null)
        {
            events = events.Where(trackingEvent =>
                DateOnly.FromDateTime(trackingEvent.ServerReceivedAtUtc) == date);
        }

        if (!string.IsNullOrWhiteSpace(eventType))
        {
            events = events.Where(trackingEvent =>
                string.Equals(trackingEvent.EventType, eventType, StringComparison.OrdinalIgnoreCase));
        }

        return events
            .OrderBy(trackingEvent => trackingEvent.ServerReceivedAtUtc)
            .ToList();
    }

    public async Task<CreativeCommandResult> CreateAsync(CreateCreativeRequest request)
    {
        var errors = Validate(request.Name, request.HtmlContent, request.Status);
        if (errors.Count > 0)
        {
            return new CreativeCommandResult(null, errors);
        }

        var creative = new Creative
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            HtmlContent = request.HtmlContent.Trim(),
            Status = NormalizeStatus(request.Status),
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Creatives.Add(creative);
        await dbContext.SaveChangesAsync();

        return new CreativeCommandResult(creative);
    }

    public async Task<CreativeCommandResult> UpdateAsync(Guid id, UpdateCreativeRequest request)
    {
        var creative = await dbContext.Creatives.FirstOrDefaultAsync(item => item.Id == id);
        if (creative is null)
        {
            return new CreativeCommandResult(null, NotFound: true);
        }

        var errors = Validate(request.Name, request.HtmlContent, request.Status);
        if (errors.Count > 0)
        {
            return new CreativeCommandResult(null, errors);
        }

        creative.Name = request.Name.Trim();
        creative.HtmlContent = request.HtmlContent.Trim();
        creative.Status = NormalizeStatus(request.Status);

        await dbContext.SaveChangesAsync();

        return new CreativeCommandResult(creative);
    }

    public async Task<LaunchCreativeResponse?> LaunchAsync(Guid id)
    {
        var creative = await dbContext.Creatives.FirstOrDefaultAsync(item => item.Id == id);
        if (creative is null)
        {
            return null;
        }

        var dealResponse = dealClient.Create(new MicrosoftCurateCreateDealRequest(
            creative.Name,
            creative.HtmlContent));

        var insertionOrderResponse = insertionOrderClient.Create(new MicrosoftCurateCreateInsertionOrderRequest(
            dealResponse.DealId,
            DateTime.UtcNow));

        creative.Status = "LaunchRequested";
        await dbContext.SaveChangesAsync();

        return new LaunchCreativeResponse(
            creative,
            $"Submitted to fake Microsoft Curate. Deal {dealResponse.DealId}, insertion order {insertionOrderResponse.InsertionOrderId}.");
    }

    // Rolls raw tracking events into one point per day for the chart.
    private static CreativeAnalyticsResponse BuildAnalyticsResponse(Guid creativeId, int days, List<TrackingEvent> events)
    {
        const int availableDays = 30;

        var normalizedDays = days <= 0
            ? availableDays
            : Math.Min(days, availableDays);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstDay = today.AddDays(-(availableDays - 1));

        var dailyTotals = events
            .Where(trackingEvent => trackingEvent.EventType == "impression")
            .GroupBy(trackingEvent => DateOnly.FromDateTime(trackingEvent.ServerReceivedAtUtc))
            .ToDictionary(
                group => group.Key,
                group => new DailyTotal(
                    // Count each session once so repeat views from the same session are not over-counted.
                    group.Select(trackingEvent => trackingEvent.SessionId).Distinct().Count(),
                    // Total billed amount for the day.
                    group.Sum(trackingEvent => trackingEvent.Price)));

        var allPoints = new List<CreativeAnalyticsPoint>(availableDays);
        for (var index = 0; index < availableDays; index++)
        {
            var date = firstDay.AddDays(index);

            allPoints.Add(dailyTotals.TryGetValue(date, out var total)
                ? new CreativeAnalyticsPoint(date, total.Impressions, decimal.Round(total.Price, 2))
                : new CreativeAnalyticsPoint(date, 0, 0m));
        }

        var selectedPoints = allPoints
            .Skip(allPoints.Count - normalizedDays)
            .ToList();

        return new CreativeAnalyticsResponse(
            creativeId,
            selectedPoints[0].Date,
            selectedPoints[^1].Date,
            selectedPoints);
    }

    private readonly record struct DailyTotal(int Impressions, decimal Price);

    private static Dictionary<string, string[]> Validate(string name, string htmlContent, string status)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Name is required."];
        }

        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            errors["htmlContent"] = ["HtmlContent is required."];
        }

        if (!IsAllowedStatus(NormalizeStatus(status)))
        {
            errors["status"] = ["Status must be one of: Draft, Ready, LaunchRequested."];
        }

        return errors;
    }

    private static string NormalizeStatus(string status) =>
        string.IsNullOrWhiteSpace(status)
            ? "Draft"
            : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(status.Trim().ToLowerInvariant());

    private static bool IsAllowedStatus(string status) => status is "Draft" or "Ready" or "LaunchRequested";
}