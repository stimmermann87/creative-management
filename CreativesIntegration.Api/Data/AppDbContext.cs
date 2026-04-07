using CreativesIntegration.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CreativesIntegration.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Creative> Creatives => Set<Creative>();

    public DbSet<User> Users => Set<User>();
}