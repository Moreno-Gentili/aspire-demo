using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;

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

                    Message.Text = "";
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