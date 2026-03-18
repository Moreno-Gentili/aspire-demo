using System.Diagnostics;

namespace ProgettoAspNetCore.Telemetry;

internal static class TraceActivitySource
{
    internal static string Name { get; } = "ProgettoAspNetCore";
    internal static ActivitySource Value { get; } = new(Name);
}
