using CreativesIntegration.Api.Data;
using CreativesIntegration.Api.Models;
using CreativesIntegration.Api.Services;
using FakeAPI.MicrosoftCurate.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("CreativesIntegration"));
builder.Services.AddScoped<ICreativeService, CreativeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IMicrosoftCurateDealService, MicrosoftCurateDealService>();
builder.Services.AddSingleton<IMicrosoftCurateInsertionOrderService, MicrosoftCurateInsertionOrderService>();

var app = builder.Build();

SeedDatabase(app);

app.MapGet("/", () => Results.Ok(new { message = "Creatives API is running." }));
app.MapControllers();

app.Run();

static void SeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    dbContext.Database.EnsureCreated();

    if (dbContext.Users.Any() || dbContext.Creatives.Any())
    {
        return;
    }

    dbContext.Users.Add(new User
    {
        Id = 1,
        Username = "demo",
        Password = "password",
        Token = "demo-token"
    });

    dbContext.Creatives.AddRange(
        new Creative
        {
            Id = Guid.Parse("8f2208c9-5738-4f68-b311-c9876bde5f62"),
            Name = "Spring Sale Banner",
            HtmlContent = "<div><h1>Spring Sale</h1><p>Up to 30% off this week.</p></div>",
            Status = "Ready",
            CreatedAtUtc = new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc)
        },
        new Creative
        {
            Id = Guid.Parse("d3d6c5fc-18b4-4fe1-8d52-7412a8b3a8de"),
            Name = "Summer Launch Hero",
            HtmlContent = "<section><h2>Summer Launch</h2><p>New collection available now.</p></section>",
            Status = "Draft",
            CreatedAtUtc = new DateTime(2026, 3, 31, 12, 0, 0, DateTimeKind.Utc)
        },
        new Creative
        {
            Id = Guid.Parse("6c89ce38-ab1b-4a7b-8cb6-7c83e1b561cf"),
            Name = "Newsletter Promo",
            HtmlContent = "<div><strong>Newsletter Promo</strong><p>Sign up for product updates.</p></div>",
            Status = "LaunchRequested",
            CreatedAtUtc = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)
        });

    dbContext.SaveChanges();
}
