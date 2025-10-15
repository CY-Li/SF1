
using DomainEntityDTO.Common;
using DotNetBackEndApi.Event;
using DotNetBackEndApi.Filters;
using DotNetBackEndApi.Shared;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.MySql;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("./logs/webapi_start_information_.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

#region serilog �Ȯɤ���
//var _logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .Enrich.FromLogContext()
//    .WriteTo.Map("ControllerName", "Other", (ControllerName, wt) => wt.File($"Logs/log-{ControllerName}.log"))
//    .CreateLogger();

//Log.Logger = new LoggerConfiguration()
//    .WriteTo.("Name", "Other", (name, wt) => wt.File($"./logs/log-{name}.txt"))
//    .CreateLogger();

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.Logger(lc => lc
//        .Filter.ByExcluding(e => e.Properties.ContainsKey("ControllerName"))
//        .WriteTo.Sink(new ControllerLogSink()))
//    .CreateLogger();

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .WriteTo.Logger(lc => lc
//        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("ControllerName") && e.Properties["ControllerName"].ToString() == "YourControllerName")
//        .WriteTo.File("C:\\Logs\\YourControllerName\\{Date}.txt", rollingInterval: RollingInterval.Day))
//    .CreateLogger();

//Log.Logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.Logger(lc => lc.Filter.ByExcluding(Matching.FromSource("Microsoft")))
//    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Properties.TryGetValue("ControllerName", out var controller) && controller.ToString() == "HomeController")
//        .WriteTo.File("logs/home-controller-.txt", rollingInterval: RollingInterval.Day))
//    .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(evt => evt.Properties.TryGetValue("ControllerName", out var controller) && controller.ToString() == "OtherController")
//        .WriteTo.File("logs/other-controller-.txt", rollingInterval: RollingInterval.Day))

//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Information()
//    .Enrich.FromLogContext()
//    .WriteTo.Logger(lc => lc
//        .Filter.ByIncludingOnly(evt => evt.Properties.TryGetValue("ControllerName", out var controller) && controller.ToString() == "HomeController")
//        .WriteTo.File("logs/home-controller-.txt", rollingInterval: RollingInterval.Day))
//    .WriteTo.Logger(lc => lc
//        .Filter.ByIncludingOnly(evt => evt.Properties.TryGetValue("ControllerName", out var controller) && controller.ToString() == "OtherController")
//        .WriteTo.File("logs/other-controller-.txt", rollingInterval: RollingInterval.Day))
//    .CreateLogger();
#endregion
try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    //�ϥ�Serilog
    //builder.Host.UseSerilog((context, services, configuration) => configuration
    //    .MinimumLevel.Information()
    //    .WriteTo.Console()
    //    .WriteTo.Sink(new ControllerLogSink())
    //);
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
    builder.Services.Configure<ApiAppConfig>(builder.Configuration); // DI config�j���O
    var mAppSetting = builder.Configuration.Get<ApiAppConfig>(); // �bprogram�̨ϥ�config�j���O

    //�Ӧa��O�]�w�VService����ƪ��]�w(�N�O�b�ϥθ�service�ɷ|�۰ʥ[�J��URL)
    builder.Services.AddHttpClient<HttpClientService>(c =>
    {
        c.BaseAddress = new Uri(mAppSetting.APIUrl);
        c.Timeout = TimeSpan.FromSeconds(600);
    });

    #region Token �������filter
    //�]�wTOKEM����
    //builder.Services
    //    .AddAuthentication(options =>
    //    {
    //        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    //    })
    //    .AddJwtBearer(options =>
    //     {
    //         // �����ҥ��ѮɡA�^�����Y�|�]�t WWW-Authenticate ���Y�A�o�̷|��ܥ��Ѫ��Բӿ��~��]
    //         options.IncludeErrorDetails = true; // �w�]�Ȭ� true�A���ɷ|�S�O����

    //         options.TokenValidationParameters = new TokenValidationParameters
    //         {
    //             // �z�L�o���ŧi�A�N�i�H�q "NAME" ����
    //             //NameClaimType = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
    //             // �z�L�o���ŧi�A�N�i�H�q "Role" ���ȡA�åi�� [Authorize] �P�_����
    //             //RoleClaimType = @"http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

    //             // ���� Issuer(�o���) (�@�볣�|)
    //             ValidateIssuer = m_appSetting.JwtSettings.ValidateIssuer,
    //             ValidIssuer = m_appSetting.JwtSettings.Issuer,

    //             // ���� Audience (�q�`���ӻݭn)
    //             ValidateAudience = m_appSetting.JwtSettings.ValidateAudience,
    //             ValidAudience = m_appSetting.JwtSettings.Audience,

    //             // �p�G Token ���]�t key �~�ݭn���ҡA�@�볣�u��ñ���Ӥw
    //             ValidateIssuerSigningKey = m_appSetting.JwtSettings.ValidateIssuerSigningKey,
    //             // ���ӱq IConfiguration ���o
    //             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_appSetting.JwtSettings.SecurityKey)),

    //             // ���� Token �����Ĵ��� (�@�볣�|)
    //             ValidateLifetime = m_appSetting.JwtSettings.ValidateLifetime,
    //             ClockSkew = TimeSpan.Zero // �ɶ���A�N�L��
    //         };
    //     });
    #endregion

    //�]�w�Ҧ�API���n�Ϊ�Filter
    builder.Services.AddMvc(options =>
    {
        //�v�����ҡAJWT TOKEN
        //options.Filters.Add(new AuthorizeFilter());
        //options.Filters.Add(new BackEndApiAuthorizationFilter());

        //�i�JAction�P���}�ɥi�H�gLOG
        options.Filters.Add(new ApiResourceFilter());
        options.Filters.Add(typeof(ApiActionFilter));
    });

    //�]�w�ϥ�swagger���
    //builder.Services.AddSwaggerGen();
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

    //�B�z�����D
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

    //builder.Services.AddHangfire(config =>
    //{
    //    config.UseMemoryStorage();
    //});
    //builder.Services.AddHangfireServer();
    //builder.Services.AddTransient<IBackgroundJobClient, BackgroundJobClient>();

    var app = builder.Build();
    //app.UseHangfireDashboard();
    app.UseCors("CorsPolicy");

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    //�Ȯɳ��|�}�Ҳ��Xswagger���
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

    //GET�ɦV�Ȯɤ���
    //app.MapGet("/", () => @$"
    //Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}
    //MySetting: {m_appSetting.JwtSettings.SecurityKey}");

    //�o�������O�]���ڲ{�b�S��https
    //app.UseHttpsRedirection();// �ϥ��R�A�ɮסA�w�]�ؿ��O wwwroot

    app.UseAuthentication(); // ����

    app.UseAuthorization(); // ���v

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

