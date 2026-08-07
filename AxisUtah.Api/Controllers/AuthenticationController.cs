namespace AxisUtah.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IDbContextFactory<AppDbContext> dbContextFactory,
    IOptions<JwtOption> jwtOptions,
    IWebHostEnvironment environment) : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    private readonly JwtOption _jwtOptions = jwtOptions.Value;
    private readonly IWebHostEnvironment _environment = environment;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest model)
    {
        if ( model.Email == null || model.Password == null)
            return BadRequest(new { message = "Email and password are required." });

        using var context = _dbContextFactory.CreateDbContext();

        if (context.Users.Any(u => u.Email == model.Email))
            return BadRequest(new { message = "Email is already registered." });

        var user = new User
        {
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Generate token and refresh token immediately
        var refreshToken = new RefreshToken
        {
            Token = GenerateSecureRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = user.UserId
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        SetRefreshTokenCookie(refreshToken.Token);

        return Ok(new
        {
            message = "Registration successful.",
            token = new JwtSecurityTokenHandler().WriteToken(GenerateJwtToken(user)),
            Token_type = "Bearer",
            expires_utc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes)
        });
    }

    [AllowAnonymous]
    [HttpPost("issuetoken")]
    public async Task<IActionResult> IssueToken([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        using var context = _dbContextFactory.CreateDbContext();
        
        var user = await context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new { message = "Invalid username or password." });

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
        if (!isPasswordValid) return Unauthorized(new { message = "Invalid username or password." });

        var newRefreshToken = new RefreshToken
        {
            Token = GenerateSecureRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = user.UserId
        };

        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        SetRefreshTokenCookie(newRefreshToken.Token);

        return Ok(new 
        {
             token = new JwtSecurityTokenHandler().WriteToken(GenerateJwtToken(user)),
             Token_type = "Bearer",
             expires_utc = GetExpiresUtc()
        });
    }


    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token is missing." });

        using var context = _dbContextFactory.CreateDbContext();
        
        RefreshToken? storedToken = context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefault(rt => rt.Token == refreshToken);

        if (storedToken == null || storedToken.User == null || !storedToken.IsActive)
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        
        var newRefreshToken = new RefreshToken
        {
            Token = GenerateSecureRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(7),
            UserId = storedToken.User.UserId
        };
        context.RefreshTokens.Remove(storedToken);
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        SetRefreshTokenCookie(newRefreshToken.Token);
        
        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(GenerateJwtToken(storedToken.User)),
            Token_type = "Bearer",
            expires_utc = GetExpiresUtc()
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            using var context = _dbContextFactory.CreateDbContext();

            var storedToken = await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken != null)            
            {
                context.RefreshTokens.Remove(storedToken);
                await context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict
        });

        return Ok(new { message = "Logged out successfully." });
    }

    private JwtSecurityToken GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Convert.FromBase64String(_jwtOptions.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: GetExpiresUtc(),
            signingCredentials: credentials
        );
    }

    private DateTime GetExpiresUtc()
    {
        return DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);
    }

    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = !_environment.IsDevelopment() ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}