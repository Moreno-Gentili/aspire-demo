using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Diagnostics;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace ProgettoAspNet
{
    public class Global : HttpApplication
    {
        public static TracerProvider Tracer { get; private set; }
        public static MeterProvider Meter { get; private set; }
        public static ILoggerFactory Logger { get; private set; }

        public static void CheckDebugger()
        {
            // This env variable is passed from Aspire as we don't have a clean way to attach the debugger to IIS Express
            // Use this if you don't enable the vsjitdebugger.exe hook
            if (System.Environment.GetEnvironmentVariable("Launch_Debugger_On_Start") == "true")
            {
                Debugger.Launch();
            }
        }

        protected void Application_Start()
        {
            var resourceBuilder = ResourceBuilder.CreateDefault().AddService("progetto-aspnet");

            Tracer = Sdk.CreateTracerProviderBuilder()
                .AddAspNetInstrumentation()
                .AddHttpClientInstrumentation() // Traces outbound calls
                .AddOtlpExporter(opt => opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                .Build();

            Meter = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetInstrumentation() // Captures HTTP request metrics
            .AddRuntimeInstrumentation() // Captures CPU, Memory, GC
            .AddOtlpExporter(opt => opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
            .Build();

            Logger = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.AddOtlpExporter(opt => opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf);
                    options.IncludeFormattedMessage = true;
                });
            });

            // TelemetryConfig.RegisterTelemetry(serviceProvider);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            Tracer.Dispose();
            Meter.Dispose();
            Logger.Dispose();
        }
    }
}