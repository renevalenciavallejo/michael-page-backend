using MichaelPage.API.Settings;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting MichalePage.API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((_, lc) => lc
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .WriteTo.Console()
        .WriteTo.File("./logs/error-.txt", LogEventLevel.Warning, retainedFileCountLimit: 10,
            rollingInterval: RollingInterval.Day)
        .WriteTo.File("./logs/info-.txt", LogEventLevel.Information, retainedFileCountLimit: 10,
            rollingInterval: RollingInterval.Day));
    
    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .WithOrigins("http://localhost:4200").AllowAnyHeader()
                    .AllowAnyMethod().AllowCredentials();
            });
    });
    
    // Others
    builder.Services.ConfigureControllers();
    builder.Services.ConfigureAutoMapper();
    builder.Services.ConfigureValidators();
    builder.Services.ConfigureSqlServerSettings(builder.Configuration);
    
    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.ConfigureSwagger();
    
    // Http
    builder.Services.AddHttpClient();
    builder.Services.AddRouting(options => options.LowercaseUrls = true);
    
    // Dependency injection
    builder.Services.ConfigureRepositories();
    builder.Services.ConfigureApplicationServices();
    
    var app = builder.Build();

    var isSwaggerEnabled = builder.Configuration["IsSwaggerEnabled"]?.ToLower() == "true";    
    
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment() || isSwaggerEnabled)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // app.UseHttpLogging();
    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseCors();
    app.MapControllers();
    
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}