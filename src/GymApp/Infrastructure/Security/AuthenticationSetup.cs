using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GymApp.Infrastructure.Security
{
    public static class AuthenticationSetup
    {
        public static void SetupAuthenticationWithAuth0(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
                {
                    c.Authority = $"https://{builder.Configuration["Auth0:Domain"]}";
                    var validAudience = builder.Configuration["Auth0:Audience"];
                    var validIssuer = $"{builder.Configuration["Auth0:Domain"]}";
                    c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidAudience = validAudience,
                        ValidIssuer = validIssuer
                    };
                });
            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy("exerciseapp:read-write", p => p.RequireAuthenticatedUser().
                    RequireClaim("scope", "exerciseapp:read-write"));
            });
        }
    }
}
