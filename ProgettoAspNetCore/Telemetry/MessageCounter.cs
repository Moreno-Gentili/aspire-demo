using System.Diagnostics.Metrics;

namespace ProgettoAspNetCore.Telemetry;

public class MessageCounter : IMessageCounter
{
    private readonly Meter meter;
    private readonly Counter<int> counter;
    public MessageCounter()
    {
        meter = new(Name);
        counter = meter.CreateCounter<int>(Name);
    }

    public void Increment()
    {
        counter.Add(1);
    }

    internal static string Name { get; } = "MessageCount";
}

public interface IMessageCounter
{
    void Increment();
}