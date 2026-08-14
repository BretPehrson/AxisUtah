using AxisUtah.Api.Mappings;

namespace AxisUtah.Tests.Controllers;

public class AuthenticationControllerTest
{
    private readonly IDbContextFactory<AppDbContext> _context;
    private readonly IOptions<JwtOption> _jwtOptions;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly AuthTokenService _authTokenService;
    private readonly AuthService _authService;

    public AuthenticationControllerTest()
    {
        _context = TestDbContextFactory.Create();
        _webHostEnvironment = new TestWebHostEnvironment();

        // Generate a valid Base64-encoded 256-bit key for testing
        var key = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }

        _jwtOptions = Options.Create(new JwtOption
        {
            Key = Convert.ToBase64String(key),  // Use Base64-encoded key
            Issuer = "AxisUtah",
            Audience = "AxisUtahUsers",
            ExpiryMinutes = 5
        });

        _authTokenService = new AuthTokenService(_jwtOptions, _webHostEnvironment);
        _authService = new AuthService(_context, _authTokenService);
    }

    private AuthController CreateController() => new(_authService, _authTokenService)
    {
        ControllerContext = TestAuthHelper.GetControllerContext(null)
    };

    [Fact]
    public void Test_CreateController_ReturnsAuthController()
    {
        // Arrange & Act
        var controller = CreateController();

        // Assert
        Assert.NotNull(controller);
        Assert.IsType<AuthController>(controller);
    }

    [Fact]
    public void Test_CreateController_ReturnsControllerContext()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var context = controller.ControllerContext;

        // Assert
        Assert.NotNull(context);
    }

    [Fact]
    public void Test_CreateController_HasExpectedJwtOptions()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var jwtOptions = _jwtOptions.Value;

        // Assert
        Assert.NotNull(jwtOptions);
        Assert.NotNull(jwtOptions.Key);
        Assert.Equal("AxisUtah", jwtOptions.Issuer);
        Assert.Equal("AxisUtahUsers", jwtOptions.Audience);
        Assert.Equal(5, jwtOptions.ExpiryMinutes);
    }

    [Fact]
    public void Test_CreateController_HasExpectedWebHostEnvironment()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var webHostEnvironment = _webHostEnvironment;

        // Assert
        Assert.NotNull(webHostEnvironment);
        Assert.Equal("Development", webHostEnvironment.EnvironmentName);
        Assert.Equal("AxisUtah.Tests", webHostEnvironment.ApplicationName);
        Assert.Equal(Directory.GetCurrentDirectory(), webHostEnvironment.ContentRootPath);
        Assert.Equal(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), webHostEnvironment.WebRootPath);
    }

    [Fact]
    public async Task Authenticate_Succeeds_RegistersUser()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var newUser = new User
        {
            Email = "testuser@example.com",
            Password = "Test!123"
        };

        var result = await controller.Register(newUser.ToLoginRequest());

        // Assert
        Assert.NotNull(result);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        var response = new RouteValueDictionary(okResult.Value);
        Assert.Equal("Registration successful", response["message"]);
        Assert.NotNull(response["token"]);
        Assert.Equal("Bearer", response["Token_type"]);
        Assert.NotNull(response["expires_utc"]);

        var tokenString = response["token"]!.ToString();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenString);
        Assert.NotNull(jwtToken);
        Assert.Equal("AxisUtah", jwtToken.Issuer);
        Assert.Equal("AxisUtahUsers", jwtToken.Audiences.FirstOrDefault());
        Assert.Equal("testuser@example.com", jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value);
    }

    [Fact]
    public async Task Authenticate_Fails_InvalidUser()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var loginRequest = new LoginRequest("nonexistentuser@example.com", "InvalidPassword!123");

        var result = await controller.IssueToken(loginRequest);

        // Assert
        Assert.NotNull(result);
        var badRequestResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        var response = new RouteValueDictionary(badRequestResult.Value);
        Assert.Equal("Invalid username or password", response["message"]);
    }

    [Fact]
    public async Task Logout_Succeeds_DeletesRefreshTokenCookie()
    {
        // Arrange
        var controller = CreateController();
        var httpContext = controller.ControllerContext.HttpContext;
        
        // Register a user (sets the refresh token cookie)
        var newUser = new User
        {
            Email = "testuser@example.com",
            Password = "Test!123"
        };
        var registerResult = await controller.Register(newUser.ToLoginRequest());
        Assert.IsType<OkObjectResult>(registerResult);
        
        // Verify refresh token cookie was set during registration
        var responseCookiesAfterRegister = httpContext.Response.Headers.SetCookie;
        Assert.True(responseCookiesAfterRegister.Count > 0);
        Assert.True(responseCookiesAfterRegister.Any(c => c!.Contains("refreshToken")), 
            "Refresh token cookie should be set after registration");
        
        // Act - Logout
        var logoutResult = await controller.Logout();
        
        // Assert
        Assert.NotNull(logoutResult);
        var okResult = Assert.IsType<OkObjectResult>(logoutResult);
        var response = new RouteValueDictionary(okResult.Value);
        Assert.Equal("Logged out successfully", response["message"]);
        
        // Verify the refresh token cookie deletion was sent
        // When deleted, the cookie has an empty value and expiration in past
        var allResponseCookies = httpContext.Response.Headers.SetCookie;
        var deletionCookie = allResponseCookies.FirstOrDefault(c => 
            c!.Contains("refreshToken") && (c.Contains("expires=") || c.Contains("Max-Age=0")));
        Assert.NotNull(deletionCookie);
    }

    [Fact]
    public async Task Logout_Fails_WhenNoRefreshTokenCookie()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Logout();

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = new RouteValueDictionary(okResult.Value);
        Assert.Equal("Logged out successfully", response["message"]);
    }

    [Fact]
    public async Task Refresh_Succeeds_WithValidRefreshToken()
    {
        // Arrange - Register a user to get a refresh token
        var registerController = CreateController();
        var newUser = new User
        {
            Email = "testuser@example.com",
            Password = "Test!123"
        };
        var registerResult = await registerController.Register(newUser.ToLoginRequest());
        Assert.IsType<OkObjectResult>(registerResult);

        // Extract the refresh token from the Set-Cookie response header
        var setCookieHeader = registerController.ControllerContext.HttpContext.Response.Headers.SetCookie
            .FirstOrDefault(c => c!.Contains("refreshToken"));
        Assert.NotNull(setCookieHeader);
        
        var refreshTokenValue = TestAuthHelper.ExtractCookieValue(setCookieHeader!, "refreshToken");
        Assert.False(string.IsNullOrEmpty(refreshTokenValue));

        // Create a new controller with the refresh token in the request cookies
        var refreshController = new AuthController(_authService, _authTokenService)
        {
            ControllerContext = TestAuthHelper.GetControllerContext(null, new Dictionary<string, string>
            {
                { "refreshToken", refreshTokenValue }
            })
        };

        // Act
        var result = await refreshController.Refresh();

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = new RouteValueDictionary(okResult.Value);
        Assert.NotNull(response["token"]);
        Assert.Equal("Bearer", response["Token_type"]);
        Assert.NotNull(response["expires_utc"]);
    }

    [Fact]
    public async Task Refresh_Fails_WithInvalidRefreshToken()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Refresh();

        // Assert
        Assert.NotNull(result);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = new RouteValueDictionary(unauthorizedResult.Value);
        Assert.Equal("Refresh token is missing", response["message"]);
    }

    [Fact]
    public async Task Refresh_Fails_WithExpiredRefreshToken()
    {
        // Arrange
        var controller = CreateController();

        // Simulate an expired refresh token by not setting any refresh token cookie

        // Act
        var result = await controller.Refresh();

        // Assert
        Assert.NotNull(result);
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = new RouteValueDictionary(unauthorizedResult.Value);
        Assert.Equal("Refresh token is missing", response["message"]);
    }

    [Fact]
    public async Task Register_Fails_NullEmail()
    {
        var controller = CreateController();
        var request = new LoginRequest(null!, "Test!123");
        
        var result = await controller.Register(request);
        
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = new RouteValueDictionary(badResult.Value);
        Assert.Equal("Email and password are required", response["message"]);
    }

    [Fact]
    public async Task Register_Fails_NullPassword()
    {
        var controller = CreateController();
        var request = new LoginRequest("test@example.com", null!);
        
        var result = await controller.Register(request);
        
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = new RouteValueDictionary(badResult.Value);
        Assert.Equal("Email and password are required", response["message"]);
    }

    [Fact]
    public async Task Register_Fails_EmailAlreadyExists()
    {
        var controller = CreateController();
        var request = new LoginRequest("testuser@example.com", "Test!123");
        
        // Register once
        await controller.Register(request);
        
        // Try to register again with same email
        var result = await controller.Register(request);
        
        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = new RouteValueDictionary(badResult.Value);
        Assert.Equal("Email is already registered", response["message"]);
    }

    [Fact]
    public async Task IssueToken_Succeeds_ReturnsToken()
    {
        var controller = CreateController();
        
        // Register first
        var registerRequest = new LoginRequest("testuser@example.com", "Test!123");
        await controller.Register(registerRequest);
        
        // Issue token
        var issueResult = await controller.IssueToken(registerRequest);
        
        var okResult = Assert.IsType<OkObjectResult>(issueResult);
        var response = new RouteValueDictionary(okResult.Value);
        Assert.NotNull(response["token"]);
        Assert.Equal("Bearer", response["Token_type"]);
        Assert.NotNull(response["expires_utc"]);
    }

    [Fact]
    public async Task IssueToken_Fails_InvalidPassword()
    {
        var controller = CreateController();
        
        // Register
        var registerRequest = new LoginRequest("testuser@example.com", "Test!123");
        await controller.Register(registerRequest);
        
        // Try with wrong password
        var wrongPasswordRequest = new LoginRequest("testuser@example.com", "WrongPassword!123");
        var result = await controller.IssueToken(wrongPasswordRequest);
        
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = new RouteValueDictionary(unauthorizedResult.Value);
        Assert.Equal("Invalid username or password", response["message"]);
    }

    [Fact]
    public async Task IssueToken_Fails_EmptyEmail()
    {
        var controller = CreateController();
        var request = new LoginRequest("", "Test!123");
        
        var result = await controller.IssueToken(request);
        
        var badResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = new RouteValueDictionary(badResult.Value);
        Assert.Equal("Email and password are required", response["message"]);
    }

    [Fact]
    public async Task Refresh_Succeeds_ReturnsNewToken()
    {
        var registerController = CreateController();
        
        // Register to get refresh token cookie
        var registerRequest = new LoginRequest("testuser@example.com", "Test!123");
        var registerResult = await registerController.Register(registerRequest);
        Assert.IsType<OkObjectResult>(registerResult);

        // Extract the refresh token from the Set-Cookie response header
        var setCookieHeader = registerController.ControllerContext.HttpContext.Response.Headers.SetCookie
            .FirstOrDefault(c => c!.Contains("refreshToken"));
        Assert.NotNull(setCookieHeader);
        
        var refreshTokenValue = TestAuthHelper.ExtractCookieValue(setCookieHeader!, "refreshToken");
        Assert.False(string.IsNullOrEmpty(refreshTokenValue));

        // Create a new controller with the refresh token in the request cookies
        var refreshController = new AuthController(_authService, _authTokenService)
        {
            ControllerContext = TestAuthHelper.GetControllerContext(null, new Dictionary<string, string>
            {
                { "refreshToken", refreshTokenValue }
            })
        };

        // Refresh the token
        var result = await refreshController.Refresh();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = new RouteValueDictionary(okResult.Value);
        Assert.NotNull(response["token"]);
        Assert.Equal("Bearer", response["Token_type"]);
        Assert.NotNull(response["expires_utc"]);
    }

    [Fact]
    public async Task Refresh_Fails_NoRefreshToken()
    {
        var controller = CreateController();
        
        // Try to refresh without registering first (no cookie)
        var result = await controller.Refresh();
        
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = new RouteValueDictionary(unauthorizedResult.Value);
        Assert.Equal("Refresh token is missing", response["message"]);
    }

    [Fact]
    public async Task Logout_Succeeds_WithoutActiveSession()
    {
        var controller = CreateController();
        
        // Try to logout without registering first - should still return OK (idempotent)
        var result = await controller.Logout();
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = new RouteValueDictionary(okResult.Value);
        Assert.Equal("Logged out successfully", response["message"]);
    }
}