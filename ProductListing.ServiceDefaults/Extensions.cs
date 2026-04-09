using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;

using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>Adds common Aspire services: service discovery, resilience, health checks &amp; OpenTelemetry.</summary>
/// <remarks>
/// This project should be referenced by each service project in your solution.<br />
/// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
/// </remarks>
public static class Extensions
{
  private const string AlivenessEndpointPath = "/alive";
  private const string HealthEndpointPath = "/health";
  private const string LivenessCheck = "live";

  private static readonly string[] TracingFilters = [AlivenessEndpointPath, HealthEndpointPath];

  /// <summary>Adds default health checks for, e.g., liveness.</summary>
  /// <param name="builder">The <typeparamref name="TBuilder" /> builder to use.</param>
  /// <param name="isGrpc">Whether to add gRPC health checks, if <see langword="true" />, too, or not.</param>
  /// <typeparam name="TBuilder">The <see cref="IHostApplicationBuilder" /> derived builder type.</typeparam>
  /// <returns>The supplied <paramref name="builder" /> instance with default health checks added.</returns>
  public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder, bool isGrpc = false)
    where TBuilder : IHostApplicationBuilder
  {
    builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy(), [LivenessCheck]);
    if (isGrpc)
    {
      builder.Services.AddGrpcHealthChecks().AddCheck("grpc", () => HealthCheckResult.Healthy(), [LivenessCheck]);
    }

    return builder;
  }

  /// <summary>Adds service defaults, e.g., default health checks, resilience, service discovery &amp; configures OpenTelemetry.</summary>
  /// <param name="builder"><inheritdoc cref="AddDefaultHealthChecks{TBuilder}(TBuilder, bool)" path="/param[@name='builder']" /></param>
  /// <param name="isGrpc">Whether to add gRPC instrumentation, if <see langword="true" />, or not.</param>
  /// <returns>The supplied <paramref name="builder" /> instance with service defaults added.</returns>
  public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder, bool isGrpc = false)
    where TBuilder : IHostApplicationBuilder
  {
    builder.ConfigureOpenTelemetry()
           .AddDefaultHealthChecks(isGrpc).Services
           .AddServiceDiscovery()
           .ConfigureHttpClientDefaults(http =>
    {
      http.AddStandardResilienceHandler();
      http.AddServiceDiscovery();
    });
    builder.Services.Configure<ServiceDiscoveryOptions>(options => options.AllowedSchemes = [Uri.UriSchemeHttps]);
    if (isGrpc)
    {
      builder.Services.AddGrpc(options => options.EnableDetailedErrors = builder.Environment.IsDevelopment());
    }

    return builder;
  }

  /// <summary>Configures OpenTelemetry defaults.</summary>
  /// <param name="builder"><inheritdoc cref="AddDefaultHealthChecks{TBuilder}(TBuilder, bool)" path="/param[@name='builder']" /></param>
  /// <param name="isGrpc"><inheritdoc cref="AddServiceDefaults{TBuilder}(TBuilder, bool)" path="/param[@name='isGrpc']" /></param>
  /// <typeparam name="TBuilder"><inheritdoc cref="AddDefaultHealthChecks{TBuilder}(TBuilder, bool)" path="/typeparam" /></typeparam>
  /// <returns>The supplied <paramref name="builder" /> instance with OpenTelemetry configured.</returns>
  public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder, bool isGrpc = false)
    where TBuilder : IHostApplicationBuilder
  {
    builder.Logging.AddOpenTelemetry(logging => logging.IncludeFormattedMessage = logging.IncludeScopes = true).Services
                   .AddOpenTelemetry()
                   .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation()
                                                  .AddHttpClientInstrumentation()
                                                  .AddRuntimeInstrumentation())
                   .WithTracing(tracing =>
                   {
                     var tracingBuilder = tracing.AddSource(builder.Environment.ApplicationName)
                                                 .AddAspNetCoreInstrumentation(tracing =>
                                                   tracing.Filter =
                                                     context => TracingFilters.All(filter =>
                                                       !context.Request.Path.StartsWithSegments(filter)))
                                                 .AddHttpClientInstrumentation();
                     if (isGrpc)
                     {
                       tracingBuilder.AddGrpcClientInstrumentation();
                     }
                   });

    return builder.AddOpenTelemetryExporters();
  }

  /// <summary>Maps default endpoints for the application, e.g., health check endpoints.</summary>
  /// <param name="app">The <see cref="WebApplication" /> to configure.</param>
  /// <param name="isGrpc">Whether to map gRPC health checks, if <see langword="true" />, too, or not.</param>
  /// <returns>The supplied <paramref name="app" /> instance with default endpoints added.</returns>
  /// <remarks>
  /// Health check endpoints are only mapped for <b>development</b> environments as enabling them for other environments causes
  /// security implications. For further details, see https://aka.ms/dotnet/aspire/healthchecks.
  /// </remarks>
  public static WebApplication MapDefaultEndpoints(this WebApplication app, bool isGrpc = false)
  {
    if (app.Environment.IsDevelopment())
    {
      app.MapHealthChecks(HealthEndpointPath);
      app.MapHealthChecks(AlivenessEndpointPath, new() { Predicate = registration => registration.Tags.Contains(LivenessCheck) });
      if (isGrpc)
      {
        app.MapGrpcHealthChecksService();
        app.MapGet(
          "/",
          () =>
            TypedResults.Text(
              @"<!DOCTYPE html>
              <html>
                <head>
                  <title>Non-gRPC request denied</title>
                </head>
                <body>
                  This endpoint serves gRPC clients only. To learn how to create one, see
                  <a href=""https://learn.microsoft.com/en-us/aspnet/core/tutorials/grpc/grpc-start"">https://learn.microsoft.com/en-us/aspnet/core/tutorials/grpc/grpc-start</a>.
                </body>
              </html>",
              System.Net.Mime.MediaTypeNames.Text.Html,
              System.Text.Encoding.UTF8
            ));
      }
    }

    return app;
  }

  private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
  {
    if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
    {
      builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    // Uncomment the following block to enable Azure Monitor exporter (requires Azure.Monitor.OpenTelemetry.AspNetCore)
    //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    //{
    //  builder.Services.AddOpenTelemetry().UseAzureMonitor();
    //}

    return builder;
  }
}
