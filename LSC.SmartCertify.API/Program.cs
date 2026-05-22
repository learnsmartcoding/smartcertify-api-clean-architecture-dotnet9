
using FluentValidation;
using LSC.SmartCertify.API.Controllers;
using LSC.SmartCertify.API.Filters;
using LSC.SmartCertify.API.Middlewares;
using LSC.SmartCertify.Application;
using LSC.SmartCertify.Application.DTOValidations;
using LSC.SmartCertify.Application.Interfaces.Certification;
using LSC.SmartCertify.Application.Interfaces.Chat;
using LSC.SmartCertify.Application.Interfaces.Common;
using LSC.SmartCertify.Application.Interfaces.Courses;
using LSC.SmartCertify.Application.Interfaces.EmailNotification;
using LSC.SmartCertify.Application.Interfaces.Graph;
using LSC.SmartCertify.Application.Interfaces.ManageUser;
using LSC.SmartCertify.Application.Interfaces.QuestionsChoice;
using LSC.SmartCertify.Application.Interfaces.Storage;
using LSC.SmartCertify.Application.Services;
using LSC.SmartCertify.Application.Services.Certification;
using LSC.SmartCertify.Application.Services.Common;
using LSC.SmartCertify.Application.Services.EmailNotification;
using LSC.SmartCertify.Application.Services.Graph;
using LSC.SmartCertify.Application.Services.ManageUser;
using LSC.SmartCertify.Infrastructure;
using LSC.SmartCertify.Infrastructure.BackgroundServices;
using LSC.SmartCertify.Infrastructure.Security;
using LSC.SmartCertify.Infrastructure.Services.Storage;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Templates;
using System.Net;
using System.Text.Json.Serialization;
using TodoApp.WebAPI.Filters;

namespace LSC.SmartCertify.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog with the settings
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .CreateBootstrapLogger();

            try
            {

                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddApplicationInsightsTelemetry();

                builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .WriteTo.Console(new ExpressionTemplate(
                    // Include trace and span ids when present.
                    "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}"))
                .WriteTo.ApplicationInsights(
                  services.GetRequiredService<TelemetryConfiguration>(),
                  TelemetryConverter.Traces));

                Log.Information("Starting the SmartCertify API...");



                // Add services to the container.

                //use this for real database on your sql server
                builder.Services.AddDbContext<SmartCertifyContext>(options =>
                {
                    options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DbContext"),                    
                    providerOptions => providerOptions.EnableRetryOnFailure()
                    ).EnableSensitiveDataLogging().EnableDetailedErrors();
                }
                  );

                builder.Services.AddControllers(options =>
                {
                    options.Filters.Add<ValidationFilter>(); // Add your custom validation filter
                    options.Filters.Add<GlobalExceptionFilter>();
                }).ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true; // Disable automatic validation
                })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

                builder.Services.AddHttpClient<YouTubeService>();
                builder.Services.Configure<YouTubeOptions>(builder.Configuration.GetSection("YouTube"));


                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddOpenApi();


                builder.Services.AddScoped<ICourseRepository, CourseRepository>();
                builder.Services.AddScoped<ICourseService, CourseService>();
                builder.Services.AddScoped<IQuestionService, QuestionService>();
                builder.Services.AddScoped<IChoiceService, ChoiceService>();
                builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
                builder.Services.AddScoped<IChoiceRepository, ChoiceRepository>();
                builder.Services.AddScoped<IExamRepository, ExamRepository>();
                builder.Services.AddScoped<IExamService, ExamService>();
                builder.Services.AddScoped<IUserProfileService, UserProfileService>();
                builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
                builder.Services.AddScoped<IUserClaims, UserClaims>();
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                builder.Services.AddTransient<RequestBodyLoggingMiddleware>();
                builder.Services.AddTransient<ResponseBodyLoggingMiddleware>();                          

                // Add FluentValidation
                builder.Services.AddValidatorsFromAssemblyContaining<CreateCourseValidator>();
                builder.Services.AddValidatorsFromAssemblyContaining<UpdateCourseValidator>();
                // Register AutoMapper profiles
                builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

                #region Entra ID configuration
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddMicrosoftIdentityWebApi(options =>
           {
               builder.Configuration.Bind("AzureAd", options);
               options.Events = new JwtBearerEvents();

               /// <summary>
               /// Below you can do extended token validation and check for additional claims, such as:
               ///
               /// - check if the caller's tenant is in the allowed tenants list via the 'tid' claim (for multi-tenant applications)
               /// - check if the caller's account is homed or guest via the 'acct' optional claim
               /// - check if the caller belongs to right roles or groups via the 'roles' or 'groups' claim, respectively
               ///
               /// Bear in mind that you can do any of the above checks within the individual routes and/or controllers as well.
               /// For more information, visit: https://docs.microsoft.com/azure/active-directory/develop/access-tokens#validate-the-user-has-permission-to-access-this-data
               /// </summary>

               //options.Events.OnTokenValidated = async context =>
               //{
               //    string[] allowedClientApps = { /* list of client ids to allow */ };

               //    string clientappId = context?.Principal?.Claims
               //        .FirstOrDefault(x => x.Type == "azp" || x.Type == "appid")?.Value;

               //    if (!allowedClientApps.Contains(clientappId))
               //    {
               //        throw new System.Exception("This client is not authorized");
               //    }
               //};
           }, options => { builder.Configuration.Bind("AzureAd", options); });

                #endregion  Entra ID configuration

                builder.Services.AddHttpClient();

                // ── AI Chat: Anthropic Claude + MCP client ─────────────────────────
                // AnthropicService orchestrates Claude API calls and MCP tool routing
                builder.Services.AddScoped<LSC.SmartCertify.API.Services.AnthropicService>();
                // Chat history persistence
                builder.Services.AddScoped<IChatHistoryRepository, LSC.SmartCertify.Infrastructure.ChatHistoryRepository>();
                builder.Services.AddScoped<IChatHistoryService, LSC.SmartCertify.Application.Services.ChatHistoryService>();
                // Named HttpClient for Anthropic API (base URL + timeout)
                builder.Services.AddHttpClient("Anthropic", c =>
                {
                    c.BaseAddress = new Uri("https://api.anthropic.com/");
                    c.Timeout = TimeSpan.FromSeconds(60);
                });

                // Named HttpClient for MCPServer REST bridge.
                var mcpClientBuilder = builder.Services.AddHttpClient("McpServer", c =>
                {
                    c.Timeout = TimeSpan.FromSeconds(30);
                });

                if (builder.Environment.IsDevelopment())
                {
                    // Trust the ASP.NET Core dev certificate on localhost only.
                    // In production the MCPServer runs on Azure with a valid cert — no bypass needed.
                    mcpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });
                }

                builder.Services.AddSingleton<GraphAuthService>();
                builder.Services.AddScoped<IGraphAuthService, GraphAuthService>();
                builder.Services.AddScoped<IGraphService, GraphService>();
                builder.Services.AddScoped<IEmailNotification, EmailNotification>();

                builder.Services.AddScoped<IStorageService, StorageService>();

                // Register the background service
                builder.Services.AddHostedService<NotificationBackgroundService>();
                builder.Services.AddHostedService<OnboardUserBackgroundService>();

                // In production, modify this with the actual domains you want to allow
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("default", policy =>
                    {
                        policy.AllowAnyOrigin()
                              //WithOrigins("http://localhost:4200", "https://smartlearnbykarthik.azurewebsites.net") // Corrected frontend URL without trailing slash
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                        //.AllowCredentials();  // Required for SignalR
                    });
                });


                var app = builder.Build();

                // Configure the HTTP request pipeline.
                app.UseCors("default");

                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        Log.Error(exception, "Unhandled exception occurred. {ExceptionDetails}", exception?.ToString());
                        Console.WriteLine(exception?.ToString());
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await context.Response.WriteAsync("An unexpected error occurred. Please try again later.");
                    });
                });


                app.UseMiddleware<RequestResponseLoggingMiddleware>();
                app.UseMiddleware<RequestBodyLoggingMiddleware>();
                app.UseMiddleware<ResponseBodyLoggingMiddleware>();

                // Configure the HTTP request pipeline.
                //if (app.Environment.IsDevelopment())
                {
                    app.UseOpenApi();          // Serves /swagger/v1/swagger.json
                    app.UseSwaggerUi();       // Serves Swagger UI at /swagger

                }

                app.UseHttpsRedirection();

                app.UseAuthorization();


                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
