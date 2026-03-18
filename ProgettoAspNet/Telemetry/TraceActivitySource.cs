using System.Diagnostics;

namespace WebApplication1.Telemetry
{
    public static class TraceActivitySource
    {
        internal static string Name { get; } = "ProgettoAspNet";
        internal static ActivitySource Value { get; } = new ActivitySource(Name);
    }
}