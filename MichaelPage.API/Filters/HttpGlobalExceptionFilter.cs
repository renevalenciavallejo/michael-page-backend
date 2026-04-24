using System.Net;
using MichaelPage.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MichaelPage.API.Filters;

public class HttpGlobalExceptionFilter : IExceptionFilter
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<HttpGlobalExceptionFilter> _logger;

    public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
    {
        _env = env;
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message);

        Dictionary<string, string[]> errorDetail = null;
        if (_env.IsDevelopment())
        {
            errorDetail = new Dictionary<string, string[]>
                {{context.Exception.Message, [context.Exception.ToString()]}};
        }

        var apiResponse = Result.Fail(errorDetail);
        apiResponse.Message = "An error occurred processing the request.";
            
        context.Result = new InternalServerErrorObjectResult(apiResponse);
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        context.ExceptionHandled = true;
    }
}

public class InternalServerErrorObjectResult : ObjectResult
{
    public InternalServerErrorObjectResult(object error)
        : base(error)
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }
}