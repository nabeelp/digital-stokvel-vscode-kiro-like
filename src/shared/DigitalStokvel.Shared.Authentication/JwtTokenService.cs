using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DigitalStokvel.Shared.Authentication;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate an access token for a user
    /// </summary>
    string GenerateAccessToken(string userId, string phoneNumber, IEnumerable<string> roles, IEnumerable<string> groupIds);

    /// <summary>
    /// Generate a refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate a token and extract claims
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Get user ID from token
    /// </summary>
    string? GetUserIdFromToken(string token);
}

/// <summary>
/// Implementation of JWT token service
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <inheritdoc/>
    public string GenerateAccessToken(string userId, string phoneNumber, IEnumerable<string> roles, IEnumerable<string> groupIds)
    {
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var symmetricKey = new SymmetricSecurityKey(key);
        var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("phone", phoneNumber)
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        // Add group memberships
        foreach (var groupId in groupIds)
        {
            claims.Add(new Claim("groups", groupId));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = signingCredentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <inheritdoc/>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Verify the token is actually a JWT and uses the correct algorithm
            if (validatedToken is JwtSecurityToken jwtToken &&
                jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return principal;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public string? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
    }
}
