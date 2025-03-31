
using FluentValidation;
using LSC.SmartCertify.API.Filters;
using LSC.SmartCertify.API.Middlewares;
using LSC.SmartCertify.Application;
using LSC.SmartCertify.Application.DTOValidations;
using LSC.SmartCertify.Application.Interfaces.Certification;
using LSC.SmartCertify.Application.Interfaces.Common;
using LSC.SmartCertify.Application.Interfaces.Courses;
using LSC.SmartCertify.Application.Interfaces.Graph;
using LSC.SmartCertify.Application.Interfaces.ManageUser;
using LSC.SmartCertify.Application.Interfaces.QuestionsChoice;
using LSC.SmartCertify.Application.Interfaces.Storage;
using LSC.SmartCertify.Application.Services;
using LSC.SmartCertify.Application.Services.Certification;
using LSC.SmartCertify.Application.Services.Common;
using LSC.SmartCertify.Application.Services.Graph;
using LSC.SmartCertify.Application.Services.ManageUser;
using LSC.SmartCertify.Infrastructure;
using LSC.SmartCertify.Infrastructure.BackgroundServices;
using LSC.SmartCertify.Infrastructure.Services.Storage;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Scalar.AspNetCore;
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

                //builder.Services.AddHttpClient<YouTubeService>();
                //builder.Services.Configure<YouTubeOptions>(builder.Configuration.GetSection("YouTube"));


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
                builder.Services.AddTransient<RequestBodyLoggingMiddleware>();
                builder.Services.AddTransient<ResponseBodyLoggingMiddleware>();                          

                // Add FluentValidation
                builder.Services.AddValidatorsFromAssemblyContaining<CreateCourseValidator>();
                builder.Services.AddValidatorsFromAssemblyContaining<UpdateCourseValidator>();
                builder.Services.AddAutoMapper(typeof(MappingProfile));

                #region AD B2C configuration
                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddMicrosoftIdentityWebApi(options =>
                  {
                      builder.Configuration.Bind("AzureAdB2C", options);

                      options.Events = new JwtBearerEvents
                      {                        

                          OnTokenValidated = context =>
                          {
                              var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                              // Access the scope claim (scp) directly
                              var scopeClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == "scp")?.Value;

                              if (scopeClaim != null)
                              {
                                  logger.LogInformation("Scope found in token: {Scope}", scopeClaim);
                              }
                              else
                              {
                                  logger.LogWarning("Scope claim not found in token.");
                              }


                              return Task.CompletedTask;
                          },
                          OnAuthenticationFailed = context =>
                          {
                              var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                              logger.LogError("Authentication failed: {Message}", context.Exception.Message);
                              return Task.CompletedTask;
                          },
                          OnChallenge = context =>
                          {
                              var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                              logger.LogError("Challenge error: {ErrorDescription}", context.ErrorDescription);
                              return Task.CompletedTask;
                          }
                      };
                  }, options => { builder.Configuration.Bind("AzureAdB2C", options); });

                // The following flag can be used to get more descriptive errors in development environments
                IdentityModelEventSource.ShowPII = true;
                #endregion  AD B2C configuration

                builder.Services.AddHttpClient();

                builder.Services.AddScoped<IStorageService, StorageService>();
                builder.Services.AddSingleton<GraphAuthService>();
                builder.Services.AddScoped<IGraphAuthService, GraphAuthService>();
                builder.Services.AddScoped<IGraphService, GraphService>();

                // Register the background service
                builder.Services.AddHostedService<NotificationBackgroundService>();
                builder.Services.AddHostedService<OnboardUserBackgroundService>();

                // In production, modify this with the actual domains you want to allow
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("default", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
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
                    app.MapOpenApi();
                    app.MapScalarApiReference(options =>
                    {
                        options.WithTitle("My API");
                        options.WithTheme(ScalarTheme.BluePlanet);
                        options.WithSidebar(true);
                    });

                    app.UseSwaggerUi(options =>
                    {
                        options.DocumentPath = "openapi/v1.json";
                    });

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
