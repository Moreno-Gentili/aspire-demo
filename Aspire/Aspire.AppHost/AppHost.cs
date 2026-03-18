IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(new DistributedApplicationOptions
{
    Args = args,
    AllowUnsecuredTransport = true // Dashboard on http: instead of https:
});

// Adds a containerized dependency
var broker = builder
    .AddRabbitMQ(
        name: "Broker",
        userName: builder.AddParameter("BrokerUsername", "guest"),
        password: builder.AddParameter("BrokerPassword", "guest"))
    .WithManagementPlugin()
    .WithBindMount("./Configuration/rabbitmq.conf", "/etc/rabbitmq/rabbitmq.conf")
    .WithBindMount("./Configuration/definitions.json", "/etc/rabbitmq/definitions.json")
    .WithDataVolume("broker-data");

// Or, we can use the lower level API .AddContainer
/*builder.AddContainer(name: "Broker", image: "rabbitmq:4.2.5-management")
       .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
       .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
       .WithBindMount("./Configuration/rabbitmq.conf", "/etc/rabbitmq/rabbitmq.conf")
       .WithBindMount("./Configuration/definitions.json", "/etc/rabbitmq/definitions.json")
       .WithVolume("broker-data", "/var/lib/rabbitmq/mnesia/");*/

// Adds a ASP.NET Core (.NET) project
builder.AddProject<Projects.ProgettoAspNetCore>("ProgettoAspNetCore")
    .WithEnvironment("BrokerEndpoint", broker.Resource.ConnectionStringExpression)
    .WaitFor(broker);

// Adds a ASP.NET (.NET Framework) project
builder.AddIISExpressProject<Projects.ProgettoAspNet>("ProgettoAspNet") // Thanks Cynthia! https://github.com/CommunityToolkit/Aspire/issues/250
    .WithOtlpExporter()
    .WithHttpEndpoint(8881)
    .WithHttpHealthCheck("/health")
    .WithEnvironment("BrokerEndpoint", broker.Resource.ConnectionStringExpression)
    .WaitFor(broker);

// Executables
// builder.AddExecutable(...)

// Dockerfiles
// builder.AddDockerfile(...)

builder.Build().Run();
