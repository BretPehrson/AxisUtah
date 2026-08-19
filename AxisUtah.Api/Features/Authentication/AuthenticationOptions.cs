namespace AxisUtah.Api.Features.Authentication;

public static class AuthenticationOptions
{
    public static Microsoft.AspNetCore.Authentication.AuthenticationBuilder GetAuthenticationOptions(this WebApplicationBuilder builder, JwtOption jwtOptions)
    {
        return builder.Services.AddAuthentication(options =>
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
    }
}