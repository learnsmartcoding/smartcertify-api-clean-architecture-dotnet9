using Microsoft.ApplicationInsights.DataContracts;

namespace LSC.SmartCertify.API.Middlewares
{
    public class ResponseBodyLoggingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var originalBodyStream = context.Response.Body;
            try
            {
                // Swap out stream with one that is buffered and suports seeking
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;
                // hand over to the next middleware and wait for the call to return
                
                await next(context);
                // Read response body from memory stream
                memoryStream.Position = 0;
                var reader = new StreamReader(memoryStream);
                var responseBody = await reader.ReadToEndAsync();
                // Copy body back to so its available to the user agent
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
                // Write response body to App Insights
                var requestTelemetry = context.Features.Get<RequestTelemetry>();
                requestTelemetry?.Properties.Add("ResponseBody", responseBody);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
