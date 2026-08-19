var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerOptions.GetSwaggerOptions());

builder.Services.AddScoped<ListingSyncCoordinator>();
builder.Services.AddScoped<AppLogService>();
builder.Services.AddScoped<AuthTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IBrokerageService, BrokerageService>();

builder.Services.AddReactCORS();

builder.Services.Configure<JwtOption>(builder.Configuration.GetSection("Jwt"));
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOption>() ?? throw new InvalidOperationException("JWT Settings are missing.");
builder.GetAuthenticationOptions(jwtOptions);
builder.Services.AddODataSyncService(builder.Configuration);


// if (builder.Configuration.GetValue<bool?>("MlsSync:RunInBackground") != false)
// {
//     builder.Services.AddHostedService<ListingSyncBackgroundService>();
// }

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map internal sync endpoints
app.MapInternalSyncEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();