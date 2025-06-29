namespace Gateway.Middleware
{
    public class TokenCheckerMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            string requestPath = context.Request.Path.Value!;
            if (requestPath.Contains("users/login", StringComparison.InvariantCultureIgnoreCase) || requestPath.Contains("users/register", StringComparison.InvariantCultureIgnoreCase)
                || requestPath.Equals("/"))
            {
                await next(context);
            }
            else
            {
                var authHeader = context.Request.Headers.Authorization;
                if (authHeader.FirstOrDefault() == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Access Denied!");


                }
                else
                {
                    await next(context);
                }
            }
        }
    }
}
