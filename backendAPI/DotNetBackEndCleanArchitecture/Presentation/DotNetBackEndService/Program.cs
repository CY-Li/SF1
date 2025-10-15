
using Serilog.Events;
using Serilog;
using DomainEntity.Common;
using InfraCommon.DBA;
using AutoMapper;
using InfraCommon.MapperServices.AutoMapperProfile;
using DotNetBackEndService.DI;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("./logs/web_svc_start_information.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting services application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File("./logs/web_svc_all.log", rollingInterval: RollingInterval.Day)
    );

    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.Configure<AppConfig>(builder.Configuration); // 讓config有強型別

    #region Service DI 
    builder.Services.AddInfraCommon();
    builder.Services.AddAuthenticationMgmtServices();
    builder.Services.AddFinancialMgmtServices();
    builder.Services.AddMemberMgmtServices();
    builder.Services.AddSystemMgmtServices();
    builder.Services.AddTenderMgmtServices();
    builder.Services.AddScheduleMgmtServices();
    #endregion Service DI END

    #region MappingsProfile
    //builder.Services.AddAutoMapper(config =>
    //{
    //    config.AddProfile<ControllerProfile>();
    //    config.AddProfile<ServiceProfile>();
    //});
    var _mapperConfig = new MapperConfiguration(config =>
    {
        //config.AddProfile<AuthnMappingsProfile>();
        config.AddProfile<AuthenticationMgmtMappingProfile>();
        //config.AddProfile<ROSCAMappingsProfile>();
    });
    IMapper _iMapperConfig = _mapperConfig.CreateMapper();
    builder.Services.AddSingleton(_iMapperConfig);
    #endregion END MappingsProfile

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    //app.UseExceptionHandleMiddleware();//加入這行即可完成設定

    // Configure the HTTP request pipeline.

    //app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

    Log.Information("Starting services application completed");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}