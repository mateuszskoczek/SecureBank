using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecureBank.API.Authentication;
using SecureBank.API.Encryption;
using SecureBank.Components;
using SecureBank.Database;
using SecureBank.Website.API;
using SecureBank.Website.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace SecureBank;

public class Program
{
    #region FIELDS

    protected static WebApplicationBuilder _builder;

    #endregion



    #region PUBLIC METHODS

    public static void Main(string[] args)
    {
        _builder = WebApplication.CreateBuilder(args);

        // Logging
        _builder.Logging.ClearProviders();
        _builder.Logging.AddConsole();

        // Database
        _builder.Services.AddDbContext<DatabaseContext>(x =>
        {
            x.UseSqlite(_builder.Configuration.GetConnectionString("Default"));
        }, ServiceLifetime.Singleton);

        // API
        BuildAPI();

        // Website
        BuildWebsite();



        var app = _builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
        else
        {
            app.UseSwagger(x =>
            {
                x.RouteTemplate = "api/swagger/{documentname}/swagger.json";
            });
            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/api/swagger/v1/swagger.json", "SecureBank API");
                x.RoutePrefix = "api/swagger";
            });
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<DatabaseContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
        }

        app.Run();
    }

    #endregion



    #region PRIVATE METHODS

    protected static void BuildAPI()
    {
        _builder.Services.AddControllers();
        _builder.Services.AddEndpointsApiExplorer();
        _builder.Services.AddSwaggerGen();

        // Authentication
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        AuthenticationBuilder auth = _builder.Services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });
        auth.AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_builder.Configuration.GetValue<string>("Authentication:Token:Key"))),
                ValidIssuer = _builder.Configuration.GetValue<string>("Authentication:Token:Issuer"),
                ValidAudience = _builder.Configuration.GetValue<string>("Authentication:Token:Audience"),
                ClockSkew = TimeSpan.FromMinutes(1),
            };
            x.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });
        _builder.Services.AddAuthorization();

        // Configurations
        _builder.Services.AddSingleton<AuthenticationConfiguration>();
        _builder.Services.AddSingleton<EncryptionConfiguration>();

        // Helpers
        _builder.Services.AddSingleton<API.Authentication.AuthenticationHelper>();
        _builder.Services.AddSingleton<API.Encryption.EncryptionHelper>();

        // Services
        _builder.Services.AddSingleton<API.Services.IAccountsService, API.Services.AccountsService>();
        _builder.Services.AddSingleton<API.Services.IBalanceService, API.Services.BalanceService>();
        _builder.Services.AddSingleton<API.Services.ITransfersService, API.Services.TransfersService>();
    }

    protected static void BuildWebsite()
    {
        _builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Clients
        _builder.Services.AddSingleton<HttpClient>();
        _builder.Services.AddSingleton<APIClient>();

        // Storage
        _builder.Services.AddBlazoredSessionStorage();

        // Authentication
        _builder.Services.AddScoped<Website.Authentication.AuthenticationHelper>();
        _builder.Services.AddAuthorizationCore();
        _builder.Services.AddScoped<TokenAuthenticationStateProvider>();

        // Configurations
        _builder.Services.AddSingleton<APIEndpointsConfiguration>();

        // Services
        _builder.Services.AddSingleton<Website.Services.IAccountsService, Website.Services.AccountsService>();
        _builder.Services.AddSingleton<Website.Services.IBalanceService, Website.Services.BalanceService>();
        _builder.Services.AddSingleton<Website.Services.ITransfersService, Website.Services.TransfersService>();
    }

    #endregion
}
