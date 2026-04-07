using CreativesIntegration.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CreativesIntegration.Api.Services;

public interface IUserService
{
    Task<string?> LoginAsync(string username, string password);

    Task<bool> IsValidTokenAsync(string? token);
}

public class UserService(AppDbContext dbContext) : IUserService
{
    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(item =>
            item.Username == username && item.Password == password);

        return user?.Token;
    }

    public async Task<bool> IsValidTokenAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return await dbContext.Users.AnyAsync(user => user.Token == token);
    }
}