using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using WebApplication1.Telemetry;

namespace ProgettoAspNet
{
    public partial class _Default : Page
    {
        protected void Send_Click(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(SendMessageAsync));
        }

        private async Task SendMessageAsync(CancellationToken cancellationToken)
        {
            // Trace
            using (Activity activity = TraceActivitySource.Value.StartActivity("PUBLISH", ActivityKind.Producer))
            {
                string username = User.Identity.IsAuthenticated ? User.Identity.Name : "Anonymous";
                activity.AddTag("Username", username);
                activity.AddTag("Message", Message.Text);

                // Log
                Global.Logger.LogInformation("Publish message {text}", Message.Text);

                // Metric
                MessageCounter.Increment();

                await SendMessageToBrokerAsync(Message.Text, activity.Id, cancellationToken);
                Message.Text = "";
            }
        }

        private async Task SendMessageToBrokerAsync(string message, string traceId, CancellationToken cancellationToken)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri(ConfigurationManager.AppSettings["BrokerEndpoint"]);

            using (IConnection conn = await factory.CreateConnectionAsync(cancellationToken))
            {
                using (IChannel channel = await conn.CreateChannelAsync(cancellationToken: cancellationToken))
                {
                    byte[] messageBody = SerializeMessage(Message.Text);
                    await channel.BasicPublishAsync(
                        exchange: "test",
                        routingKey: string.Empty,
                        body: messageBody, cancellationToken);
                }
            }
        }

        private byte[] SerializeMessage(string text)
        {
            var message = new { Text = text, Timestamp = DateTimeOffset.Now };
            string payload = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(payload);
        }
    }
}