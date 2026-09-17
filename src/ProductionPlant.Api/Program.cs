using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics;
using ProductionPlant.Application;
using ProductionPlant.Domain;
using ProductionPlant.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Logging
// --------------------------------------------------

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

#region Azure Identity

var managedIdentityClientId =
    Environment.GetEnvironmentVariable(
        "MANAGED_IDENTITY_CLIENT_ID");

var credentialOptions =
    string.IsNullOrWhiteSpace(managedIdentityClientId)
        ? new DefaultAzureCredentialOptions()
        : new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId =
                managedIdentityClientId
        };

var credential =
    new DefaultAzureCredential(credentialOptions);

#endregion

#region Azure App Configuration

var appConfigurationEndpoint =
    builder.Configuration
        .GetConnectionString(
            "AppConfigurationEndpoint");

if (!string.IsNullOrWhiteSpace(appConfigurationEndpoint))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(
            new Uri(appConfigurationEndpoint),
            credential)
         .ConfigureKeyVault(kv =>
          {
              kv.SetCredential(credential);
          });
    });
}

#endregion

// --------------------------------------------------
// Services
// --------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<IConditionOperator, ConditionOperator>();
builder.Services.AddScoped<IRuleEvaluator, RuleEvaluator>();
builder.Services.AddScoped<EvaluationService>();


builder.Services.AddMemoryCache();

// --------------------------------------------------
// CORS
// --------------------------------------------------

var allowedOrigins =
    builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// --------------------------------------------------
// Build
// --------------------------------------------------

var app = builder.Build();

// --------------------------------------------------
// Exception Handling
// --------------------------------------------------

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception =
            context.Features
                .Get<IExceptionHandlerFeature>()?
                .Error;

        context.Response.StatusCode = 500;
        context.Response.ContentType =
            "application/problem+json";

        await context.Response.WriteAsJsonAsync(new
        {
            title = "Server error",
            detail = app.Environment.IsDevelopment()
                ? exception?.Message
                : "An unexpected error occurred."
        });
    });
});

// --------------------------------------------------
// Swagger
// --------------------------------------------------

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Production Plant API V1");

    options.RoutePrefix = "swagger";
});

// --------------------------------------------------
// HTTP Pipeline
// --------------------------------------------------

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.MapControllers();

// --------------------------------------------------
// Health Check
// --------------------------------------------------

app.MapGet(
    "/health",
    () => Results.Ok(new
    {
        status = "healthy"
    }));

app.Run();

public partial class Program
{
}