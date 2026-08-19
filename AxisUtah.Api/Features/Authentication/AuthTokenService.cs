namespace AxisUtah.Api.Features.Authentication;

public class AuthTokenService(
    IOptions<JwtOption> jwtOptions,
    IWebHostEnvironment environment)
{
    private readonly JwtOption _jwtOptions = jwtOptions.Value;
    private readonly IWebHostEnvironment _environment = environment;

    public JwtSecurityToken GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(_jwtOptions.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: GetExpiresUtc(),
            signingCredentials: credentials);
    }

    public RefreshToken CreateRefreshToken(int userId) => new()
    {
        Token = GenerateSecureRefreshToken(),
        Expires = DateTime.UtcNow.AddDays(7),
        UserId = userId
    };

    public CookieOptions CreateRefreshTokenCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = !_environment.IsDevelopment(),
        SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
        Expires = DateTime.UtcNow.AddDays(7)
    };

    public DateTime GetExpiresUtc() => DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}