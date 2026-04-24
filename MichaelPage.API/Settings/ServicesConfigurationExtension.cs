using FluentValidation;
using FluentValidation.AspNetCore;
using MichaelPage.API.Filters;
using MichaelPage.Application.Mappers;
using MichaelPage.Application.Tasks;
using MichaelPage.Application.Tasks.Dto;
using MichaelPage.Application.Users;
using MichaelPage.Application.Users.Dto;
using MichaelPage.Common.Models;
using MichaelPage.Common.Settings;
using MichaelPage.Core.Repositories;
using MichaelPage.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MichaelPage.API.Settings;

public static class ServicesConfigurationExtension
{
    public static void ConfigureApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IUserService, UserService>();
    }
    
    public static void ConfigureAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
            {
                cfg.LicenseKey = "";
            }, typeof(TaskMapperProfile), typeof(UserMapperProfile));
    }
    
    public static void ConfigureControllers(this IServiceCollection services)
    {
        services.AddControllers(options => { 
                options.Filters.Add(typeof(HttpGlobalExceptionFilter));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<HttpGlobalExceptionFilter>>();
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest
                    };

                    logger.LogWarning("Bad request - Validation error: {Errors}", problemDetails.Errors);

                    return new BadRequestObjectResult(Result.Fail(problemDetails.Errors));
                };
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });
    }
    
    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
    
    public static void ConfigureSqlServerSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlServerSettings = new SqlServerSettings();
        configuration.Bind(nameof(sqlServerSettings), sqlServerSettings);
        //sqlServerSettings.ConnectionString = "\"Data Source=database.windows.net;Initial Catalog=database-name;persist security info=True;user id=michael_page_app;password=Tr0n@Secure#2026!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;\""
        services.AddSingleton(sqlServerSettings);
    }
    
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen();
    }
    
    public static void ConfigureValidators(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();

        services.AddScoped<IValidator<CreateTaskDto>, CreateTaskValidation>();
        services.AddScoped<IValidator<CreateUserDto>, CreateUserValidation>();
    }
}