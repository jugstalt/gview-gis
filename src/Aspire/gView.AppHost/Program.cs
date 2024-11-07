using MessageQueueNET.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

//optional
//builder.AddMessageQueueNET();
//builder.AddMessageQueueNETDashboard();

builder.AddProject<Projects.gView_Server>("gview-server");

builder.AddProject<Projects.gView_WebApps>("gview-webapps");

builder.Build().Run();