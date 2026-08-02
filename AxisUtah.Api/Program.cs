var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<ListingSyncCoordinator>();
builder.Services.AddScoped<AppLogService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.Configure<JwtOption>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOption>() ?? throw new InvalidOperationException("JWT Settings are missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.IncludeErrorDetails = true; // Dev, off for Production

    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtOptions.Key)),
        
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),

        RoleClaimType = "role",
        NameClaimType = "name"
    };
});

builder.Services.AddHttpClient<ODataSyncService>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var baseUrl = config["MlsApi:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("MlsApi:BaseUrl must be configured.");
    }

    client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/");

    var token = config["MlsApi:BearerToken"];

    if (!string.IsNullOrWhiteSpace(token))
    {
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
});


// if (builder.Configuration.GetValue<bool?>("MlsSync:RunInBackground") != false)
// {
//     builder.Services.AddHostedService<ListingSyncBackgroundService>();
// }

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("===> Attempting to connect to database...");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        logger.LogInformation("===> Database connection successful!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "===> DATABASE CONNECTION FAILED!");
        throw; // Re-throw so the app stops with full stack trace
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.MapPost("/internal/sync/listings", async (
//     HttpRequest request,
//     ListingSyncCoordinator coordinator,
//     AppLogService appLogService,
//     IConfiguration configuration,
//     ILoggerFactory loggerFactory,
//     CancellationToken cancellationToken) =>
// {
//     var logger = loggerFactory.CreateLogger("InternalSyncEndpoint");
//     var expectedApiKey = configuration["SyncAutomation:ApiKey"];
//     var correlationId = request.Headers["X-Correlation-Id"].ToString();
//     if (string.IsNullOrWhiteSpace(correlationId))
//     {
//         correlationId = Guid.NewGuid().ToString("n");
//     }

//     if (string.IsNullOrWhiteSpace(expectedApiKey))
//     {
//         logger.LogError("Sync automation endpoint called before SyncAutomation:ApiKey was configured.");
//         await appLogService.WriteAsync(
//             level: "Error",
//             category: "InternalSyncEndpoint",
//             eventType: "SyncConfigurationMissing",
//             message: "Sync automation endpoint called before SyncAutomation:ApiKey was configured.",
//             source: "AxisUtah.Api",
//             correlationId: correlationId,
//             cancellationToken: cancellationToken);

//         return Results.Problem(
//             detail: "Sync automation key is not configured.",
//             statusCode: StatusCodes.Status503ServiceUnavailable);
//     }

//     if (!request.Headers.TryGetValue("X-Sync-Api-Key", out var providedApiKey) ||
//         !string.Equals(providedApiKey.ToString(), expectedApiKey, StringComparison.Ordinal))
//     {
//         logger.LogWarning("Rejected internal listing sync request due to invalid automation key.");
//         await appLogService.WriteAsync(
//             level: "Warning",
//             category: "InternalSyncEndpoint",
//             eventType: "SyncAuthorizationFailed",
//             message: "Rejected internal listing sync request due to invalid automation key.",
//             source: "AxisUtah.Api",
//             correlationId: correlationId,
//             cancellationToken: cancellationToken);

//         return Results.Unauthorized();
//     }

//     logger.LogInformation("Accepted internal listing sync request.");
//     await appLogService.WriteAsync(
//         level: "Information",
//         category: "InternalSyncEndpoint",
//         eventType: "SyncRequestAccepted",
//         message: "Accepted internal listing sync request.",
//         source: "AxisUtah.Api",
//         correlationId: correlationId,
//         cancellationToken: cancellationToken);

//     await coordinator.RunPropertySyncAsync(correlationId, cancellationToken);

//     logger.LogInformation("Internal listing sync request completed successfully.");
//     await appLogService.WriteAsync(
//         level: "Information",
//         category: "InternalSyncEndpoint",
//         eventType: "SyncRequestCompleted",
//         message: "Internal listing sync request completed successfully.",
//         source: "AxisUtah.Api",
//         correlationId: correlationId,
//         cancellationToken: cancellationToken);

//     return Results.Ok(new { status = "completed" });
// });

// app.MapPost("/internal/logs", async (
//     HttpRequest request,
//     LogEntryDto logEntry,
//     AppLogService appLogService,
//     IConfiguration configuration,
//     CancellationToken cancellationToken) =>
// {
//     var expectedApiKey = configuration["SyncAutomation:ApiKey"];

//     if (string.IsNullOrWhiteSpace(expectedApiKey))
//     {
//         return Results.Problem(
//             detail: "Sync automation key is not configured.",
//             statusCode: StatusCodes.Status503ServiceUnavailable);
//     }

//     if (!request.Headers.TryGetValue("X-Sync-Api-Key", out var providedApiKey) ||
//         !string.Equals(providedApiKey.ToString(), expectedApiKey, StringComparison.Ordinal))
//     {
//         return Results.Unauthorized();
//     }

//     await appLogService.WriteAsync(
//         logEntry.Level,
//         logEntry.Category,
//         logEntry.EventType,
//         logEntry.Message,
//         logEntry.Details,
//         logEntry.Source,
//         logEntry.CorrelationId,
//         cancellationToken);

//     return Results.Accepted();
// });

// app.MapGet("/admin/logs", async (
//     AppLogService appLogService,
//     string? level,
//     string? source,
//     string? eventType,
//     string? correlationId,
//     string? search,
//     DateTimeOffset? createdAfterUtc,
//     DateTimeOffset? createdBeforeUtc,
//     int? skip,
//     int? take,
//     CancellationToken cancellationToken) =>
// {
//     var entries = await appLogService.QueryAsync(
//         level,
//         source,
//         eventType,
//         correlationId,
//         search,
//         createdAfterUtc,
//         createdBeforeUtc,
//         skip ?? 0,
//         take ?? 100,
//         cancellationToken);

//     return Results.Ok(entries);
// });

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
    
app.Run();