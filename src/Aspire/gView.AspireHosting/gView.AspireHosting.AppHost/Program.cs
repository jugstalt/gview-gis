var builder = DistributedApplication.CreateBuilder(args);

#region Database

/*
var postgresPassword = builder.AddParameter("postgresql-password", "postgres");

// Add a PostgreSQL container using the PostGIS-enabled image
var postgres = builder
                    .AddPostgres("gview-postgis", password: postgresPassword)
                    .WithImage("postgis/postgis")
                    .WithDataVolume("gview-gis-postgis")
                    //.WithInitBindMount(source: "C:\\postgres\\init")
                    .WithContainerName("gview-postgis")
                    .WithPgAdmin(containerName: "gview-pgadmin")
                    .WithLifetime(ContainerLifetime.Persistent);

*/

#endregion

var gViewServer = builder
                    .AddgViewServer("gview-server", 8090)
                    .Build()
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithContainerName("gview-server");

var gViewWebApps = builder
                    .AddgViewWebApps("gview-webapps")
                    .WithDrive("GEODATA", "/geodata", @"C:\temp\GeoDaten")
                    .WithgViewServer(gViewServer)
                    .Build()
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithContainerName("gview-webapps");           

builder.Build().Run();
