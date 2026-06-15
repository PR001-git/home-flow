using System.Text.Json;
using HomeFlow.API.Models;
using HomeFlow.Application.Exceptions;

namespace HomeFlow.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteErrorAsync(context, 400, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, 404, ex.Message);
        }
        catch (Exception)
        {
            await WriteErrorAsync(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new ErrorResponse(message), JsonOptions));
    }
}
