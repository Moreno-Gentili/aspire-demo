using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using WebApplication1.Telemetry;

namespace ProgettoAspNet
{
    public class Global : HttpApplication
    {
        private const string ApplicationName = "ProgettoAspNet";
        private ServiceProvider serviceProvider;
        private MeterProvider meterProvider;
        private LoggerProvider loggerProvider;
        private TracerProvider tracerProvider;
        public static ILogger Logger { get; private set; }

        protected void Application_Start()
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
                        .AddHttpClientInstrumentation()
                        .AddRabbitMQInstrumentation()
                        .AddSource(TraceActivitySource.Name);
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetInstrumentation()
                        .AddAspNetInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter(MessageCounter.Name);
                })
                .WithLogging(logging =>
                {
                })
            .UseOtlpExporter();

            serviceProvider = services.BuildServiceProvider();

            meterProvider = serviceProvider.GetService<MeterProvider>();
            tracerProvider = serviceProvider.GetService<TracerProvider>();
            loggerProvider = serviceProvider.GetService<LoggerProvider>();
            Logger = serviceProvider.GetService<ILogger<Global>>();

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            meterProvider?.Dispose();
            loggerProvider?.Dispose();
            tracerProvider?.Dispose();
            serviceProvider?.Dispose();
        }
    }
}