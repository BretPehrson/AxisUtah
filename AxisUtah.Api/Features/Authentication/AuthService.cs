namespace AxisUtah.Api.Features.Authentication;

public class AuthService(
    IDbContextFactory<AppDbContext> dbContextFactory,
    AuthTokenService tokenService)
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory = dbContextFactory;
    private readonly AuthTokenService _tokenService = tokenService;

    public async Task<(bool Success, string Message, User? User)> RegisterAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return (false, "Email and password are required", null);

        using var context = _dbContextFactory.CreateDbContext();

        if (await context.Users.AnyAsync(u => u.Email == email))
            return (false, "Email is already registered", null);

        var user = new User
        {
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (true, "Registration successful", user);
    }

    public async Task<(bool Success, string Message, User? User)> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return (false, "Email and password are required", null);

        using var context = _dbContextFactory.CreateDbContext();

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            return (false, "Invalid username or password", null);

        return (true, "Authentication successful", user);
    }

    public async Task<(bool Success, RefreshToken? Token)> CreateRefreshTokenAsync(int userId)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var refreshToken = _tokenService.CreateRefreshToken(userId);
        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return (true, refreshToken);
    }

    public async Task<(bool Success, string Message, User? User, RefreshToken? Token)> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return (false, "Refresh token is missing", null, null);

        using var context = _dbContextFactory.CreateDbContext();

        var storedToken = await context.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.Token == refreshToken);

        if (storedToken?.User == null || !storedToken.IsActive)
            return (false, "Invalid or expired refresh token", null, null);

        var newRefreshToken = _tokenService.CreateRefreshToken(storedToken.User.UserId);
        context.RefreshTokens.Remove(storedToken);
        context.RefreshTokens.Add(newRefreshToken);
        await context.SaveChangesAsync();

        return (true, "Token refreshed successfully", storedToken.User, newRefreshToken);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;

        using var context = _dbContextFactory.CreateDbContext();

        var storedToken = await context.RefreshTokens.FirstOrDefaultAsync(token => token.Token == refreshToken);
        if (storedToken == null)
            return;

        context.RefreshTokens.Remove(storedToken);
        await context.SaveChangesAsync();
    }

    public JwtSecurityToken GenerateJwtToken(User user) => _tokenService.GenerateJwtToken(user);

    public DateTime GetTokenExpiresUtc() => _tokenService.GetExpiresUtc();
}