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

builder.AddProject<Projects.ProgettoAspNetCore>("AspNetCore")
       .WithEnvironment("BrokerEndpoint", broker.Resource.ConnectionStringExpression)
       .WaitFor(broker);

builder.AddIISExpressProject<Projects.ProgettoAspNet>()
    .WithOtlpExporter()
    .WithHttpEndpoint(8881)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BrokerEndpoint", broker.Resource.ConnectionStringExpression);

builder.Build().Run();
