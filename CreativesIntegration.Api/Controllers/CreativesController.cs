using CreativesIntegration.Api.Models;
using CreativesIntegration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreativesIntegration.Api.Controllers;

[ApiController]
[Route("api/creatives")]
public class CreativesController(ICreativeService creativeService, IUserService userService) : ControllerBase
{
    private const string DemoTokenHeader = "X-Demo-Token";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var authorizationResult = await AuthorizeRequestAsync();
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        return Ok(await creativeService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var authorizationResult = await AuthorizeRequestAsync();
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var creative = await creativeService.GetByIdAsync(id);
        return creative is null
            ? NotFound(new { message = "Creative not found." })
            : Ok(creative);
    }

    [HttpGet("{id:guid}/analytics")]
    public async Task<IActionResult> GetAnalytics(Guid id, [FromQuery] int days = 30)
    {
        var authorizationResult = await AuthorizeRequestAsync();
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var analytics = await creativeService.GetAnalyticsAsync(id, days);
        return analytics is null
            ? NotFound(new { message = "Creative not found." })
            : Ok(analytics);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCreativeRequest request)
    {
        var authorizationResult = await AuthorizeRequestAsync();
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var result = await creativeService.CreateAsync(request);
        if (result.Errors is not null)
        {
            return ValidationProblem(new ValidationProblemDetails(result.Errors));
        }

        return Created($"/api/creatives/{result.Creative!.Id}", result.Creative);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCreativeRequest request)
    {
        var authorizationResult = await AuthorizeRequestAsync();
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var result = await creativeService.UpdateAsync(id, request);

        if (result.NotFound)
        {
            return NotFound(new { message = "Creative not found." });
        }

        if (result.Errors is not null)
        {
            return ValidationProblem(new ValidationProblemDetails(result.Errors));
        }

        return Ok(result.Creative);
    }

    [HttpPost("{id:guid}/launch")]
    public async Task<IActionResult> Launch(Guid id)
    {
        var authorizationResult = await AuthorizeRequestAsync();
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var result = await creativeService.LaunchAsync(id);
        return result is null
            ? NotFound(new { message = "Creative not found." })
            : Ok(result);
    }

    private async Task<IActionResult?> AuthorizeRequestAsync()
    {
        if (!Request.Headers.TryGetValue(DemoTokenHeader, out var token)
            || !await userService.IsValidTokenAsync(token))
        {
            return Unauthorized(new { message = "Missing or invalid demo token." });
        }

        return null;
    }
}