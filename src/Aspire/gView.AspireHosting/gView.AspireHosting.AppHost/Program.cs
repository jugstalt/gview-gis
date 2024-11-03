var builder = DistributedApplication.CreateBuilder(args);

#region Database

var postgresPassword = builder.AddParameter("postgresql-password", "postgres");

// Add a PostgreSQL container using the PostGIS-enabled image
var postgres = builder
                    .AddPostgres("postgres", password: postgresPassword)
                    .WithImage("postgis/postgis")
                    .WithDataVolume("gview-gis-postgis")
                    .WithInitBindMount(source: "C:\\postgres\\init")
                    .WithPgAdmin();

#endregion

var gViewServer = builder
                    .AddgViewServer("gview-server")
                    .Build();

var gViewWebApps = builder
                    .AddgViewWebApps("gview-webapps")
                    .WithDrive("GEODATA", "/geodata", @"C:\temp\GeoDaten")
                    .WithgViewServer(gViewServer)
                    .Build();

builder.Build().Run();
