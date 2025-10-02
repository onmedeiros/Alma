using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Alma.Core.Contexts;
using Alma.Core.Services;
using Alma.Core.Validations;
using System.Reflection;

namespace SimpleCore
{
    public static class SimpleCoreExtensions
    {
        public static WebApplicationBuilder AddSimpleCore(this WebApplicationBuilder builder)
        {
            // Add memory cache
            builder.Services.AddMemoryCache();

            // Configure Serilog
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration.ReadFrom.Configuration(builder.Configuration)
                .WriteTo.Console(outputTemplate: Logging.LogOutputTemplate)
                .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces)
                .Enrich.FromLogContext()
                .Enrich.WithCorrelationId(headerName: "x-correlation-id");
            });

            // Add Core Services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserContext, UserContext>();
            builder.Services.AddSingleton<IValidationService, ValidationService>();

            return builder;
        }

        public static IServiceCollection AddSimpleCosmos<TContext>(this IServiceCollection services, string connectionString, string database)
            where TContext : DbContext
        {
            // Add database
            services.AddDbContext<TContext>(options =>
            {
                options.UseCosmos(connectionString, database);
                options.EnableDetailedErrors();
            });

            return services;
        }

        #region Use

        public static WebApplication UseSimpleCosmos<TContext>(this WebApplication app)
            where TContext : DbContext
        {
            app.Services.GetRequiredService<TContext>().Database.EnsureCreated();

            return app;
        }

        #endregion
    }
}