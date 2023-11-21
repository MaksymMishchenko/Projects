using CarBlogApp.Dto;
using Microsoft.AspNetCore.Http;
using System.Buffers;
using System.Net;
using System.Text.Json;

namespace CarBlogApp.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestDelegate> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<RequestDelegate> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(httpContext, ex.Message);
            }
        }

        private void HandleExceptionAsync(HttpContext context,
            string exMsg
            )
        {
            _logger.LogError(exMsg);              
            context.Response.Redirect("/Home/Error");
        }
    }
}

