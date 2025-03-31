using Microsoft.ApplicationInsights.DataContracts;
using Serilog;
using System.Text;

namespace LSC.SmartCertify.API.Middlewares
{
    public class RequestBodyLoggingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var method = context.Request.Method;
            // Ensure the request body can be read multiple times
            context.Request.EnableBuffering();
            // Only if we are dealing with POST or PUT, GET and others shouldn't have a body
            if (context.Request.Body.CanRead && (method == HttpMethods.Post || method == HttpMethods.Put))
            {
                // Leave stream open so next middleware can read it
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 512, leaveOpen: true);
                var requestBody = await reader.ReadToEndAsync();
                // Reset stream position, so next middleware can read it
                context.Request.Body.Position = 0;
                // Write request body to App Insights
                var requestTelemetry = context.Features.Get<RequestTelemetry>();
                requestTelemetry?.Properties.Add("RequestBody", requestBody);
                Log.Information("Request:" + requestBody);
            }
            // Call next middleware in the pipeline
            await next(context);
        }
    }
}
