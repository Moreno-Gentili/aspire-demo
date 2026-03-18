using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ProgettoAspNet
{
    public static class TelemetryConfig
    {
        public static void RegisterTelemetry(ServiceProvider serviceProvider)
        {
            var services = new ServiceCollection();

            services.AddLogging(logging =>
            {
                logging.Configure(options =>
                {
                    options.ActivityTrackingOptions =
                        ActivityTrackingOptions.SpanId |
                        ActivityTrackingOptions.TraceId |
                        ActivityTrackingOptions.ParentId;
                });
                logging.AddOpenTelemetry(ot =>
                {
                    ot.IncludeFormattedMessage = true;
                    ot.IncludeScopes = true;
                    ot.ParseStateValues = true;
                });
            });

            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetInstrumentation()
                        .AddHttpClientInstrumentation();
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetInstrumentation()
                        .AddAspNetInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithLogging(logging =>
                {
                })
            .UseOtlpExporter();

            serviceProvider = services.BuildServiceProvider();

            var meterProvider = serviceProvider.GetService<MeterProvider>();
            var tracerProvider = serviceProvider.GetService<TracerProvider>();
            var loggerProvider = serviceProvider.GetService<LoggerProvider>();
        }
    }
}
