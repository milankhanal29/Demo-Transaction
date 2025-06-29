using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace SharedLibrary
{
    public class RestrictAccessMiddleware(RequestDelegate requestDelegate)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var referrer = context.Request.Headers["Referrer"].FirstOrDefault();
            if (string.IsNullOrEmpty(referrer))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Cant reach this page");
                return;

            }
            else
                await requestDelegate(context);
        }

    }
}
