namespace AxisUtah.Api.Features.Authentication;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AuthService authService,
    AuthTokenService tokenService) : ControllerBase
{
    private readonly AuthService _authService = authService;
    private readonly AuthTokenService _tokenService = tokenService;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest model)
    {
        var (success, message, user) = await _authService.RegisterAsync(model.Email, model.Password);
        if (!success)
            return BadRequest(new { message });

        var (tokenSuccess, refreshToken) = await _authService.CreateRefreshTokenAsync(user!.UserId);
        if (!tokenSuccess)
            return BadRequest(new { message = "Failed to create refresh token" });

        Response.Cookies.Append("refreshToken", refreshToken!.Token, _tokenService.CreateRefreshTokenCookieOptions());

        return Ok(new
        {
            message = "Registration successful",
            token = new JwtSecurityTokenHandler().WriteToken(_authService.GenerateJwtToken(user)),
            Token_type = "Bearer",
            expires_utc = _authService.GetTokenExpiresUtc()
        });
    }

    [AllowAnonymous]
    [HttpPost("issuetoken")]
    public async Task<IActionResult> IssueToken([FromBody] LoginRequest request)
    {
        var (success, message, user) = await _authService.AuthenticateAsync(request.Email, request.Password);
        if (!success)
            return Unauthorized(new { message });

        var (tokenSuccess, refreshToken) = await _authService.CreateRefreshTokenAsync(user!.UserId);
        if (!tokenSuccess)
            return BadRequest(new { message = "Failed to create refresh token" });

        Response.Cookies.Append("refreshToken", refreshToken!.Token, _tokenService.CreateRefreshTokenCookieOptions());

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(_authService.GenerateJwtToken(user)),
            Token_type = "Bearer",
            expires_utc = _authService.GetTokenExpiresUtc()
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            return Unauthorized(new { message = "Refresh token is missing" });

        var (success, message, user, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken);
        if (!success)
            return Unauthorized(new { message });

        Response.Cookies.Append("refreshToken", newRefreshToken!.Token, _tokenService.CreateRefreshTokenCookieOptions());

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(_authService.GenerateJwtToken(user!)),
            Token_type = "Bearer",
            expires_utc = _authService.GetTokenExpiresUtc()
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            await _authService.LogoutAsync(refreshToken);

        Response.Cookies.Delete("refreshToken", _tokenService.CreateRefreshTokenCookieOptions());

        return Ok(new { message = "Logged out successfully" });
    }
}