using RabbitMQ.Client;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace ProgettoAspNet
{
    public partial class Health : System.Web.UI.Page
    {
        protected async Task Page_Load(object sender, EventArgs e)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = new Uri(ConfigurationManager.AppSettings["BrokerEndpoint"]);

            using (IConnection conn = await factory.CreateConnectionAsync())
            {
                await conn.CloseAsync();
            }
        }
    }
}