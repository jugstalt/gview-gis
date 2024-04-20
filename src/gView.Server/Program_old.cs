//using gView.Framework.Common;
//using gView.Server.AppCode;
//using Microsoft.AspNetCore;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;

//namespace gView.Server
//{
//    public class Program_
//    {
//        public static void Main_(string[] args)
//        {
//            try
//            {
//                #region Init the global PluginManager

//                PlugInManager.Init();

//                #endregion

//                #region First Start => init configuration

//                new Setup().TrySetup(args);

//                #endregion

//                CreateHostBuilder(args).Build().Run();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Exiting program:");
//                Console.WriteLine(ex.Message);
//                Console.WriteLine(ex.StackTrace);
//            }
//        }

//        public static IWebHostBuilder CreateHostBuilder(string[] args)
//        {
//            var webhostBuilder = WebHost.CreateDefaultBuilder(args)
//                .ConfigureLogging(logging =>
//                {
//                    logging.AddConsole();
//                })
//                .ConfigureAppConfiguration(builder =>
//                {
//                    builder.AddJsonFile("_config/mapserver.json", optional: true, reloadOnChange: false);
//                })
//                .UseStartup<Startup>();

//            #region Expose Ports

//            List<string> urls = new List<string>();
//            for (int i = 0; i < args.Length - 1; i++)
//            {
//                switch (args[i].ToLower())
//                {
//                    case "-expose-http":
//                        urls.Add("http://localhost:" + int.Parse(args[++i]));
//                        break;
//                    case "-expose-https":
//                        urls.Add("https://localhost:" + int.Parse(args[++i]));
//                        break;
//                }
//            }
//            if (urls.Count > 0)
//            {
//                webhostBuilder = webhostBuilder.UseUrls(urls.ToArray());

//                foreach (var url in urls)
//                {
//                    Console.WriteLine($"Exposing: {url}");
//                }
//            }

//            #endregion

//            return webhostBuilder;
//        }
//    }
//}
