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

    Task<CreativeCommandResult> CreateAsync(CreateCreativeRequest request);

    Task<CreativeCommandResult> UpdateAsync(Guid id, UpdateCreativeRequest request);

    Task<LaunchCreativeResponse?> LaunchAsync(Guid id);
}

public class CreativeService(
    AppDbContext dbContext,
    IDealService dealService,
    IInsertionOrderService insertionOrderService) : ICreativeService
{
    public async Task<IReadOnlyList<Creative>> GetAllAsync() =>
        await dbContext.Creatives
            .OrderByDescending(creative => creative.CreatedAtUtc)
            .ToListAsync();

    public async Task<Creative?> GetByIdAsync(Guid id) =>
        await dbContext.Creatives.FirstOrDefaultAsync(creative => creative.Id == id);

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

        _ = ResolvePlatformName(creative);

        var dealResponse = dealService.Create(new CreateDealRequest(
            creative.Name,
            creative.HtmlContent));

        var insertionOrderResponse = insertionOrderService.Create(new CreateInsertionOrderRequest(
            dealResponse.DealId,
            DateTime.UtcNow));

        creative.Status = "LaunchRequested";
        await dbContext.SaveChangesAsync();

        return new LaunchCreativeResponse(
            creative,
            $"Submitted to fake Microsoft Curate. Deal {dealResponse.DealId}, insertion order {insertionOrderResponse.InsertionOrderId}.");
    }

    private static PlatformName ResolvePlatformName(Creative creative) => PlatformName.MicrosoftCurate;

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