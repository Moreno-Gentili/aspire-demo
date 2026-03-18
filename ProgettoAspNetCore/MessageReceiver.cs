using Microsoft.AspNetCore.SignalR;
using ProgettoAspNetCore.Telemetry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text.Json;

namespace ProgettoAspNetCore;

public class MessageReceiver(
    IConfiguration configuration,
    ILogger<MessageReceiver> logger,
    IMessageCounter messageCounter,
    IHubContext<MessageHub> messages) : IHostedService
{
    private RabbitReceiver? receiver;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        receiver = await CreateRabbitReceiver(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (receiver is null)
        {
            return;
        }

        await receiver.Channel.BasicCancelAsync(receiver.ConsumerTag);
        await receiver.Channel.DisposeAsync();
        await receiver.Connection.DisposeAsync();
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (receiver is null)
        {
            return;
        }

        // Trace
        using Activity? activity = TraceActivitySource.Value.StartActivity("RECEIVE", ActivityKind.Consumer);

        try
        {
            Message message = DeserializeMessage(args.Body);
            activity?.SetTag("ReceivedMessage", message.Text);

            // Log
            logger.LogInformation("Received message {text}", message.Text);

            // Metric
            messageCounter.Increment();

            await messages.Clients.All.SendAsync("ReceiveMessage", message.Text, message.Timestamp, args.CancellationToken);
            await receiver.Channel.BasicAckAsync(args.DeliveryTag, false, args.CancellationToken);
        }
        catch (Exception exc)
        {
            activity?.AddException(exc);
            activity?.SetStatus(ActivityStatusCode.Error);
        }
    }

    private Message DeserializeMessage(ReadOnlyMemory<byte> body)
    {
        JsonDocument doc = JsonDocument.Parse(body);
        string? text = doc.RootElement.GetProperty("Text").GetString() ?? "--";
        string? timestamp = doc.RootElement.GetProperty("Timestamp").GetString() ?? "--";
        return new Message(text, timestamp);
    }

    private async Task<RabbitReceiver?> CreateRabbitReceiver(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new();
        string endpoint = configuration["BrokerEndpoint"] ?? throw new InvalidOperationException("Could not get the broker endpoint");
        factory.Uri = new Uri(endpoint);

        IConnection conn = await factory.CreateConnectionAsync(cancellationToken: cancellationToken);
        IChannel channel = await conn.CreateChannelAsync(cancellationToken: cancellationToken);

        AsyncEventingBasicConsumer consumer = new(channel);
        consumer.ReceivedAsync += HandleMessageAsync;
        string? consumerTag = await channel.BasicConsumeAsync(queue: "test", autoAck: false, consumer, cancellationToken);
        return new RabbitReceiver(consumerTag, channel, conn);
    }
}

internal record RabbitReceiver(string ConsumerTag, IChannel Channel, IConnection Connection);
internal record Message(string Text, string Timestamp);