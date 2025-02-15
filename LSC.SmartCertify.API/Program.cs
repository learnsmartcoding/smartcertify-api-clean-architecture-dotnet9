
using FluentValidation;
using LSC.SmartCertify.API.Filters;
using LSC.SmartCertify.Application;
using LSC.SmartCertify.Application.DTOValidations;
using LSC.SmartCertify.Application.Interfaces.Courses;
using LSC.SmartCertify.Application.Interfaces.QuestionsChoice;
using LSC.SmartCertify.Application.Services;
using LSC.SmartCertify.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

namespace LSC.SmartCertify.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


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
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // Disable automatic validation
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });


            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddOpenApi();


            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IChoiceService, ChoiceService>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IChoiceRepository, ChoiceRepository>();

            // Add FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<CreateCourseValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<UpdateCourseValidator>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));


            // In production, modify this with the actual domains you want to allow
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                    //.AllowCredentials();  // Required for SignalR
                });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseCors("default");



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

            //app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
    }
}
