using MechanicShop.Infrastructure.Data;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddPresentation(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

var isDevelopment = app.Environment.IsDevelopment();
var enableDocs = isDevelopment || app.Configuration.GetValue<bool>("EnableApiDocs");
var enableDataSeeding = isDevelopment || app.Configuration.GetValue<bool>("EnableDataSeeding");

if (enableDocs)
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MechanicShop API V1");

        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });

    app.MapScalarApiReference();

}

await app.InitialiseDatabaseAsync(shouldSeed: enableDataSeeding);

if (!isDevelopment)
{
    app.UseHsts();
}

app.UseCoreMiddlewares(builder.Configuration);

app.MapControllers();

app.Run();
