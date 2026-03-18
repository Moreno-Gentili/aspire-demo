using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using ProgettoAspNetCore;
using ProgettoAspNetCore.Telemetry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddHostedService<MessageReceiver>();
builder.Services.AddSingleton<IMessageCounter, MessageCounter>();

builder.Services.AddOpenTelemetry()
                    .ConfigureResource(r => r.AddService(TraceActivitySource.Name))
                    .WithTracing(tracing => tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRabbitMQInstrumentation()
                        .AddSource(TraceActivitySource.Name)
                        .AddOtlpExporter())
                    .WithMetrics(metrics => metrics
                        // Instrumentations
                        .AddMeter(MessageCounter.Name)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        // Exporter
                        .AddPrometheusExporter()
                        .AddOtlpExporter())
                    .WithLogging(logging => logging
                        .AddOtlpExporter());

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapStaticAssets();
app.MapHub<MessageHub>("/messages");

app.Run();
