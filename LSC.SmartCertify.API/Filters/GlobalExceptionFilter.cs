using Azure.Core;
using LSC.SmartCertify.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Net;


namespace TodoApp.WebAPI.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IWebHostEnvironment env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            this.env = env;
        }

        //public void OnException(ExceptionContext context)
        //{
        //    _logger.LogError(context.Exception, "An unhandled exception occurred");
        //    _logger.LogError(new EventId(context.Exception.HResult),
        //        context.Exception,
        //        context.Exception.Message);

        //    // Handle specific exceptions
        //    if (context.Exception is NotFoundException or KeyNotFoundException)
        //    {
        //        context.Result = new ObjectResult(new { message = context.Exception.Message })
        //        {
        //            StatusCode = StatusCodes.Status404NotFound
        //        };
        //    }
        //    else if (context.Exception is FluentValidation.ValidationException validationException)
        //    {
        //        var errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
        //        context.Result = new BadRequestObjectResult(errors);
        //        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    }
        //    else
        //    {
        //        // This is often very handy information for tracing the specific request
        //        var traceId = Activity.Current?.Id ?? context.HttpContext?.TraceIdentifier;

        //        var json = new JsonErrorResponse
        //        {
        //            Messages = new[] { "An error ocurred." },
        //            TraceId = traceId ?? string.Empty
        //        };

        //        if (env.IsDevelopment())
        //        {
        //            json.DeveloperMessage = context.Exception;
        //        }

        //        // General exception handling
        //        context.Result = new ObjectResult(new { message = "An error occurred while processing your request" })
        //        {
        //            StatusCode = StatusCodes.Status500InternalServerError
        //        };

        //    }

        //    // Mark the exception as handled
        //    context.ExceptionHandled = true;
        //}

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred");
            _logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);

            // Capture the Trace ID (Request ID) for debugging
            var traceId = Activity.Current?.Id ?? context.HttpContext?.TraceIdentifier;
            var requestId = context.HttpContext?.TraceIdentifier;

            // Handle specific exceptions
            if (context.Exception is NotFoundException or KeyNotFoundException)
            {
                context.Result = new ObjectResult(new { message = context.Exception.Message, traceId })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            else if (context.Exception is FluentValidation.ValidationException validationException)
            {
                var errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                context.Result = new BadRequestObjectResult(new { errors, traceId });
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { "An error occurred." },
                    TraceId = traceId ?? string.Empty
                };

                if (env.IsDevelopment())
                {
                    json.DeveloperMessage = context.Exception;
                }

                // General 500 error response with Request ID
                context.Result = new ObjectResult(new
                {
                    message = "An error occurred while processing your request. Please contact support with the Request ID.",
                    traceId,
                    requestId
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            // Mark the exception as handled
            context.ExceptionHandled = true;
        }

        private class JsonErrorResponse
        {
            public string[] Messages { get; set; }
            public string TraceId { get; set; }
            public object DeveloperMessage { get; set; }
        }
    }


}
