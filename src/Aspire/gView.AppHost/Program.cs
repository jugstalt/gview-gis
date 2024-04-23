using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddContainer("message-queue", "gstalt/messagequeue_net", "latest")
    .WithHttpEndpoint(9091, 8080)
    .WithBindMount("messagequeue", "/etc/messagequeue")
    .WithEnvironment(e =>
    {
        e.EnvironmentVariables.Add("SWAGGERUI", "true");
        e.EnvironmentVariables.Add("MESSAGEQUEUE__PERSIST__TYPE", "filesystem");
        e.EnvironmentVariables.Add("MESSAGEQUEUE__PERSIST__ROOTPATH", "/etc/messagequeue/persist");
        e.EnvironmentVariables.Add("MESSAGEQUEUE__MAXREQUESTPOLLINGSECONDS", "8");
    });
builder.AddContainer("message-queue-dashboard", "gstalt/messagequeue_net_dashboard", "latest")
    .WithHttpEndpoint(9090, 8080)
    .WithEnvironment(e =>
    {
        e.EnvironmentVariables.Add("DASHBOARD__QUEUES__0__NAME", "gview-server");
        e.EnvironmentVariables.Add("DASHBOARD__QUEUES__0__URL", "http://docker.for.mac.localhost:9091");
    });

builder.AddProject<Projects.gView_Server>("gview-server");

builder.AddProject<Projects.gView_Web>("gview-web");

builder.Build().Run();