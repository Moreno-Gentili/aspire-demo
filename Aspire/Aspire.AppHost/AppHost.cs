using Aspire.AppHost;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
{
    Args = args,
    AllowUnsecuredTransport = true
});

var broker = builder
    .AddRabbitMQ(
        name: "broker",
        userName: builder.AddParameter("userName", "guest"),
        password: builder.AddParameter("password", "guest"))
    .WithManagementPlugin()
    .WithBindMount("./Configuration/rabbitmq.conf", "/etc/rabbitmq/rabbitmq.conf")
    .WithBindMount("./Configuration/definitions.json", "/etc/rabbitmq/definitions.json")
    .WithDataVolume("broker-data");

/*var broker = builder
    .AddContainer(name: "Broker", image: "rabbitmq:4.2.4-management")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
    .WithVolume("BrokerData", "/var/lib/rabbitmq/mnesia/");*/

builder.AddProject<Projects.ProgettoAspNetCore>("AspNetCore")
       .WithEnvironment("BrokerEndpoint", broker.Resource.ConnectionStringExpression)
       .WaitFor(broker);

builder.AddIISExpressProject<Projects.ProgettoAspNet>() // Thanks Cynthia MacLeod (not Microsoft)
    .WithOtlpExporter()
    .WithHttpEndpoint(8881)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BrokerEndpoint", broker.Resource.ConnectionStringExpression);

builder.Build().Run();
