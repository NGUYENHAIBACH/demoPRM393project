using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dormio.Api.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Dormio.Api.Services;

public class JwtTokenService
{
    private readonly IConfiguration _config;
    public JwtTokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime ExpiresAt) Create(AppUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var expires = DateTime.UtcNow.AddMinutes(_config.GetValue("Jwt:ExpireMinutes", 120));
        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"], _config["Jwt:Audience"], claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
