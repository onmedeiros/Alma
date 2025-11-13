using Alma.Blazor;
using Alma.Blazor.Components;
using Alma.Blazor.Components.Account;
using Alma.Blazor.Entities;
using Alma.Blazor.HostedServices;
using Alma.Blazor.Services;
using Alma.Core;
using Alma.Core.Exceptions;
using Alma.Core.Mongo;
using Alma.Workflows;
using Alma.Workflows.Alerts;
using Alma.Workflows.Apis.AspNetCore;
using Alma.Workflows.Monitoring.Mongo;
using Alma.Modules.Integrations.Controllers.Filters;
using Alma.Organizations;
using Alma.Organizations.Middlewares;
using ApexCharts;
using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hellang.Middleware.ProblemDetails;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MudBlazor.Services;
using Serilog;
using System.Text.Json.Serialization;
using Alma.Workflows.Databases;

// Logging
Log.Logger = Logging.ConfigureLogger();
Log.Logger.Information("Starting application.");

var builder = WebApplication.CreateBuilder(args);

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    builder.WebHost.UseStaticWebAssets();
}

#region Logging

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console(outputTemplate: Logging.LogOutputTemplate)
    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces)
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId(headerName: "x-correlation-id");
});

#endregion

builder.Services.AddApplicationInsightsTelemetry();

// Serviços para Componentes UI
builder.Services.AddMudServices();
builder.Services.AddApexCharts();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddSingleton<IEmailSender<AlmaUser>, IdentityNoOpEmailSender>();

builder.Services.AddProblemDetails((Hellang.Middleware.ProblemDetails.ProblemDetailsOptions options) =>
{
    // This will map NotImplementedException to the 501 Not Implemented status code.
    options.MapToStatusCode<NotImplementedException>(StatusCodes.Status501NotImplemented);

    // Veripag custom exception
    options.MapToStatusCode<ValidationException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<InvalidOperationException>(StatusCodes.Status400BadRequest);
    options.MapToStatusCode<NotFoundException>(StatusCodes.Status404NotFound);

    // Because exceptions are handled polymorphically, this will act as a "catch all" mapping, which is why it's added last.
    // If an exception other than NotImplementedException and HttpRequestException is thrown, this will handle it.
    options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
});

builder.Services.AddHostedService<ApplicationRegister>();
builder.Services.AddAlmaWorkflows(options => { });

#region Database

var connectionString = builder.Configuration.GetValue<string>("MongoDatabase:ConnectionString");
var databaseName = builder.Configuration.GetValue<string>("MongoDatabase:DatabaseName");

ArgumentException.ThrowIfNullOrEmpty(connectionString);
ArgumentException.ThrowIfNullOrEmpty(databaseName);

builder.Services.AddAlmaMongo(connectionString, databaseName);

#endregion

#region Identity

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IIdentityManager, IdentityManager>();
builder.Services.AddUserContext();

builder.Services.AddIdentityCore<AlmaUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
    .AddMongoDbStores<AlmaUser, MongoRole, ObjectId>(mongo => mongo.ConnectionString = connectionString.Replace("/?", $"/{databaseName}?"))
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseMongoDb(options =>
        {
            options.ReplaceDefaultApplicationEntity<AlmaApplication>();
        });
    })
    .AddServer(options =>
    {
        // Enable the token endpoint.
        options.SetTokenEndpointUris("connect/token");

        // Enable the client credentials flow.
        options.AllowClientCredentialsFlow();

        // Register the signing and encryption credentials.
        // options.AddEncryptionKey(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)));
        // options.AddSigningKey(new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32)));
        options.AddEphemeralSigningKey();
        options.AddEphemeralEncryptionKey();

        options.DisableAccessTokenEncryption();

        // Register the ASP.NET Core host and configure the ASP.NET Core options.
        options.UseAspNetCore()
                .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

#endregion

#region Hangfire

builder.Services.AddHangfire(options =>
{
    options.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
    options.UseSimpleAssemblyNameTypeSerializer();
    options.UseRecommendedSerializerSettings();
    options.UseMongoStorage(connectionString, databaseName, new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        },
        Prefix = "hangfire",
        CheckConnection = true,
        CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.Poll
    });
});

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "Alma.Blazor";
    options.Queues = new[] { "default", "import-transactions", "monitoring-schedule" };
});

#endregion

builder.Services.AddAlmaOrganizations();
builder.Services.AddWorkflowDatabases();

// New model services
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHybridCache();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Registrar o filtro para DI
builder.Services.AddScoped<ApiHostRestrictionFilter>();

// Registro de m�dulos
builder.Services.AddModules(options =>
{
    // options.Register(typeof(Alma.Modules.Base.Module).Assembly);
    // options.Register(typeof(Alma.Modules.Workflows.Module).Assembly);
})
    .Register<Alma.Modules.Core.Module>()
    .Register<Alma.Modules.Integrations.Module>()
    .Register<Alma.Modules.Organizations.Module>()
    .Register<Alma.Modules.Workflows.Module>()
    .Register<Alma.Modules.Alerts.Module>()
    .Register<Alma.Modules.Monitoring.Module>()
    .Register<Alma.Modules.Dashboards.Module>();

// Registro de bibliotecas
builder.Services.AddAlmaWorkflowsApis();
builder.Services.AddAlmaWorkflowsMonitoringMongo();

var app = builder.Build();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()//.AddAdditionalAssemblies(typeof(Alma.Modules.Base.Module).Assembly);
    .AddModules(app);

app.UseHangfireDashboard("/hangfire");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.UseAntiforgery();

app.UseOrganizationMiddleware();

app.Run();