using Dormio.Api.Dtos;
using Dormio.Api.Entities;
using Dormio.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dormio.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _users;
    private readonly JwtTokenService _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<AppUser> users, JwtTokenService jwt, ILogger<AuthController> logger)
    {
        _users = users;
        _jwt = jwt;
        _logger = logger;
    }

    /// <summary>Register a Landlord or Tenant account. Admin accounts cannot be self-registered.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
    {
        _logger.LogInformation("Register invoked for {Email}", req.Email);
        if (req.Role is not (Roles.Landlord or Roles.Tenant))
            return BadRequest(new { error = "role_not_allowed" });

        var user = new AppUser { UserName = req.Email, Email = req.Email, PhoneNumber = req.PhoneNumber, FullName = req.FullName };
        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _users.AddToRoleAsync(user, req.Role);
        var (token, exp) = _jwt.Create(user, new[] { req.Role });
        return StatusCode(StatusCodes.Status201Created,
            new AuthResponse(token, exp, user.Id, user.FullName, user.Email!, new[] { req.Role }));
    }

    /// <summary>Authenticate and receive a JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        _logger.LogInformation("Login invoked for {Email}", req.Email);
        var user = await _users.FindByEmailAsync(req.Email);
        if (user is null || !user.IsActive || !await _users.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { error = "invalid_credentials" });

        var roles = (await _users.GetRolesAsync(user)).ToArray();
        var (token, exp) = _jwt.Create(user, roles);
        return new AuthResponse(token, exp, user.Id, user.FullName, user.Email!, roles);
    }
}
