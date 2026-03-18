using System.Diagnostics.Metrics;

namespace WebApplication1.Telemetry
{
    public static class MessageCounter
    {
        private static readonly Meter meter;
        private static readonly Counter<int> counter;
        static MessageCounter()
        {
            meter = new Meter(Name);
            counter = meter.CreateCounter<int>(Name);
        }

        public static void Increment()
        {
            counter.Add(1);
        }

        internal static string Name { get; } = "MessageCount";
    }
}