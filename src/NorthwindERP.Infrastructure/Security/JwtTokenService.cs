using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NorthwindERP.Core.Entities;
using NorthwindERP.Core.Interfaces;
using NorthwindERP.Core.Models;

namespace NorthwindERP.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options) => _settings = options.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateToken(UserAccount user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        // Datos que viajan dentro del token (el frontend puede leerlos, NO pongas secretos).
        var claims = new List<Claim>
        {
            new(CustomClaims.UserId, user.UserAccountId.ToString()),
            new(CustomClaims.Username, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Role?.RoleName))
            claims.Add(new Claim(CustomClaims.Role, user.Role.RoleName));

        if (user.EmployeeId.HasValue)
            claims.Add(new Claim(CustomClaims.EmployeeId, user.EmployeeId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
