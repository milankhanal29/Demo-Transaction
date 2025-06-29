namespace Gateway.Middleware
{
    public class InterceptionMiddleware(RequestDelegate requestDeligate)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers["Referrer"] = "Api-Gateway";
            await requestDeligate(context);
        }
    }
}
