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

        return BuildAnalyticsResponse(id, days);
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

    // The API returns the raw seeded points as canonical data so chart-specific handling stays in the UI.
    private static CreativeAnalyticsResponse BuildAnalyticsResponse(Guid creativeId, int days)
    {
        const int availableDays = 30;

        var normalizedDays = days <= 0
            ? availableDays
            : Math.Min(days, availableDays);

        var allPoints = BuildThirtyDayAnalytics(DateOnly.FromDateTime(DateTime.UtcNow));
        var selectedPoints = allPoints
            .Skip(allPoints.Count - normalizedDays)
            .ToList();

        return new CreativeAnalyticsResponse(
            creativeId,
            selectedPoints[0].Date,
            selectedPoints[^1].Date,
            selectedPoints);
    }

    private static List<CreativeAnalyticsPoint> BuildThirtyDayAnalytics(DateOnly toDate)
    {
        var points = new List<CreativeAnalyticsPoint>(capacity: 30);
        var fromDate = toDate.AddDays(-29);

        for (var index = 0; index < 30; index++)
        {
            var dayNumber = index + 1;
            var date = fromDate.AddDays(index);

            if (dayNumber == 1)
            {
                points.Add(new CreativeAnalyticsPoint(date, 184, 26.45m));
                continue;
            }

            if (dayNumber == 2)
            {
                points.Add(new CreativeAnalyticsPoint(date, 4, 1280.00m));
                continue;
            }

            if (dayNumber <= 9)
            {
                points.Add(new CreativeAnalyticsPoint(date, 0, 0m));
                continue;
            }

            var balancedDay = dayNumber - 9;
            var impressions = 175 + ((balancedDay * 37) % 120) + ((balancedDay % 3) * 18);
            var price = decimal.Round((impressions * 0.12m) + ((balancedDay % 4) * 1.65m), 2);

            points.Add(new CreativeAnalyticsPoint(date, impressions, price));
        }

        return points;
    }

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