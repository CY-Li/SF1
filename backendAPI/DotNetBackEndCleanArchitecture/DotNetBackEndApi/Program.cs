using DomainEntityDTO.Common;
using DotNetBackEndApi.Event;
using DotNetBackEndApi.Filters;
using DotNetBackEndApi.Shared;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.MySql;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("./logs/webapi_start_information_.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

#region serilog
// ... (omitted for brevity)
#endregion
try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.With(new LogEnricher())
        .WriteTo.Console()
        .WriteTo.File("./logs/webapi_all_.log",
            rollingInterval: RollingInterval.Day)
        .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(e =>
            e.Properties.ContainsKey("ControllerName")
            && (e.Properties["ControllerName"]?.ToString() ?? "").Contains("Controller"))
            .WriteTo.Map("ControllerName", "Other", (ControllerName, wt) =>
            wt.File($"logs/webapi_{ControllerName}_log_.log", rollingInterval: RollingInterval.Day))
        )
    );

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.Configure<ApiAppConfig>(builder.Configuration); 
    var mAppSetting = builder.Configuration.Get<ApiAppConfig>(); 

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy("API Gateway is running"));

    builder.Services.AddHttpClient<HttpClientService>(c =>
    {
        c.BaseAddress = new Uri(mAppSetting.APIUrl);
        c.Timeout = TimeSpan.FromSeconds(600);
    });

    #region Token filter
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
         {
             options.IncludeErrorDetails = true; 

             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = mAppSetting.JwtSettings.ValidateIssuer,
                 ValidIssuer = mAppSetting.JwtSettings.Issuer,

                 ValidateAudience = mAppSetting.JwtSettings.ValidateAudience,
                 ValidAudience = mAppSetting.JwtSettings.Audience,

                 ValidateIssuerSigningKey = mAppSetting.JwtSettings.ValidateIssuerSigningKey,
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mAppSetting.JwtSettings.SecurityKey)),

                 ValidateLifetime = mAppSetting.JwtSettings.ValidateLifetime,
                 ClockSkew = TimeSpan.Zero 
             };
         });
    #endregion

    builder.Services.AddMvc(options =>
    {
        options.Filters.Add(new ApiResourceFilter());
        options.Filters.Add(typeof(ApiActionFilter));
    });

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtDemo", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter JWT with Bearer into field",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                    },
                new string[] {}
            }
        });

        c.EnableAnnotations();
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            build =>
            {
                build.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
    });

    var app = builder.Build();
    app.UseCors("CorsPolicy");

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapControllers();

    app.Run();

    Log.Information("Starting web application completed");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}