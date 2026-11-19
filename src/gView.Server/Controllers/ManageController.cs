using gView.Framework.Common;
using gView.Framework.Core.Common;
using gView.Framework.Core.Exceptions;
using gView.Framework.Core.IO;
using gView.Framework.Core.MapServer;
using gView.Framework.IO;
using gView.Framework.Security;
using gView.Framework.Security.Extensions;
using gView.Server.AppCode;
using gView.Server.AppCode.Extensions;
using gView.Server.Extensions;
using gView.Server.Models;
using gView.Server.Models.Manage;
using gView.Server.Services.Logging;
using gView.Server.Services.MapServer;
using gView.Server.Services.Network;
using gView.Server.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace gView.Server.Controllers;

[AdminNetworkOnly]
[AntiforgeryForCookieAuth]
public class ManageController : BaseController
{
    private readonly MapServiceManager _mapServiceMananger;
    private readonly MapServiceDeploymentManager _mapServiceDeploymentManager;
    private readonly LoginManager _loginManager;
    private readonly MapServicesEventLogger _logger;
    private readonly IConfiguration _configuration;
    private readonly NetworkServerService _network;
    private readonly AdminSetupService _adminSetup;

    public ManageController(MapServiceManager mapServiceManager,
                            MapServiceDeploymentManager mapServiceDeploymentManager,
                            LoginManager loginManager,
                            MapServicesEventLogger logger,
                            EncryptionCertificateService encryptionCertificateService,
                            IConfiguration configuration,
                            NetworkServerService network,
                            AdminSetupService adminSetup)
                            : base(mapServiceManager, loginManager, encryptionCertificateService)
    {
        _network = network;
        _adminSetup = adminSetup;
        _mapServiceMananger = mapServiceManager;
        _mapServiceDeploymentManager = mapServiceDeploymentManager;
        _loginManager = loginManager;
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        try
        {
            var authToken = _loginManager.GetAuthToken(this.Request);
            if (!authToken.IsManageUser)
            {
                return RedirectToAction("Login");
            }

            ViewData["mainMenuItems"] = "mainMenuItemsPartial";
            return View();
        }
        catch (Exception)
        {
            return RedirectToAction("Login");
        }
    }

    #region Login

    [HttpGet]
    public IActionResult Login()
    {
        var authConfig = new AuthConfigModel();
        _configuration.Bind("Authentication", authConfig);

        if ("oidc".Equals(authConfig.Type, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("OidcLogin");
        }

        if (Globals.AllowFormsLogin == false)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new ManageLoginModel()
        {
            SetupRequired = _adminSetup.IsSetupRequired(_loginManager)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(ManageLoginModel model)
    {
        try
        {
            if (Globals.AllowFormsLogin == false)
            {
                return RedirectToAction("Index", "Home");
            }

            //Console.WriteLine("UN: "+model.Username);
            //Console.WriteLine("PW: "+model.Password);

            if (String.IsNullOrWhiteSpace(model.Username))
            {
                throw new Exception("Username is required...");
            }

            if (String.IsNullOrWhiteSpace(model.Password))
            {
                throw new Exception("Password is required...");
            }

            if (_adminSetup.IsSetupRequired(_loginManager))
            {
                model.SetupRequired = true;

                _adminSetup.CreateFirstAdmin(
                    _loginManager,
                    this.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    model.SetupCode,
                    model.Username.Trim(),
                    model.Password.Trim(),
                    model.PasswordRepeat?.Trim());
            }

            var authToken = _loginManager.GetManagerAuthToken(
                model.Username.Trim(),
                model.Password.Trim(),
                exipreMinutes: 60 * this.HttpContext.RequestServices.GetRequiredService<SecurityOptions>().ManageSessionHours);

            if (authToken == null)
            {
                throw new Exception("Unknown user or password");
            }

            base.SetAuthCookie(authToken);

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            model.ErrorMessage = ex.Message;

            ex.ToConsole();

            return View(model);
        }
    }

    [Authorize]
    public IActionResult OidcLogin()
    {
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Ends all sessions of the current admin on all devices (new security stamp).
    /// </summary>
    [HttpPost]
    public IActionResult SignOutEverywhere()
    {
        var authToken = _loginManager.GetAuthToken(this.Request);

        if (authToken.IsManageUser && authToken.Stamp is not null)
        {
            _loginManager.SignOutEverywhere(authToken);
        }

        base.RemoveAuthCookie();

        return RedirectToAction("Login");
    }

    #endregion

    public IActionResult Collect() => SecureApiCall(() =>
    {
        var mem1 = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        GC.Collect();
        var mem2 = GC.GetTotalMemory(true) / 1024.0 / 1024.0;
        return Json(new { succeeded = true, mem1 = mem1, mem2 = mem2 });
    });

    #region Services

    async public Task<IActionResult> Folders()
    {
        return await SecureApiCall(async () =>
        {
            var folderServices = _mapServiceMananger.MapServices
                                .Where(s => s.Type == MapServiceType.Folder)
                                .Select(s => s)
                                .OrderBy(s => s.Name)
                                .Distinct();

            List<object> mapServiceJson = new List<object>();
            foreach (var folderService in folderServices)
            {
                mapServiceJson.Add(await MapService2Json(folderService, await folderService.GetSettingsAsync()));
            }

            return Json(new
            {
                success = true,
                folders = mapServiceJson.ToArray()
            });
        });
    }

    async public Task<IActionResult> Services(string folder)
    {
        folder = folder ?? String.Empty;

        return await SecureApiCall(async () =>
        {
            _mapServiceMananger.ReloadServices(folder, true);

            var servicesInFolder = _mapServiceMananger.MapServices
                .Where(s => s.Type != MapServiceType.Folder &&
                                s.Folder == folder);

            List<object> mapServiceJson = new List<object>();
            foreach (var serviceInFolder in servicesInFolder)
            {
                mapServiceJson.Add(await MapService2Json(serviceInFolder, await serviceInFolder.GetSettingsAsync()));
            }

            return Json(new
            {
                success = true,
                services = mapServiceJson.ToArray()
            });
        });
    }

    async public Task<IActionResult> SetServiceStatus(string service, string status)
    {
        return await SecureApiCall(async () =>
        {
            var mapService = _mapServiceMananger.MapServices.Where(s => s.Fullname == service).FirstOrDefault();
            if (mapService == null)
            {
                throw new MapServerException("Unknown service: " + service);
            }

            var settings = await mapService.GetSettingsAsync();
            switch (status.ToLower())
            {
                case "running":
                    if (!settings.IsRunning() || !_mapServiceMananger.Instance.IsLoaded(mapService.Name, mapService.Folder))
                    {
                        // start
                        settings.RefreshService = DateTime.UtcNow;
                        settings.Status = MapServiceStatus.Running;
                        await mapService.SaveSettingsAsync();

                        // reload
                        await _mapServiceMananger.Instance.GetServiceMapAsync(service.ServiceName(), service.FolderName());
                    }
                    break;
                case "stopped":
                    settings.Status = MapServiceStatus.Stopped;
                    _mapServiceDeploymentManager.MapDocument.RemoveMap(mapService.Fullname);
                    await mapService.SaveSettingsAsync();
                    break;
                case "refresh":
                    // stop
                    _mapServiceDeploymentManager.MapDocument.RemoveMap(mapService.Fullname);

                    // start
                    settings.RefreshService = DateTime.UtcNow;
                    settings.Status = MapServiceStatus.Running;
                    await mapService.SaveSettingsAsync();

                    // reload
                    await _mapServiceMananger.Instance.GetServiceMapAsync(service.ServiceName(), service.FolderName());
                    break;
            }

            return Json(new
            {
                success = true,
                service = await MapService2Json(mapService, settings)
            });
        });
    }

    #region Network

    /// <summary>The network of a service: where it is, whether and when it was built, the running or last build.</summary>
    [HttpGet]
    public IActionResult ServiceNetwork(string service) => SecureApiCall(() =>
    {
        var mapService = _mapServiceMananger.GetMapService(service) ?? throw new MapServerException("Unknown service: " + service);
        return Json(NetworkStatus(mapService));
    });

    /// <summary>Starts building the network of a service in the background.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ServiceNetworkBuild(string service) => SecureApiCall(() =>
    {
        var mapService = _mapServiceMananger.GetMapService(service) ?? throw new MapServerException("Unknown service: " + service);
        if (_network.Config(mapService) is null)
        {
            throw new MapServerException("The map of the service has no network configured.");
        }
        if (_network.StartBuild(mapService) is null)
        {
            throw new MapServerException("A build of this network is running already.");
        }
        return Json(NetworkStatus(mapService));
    });

    /// <summary>A report of the last build (build.log, duplicates.json, ...).</summary>
    [HttpGet]
    public IActionResult ServiceNetworkReport(string service, string file) => SecureApiCall(() =>
    {
        var mapService = _mapServiceMananger.GetMapService(service) ?? throw new MapServerException("Unknown service: " + service);
        if (!gView.Framework.NetworkModel.Build.NetworkBuildRunner.ReportFiles.Contains(file))
        {
            throw new MapServerException("Unknown report: " + file);
        }
        var path = Path.Combine(_network.NetworkDirectory(mapService) ?? throw new MapServerException("No network"), file);
        if (!System.IO.File.Exists(path))
        {
            throw new MapServerException("Report not found: " + file);
        }
        return File(System.IO.File.ReadAllBytes(path), file.EndsWith(".json") ? "application/json" : "text/plain", file);
    });

    private object NetworkStatus(IMapService mapService)
    {
        var config = _network.Config(mapService);
        var directory = _network.NetworkDirectory(mapService);
        var metadata = NetworkServerService.Metadata(directory);
        var job = _network.Job(mapService);
        return new
        {
            success = true,
            configured = config is not null,
            directory,
            built = metadata is not null,
            created = metadata?.Created.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            nodeCount = metadata?.NodeCount,
            edgeCount = metadata?.EdgeCount,
            aliasCount = metadata?.AliasCount,
            shardCount = metadata?.Shards.Count,
            rules = metadata?.Rules.Count,
            reports = directory is null
                ? []
                : gView.Framework.NetworkModel.Build.NetworkBuildRunner.ReportFiles.Where(f => System.IO.File.Exists(Path.Combine(directory, f))).ToArray(),
            job = job is null ? null : new
            {
                running = job.Running,
                succeeded = job.Succeeded,
                error = job.Error,
                started = job.Started.ToString("yyyy-MM-dd HH:mm:ss"),
                finished = job.Finished?.ToString("yyyy-MM-dd HH:mm:ss"),
                log = job.Log
            }
        };
    }

    #endregion

    public IActionResult ServiceErrorLogs(string service, string last = "0")
    {
        return SecureApiCall(() =>
        {
            var errorsResult = _logger.ErrorLogs(service, loggingMethod.error, long.Parse(last));

            return Json(new
            {
                errors = errorsResult.errors,
                ticks = errorsResult.ticks > 0 ? errorsResult.ticks.ToString() : null
            });
        });
    }

    [HttpGet]
    async public Task<IActionResult> ServiceSecurity(string service)
    {
        return await SecureApiCall(async () =>
        {
            service = service?.ToLower() ?? String.Empty;

            var mapService = _mapServiceMananger.MapServices.Where(s => s.Fullname?.ToLower() == service).FirstOrDefault();
            if (mapService == null)
            {
                throw new MapServerException("Unknown service: " + service);
            }

            var settings = await mapService.GetSettingsAsync();

            List<string> allTypes = new List<string>(Enum.GetNames(typeof(AccessTypes)).Select(n => n.ToLower()).Where(n => n != "none"));
            allTypes.Add("_all");

            var accessRules = settings.AccessRules;

            foreach (var interpreterType in _mapServiceMananger.Interpreters)
            {
                allTypes.Add("_" + ((IServiceRequestInterpreter)Activator.CreateInstance(interpreterType)).IdentityName.ToLower());
            }

            return Json(new
            {
                allTypes = allTypes.ToArray(),
                accessRules = accessRules,
                allUsers = _loginManager.GetManageAndTokenUsernames(),
                anonymousUsername = Identity.AnonyomousUsername
            });
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    async public Task<IActionResult> ServiceSecurity()
    {
        return await SecureApiCall(async () =>
        {
            var service = Request.Query["service"].ToString().ToLower();

            var mapService = _mapServiceMananger.MapServices.Where(s => s.Fullname?.ToLower() == service).FirstOrDefault();
            if (mapService == null)
            {
                throw new MapServerException("Unknown service: " + service);
            }

            var settings = await mapService.GetSettingsAsync();
            settings.AccessRules = null;  // Remove all

            if (Request.Form != null)
            {
                var form = Request.Form;
                foreach (var key in form.Keys)
                {
                    if (key.Contains("~"))   // username~accesstype or username~_interpreter
                    {
                        var username = key.Substring(0, key.IndexOf("~"));

                        var accessRule = settings.AccessRules?.Where(a => a.Username.ToLower() == username.ToLower()).FirstOrDefault();
                        if (accessRule == null)
                        {
                            accessRule = new MapServiceSettings.MapServiceAccess();
                            var rules = new List<IMapServiceAccess>();
                            if (settings.AccessRules != null)
                            {
                                rules.AddRange(settings.AccessRules);
                            }

                            rules.Add(accessRule);
                            settings.AccessRules = rules.ToArray();
                        }
                        accessRule.Username = username;


                        var rule = key.Substring(key.IndexOf("~") + 1, key.Length - key.IndexOf("~") - 1);

                        string serviceType = String.Empty;

                        if (rule == "_all")
                        {
                            serviceType = rule;
                        }
                        else if (rule.StartsWith("_") && _mapServiceMananger.Interpreters
                                        .Select(t => new Framework.Common.PlugInManager().CreateInstance<IServiceRequestInterpreter>(t))
                                        .Where(i => "_" + i.IdentityName.ToLower() == rule.ToLower())
                                        .Count() == 1)  // Interpreter
                        {
                            serviceType = rule.ToLower();
                        }
                        else if (Enum.TryParse<AccessTypes>(rule, true, out AccessTypes accessType))
                        {
                            serviceType = accessType.ToString();
                        }

                        if (!String.IsNullOrWhiteSpace(serviceType))
                        {
                            if (Convert.ToBoolean(form[key]) == true)
                            {
                                accessRule.AddServiceType(serviceType);
                            }
                            else
                            {
                                accessRule.RemoveServiceType(serviceType);
                            }
                        }
                    }
                }
            }

            await mapService.SaveSettingsAsync();

            return Json(new
            {
                success = true,
                service = await MapService2Json(mapService, settings)
            });
        });
    }

    [HttpGet]
    public Task<IActionResult> ServiceMetadata(string service)
        => SecureApiCall(async () =>
        {
            var pluginManager = new PlugInManager();
            var mapService = _mapServiceMananger.GetMapService(service);
            var map = await _mapServiceMananger.Instance.GetServiceMapAsync(mapService);
            Dictionary<string, string> meta = new Dictionary<string, string>();

            if (map == null)
            {
                return Json(meta);
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            List<IMetadataProvider> metadataProviders = new List<IMetadataProvider>();
            foreach (var metadataProviderType in pluginManager.GetPlugins(Framework.Common.Plugins.Type.IMetadataProvider))
            {
                var metadataProvider = (map.MetadataProvider(PlugInManager.PluginIDFromType(metadataProviderType))
                    ?? Activator.CreateInstance(metadataProviderType)) as IMetadataProvider;

                if (metadataProvider is IPropertyModel && await metadataProvider.ApplyTo(map, true) == true)
                {
                    metadataProviders.Add(metadataProvider);
                }
            }

            foreach (var metadataProvider in metadataProviders.OrderBy(m => m.Name))
            {
                var propertyObject = ((IPropertyModel)metadataProvider).GetPropertyModel();
                meta.Add(metadataProvider.Name, serializer.Serialize(propertyObject));
            }

            return Json(meta);
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> ServiceMetadata(string service, Dictionary<string, string> metadata) => SecureApiCall(async () =>
    {
        if (metadata == null && metadata.Keys.Count == 0)
        {
            return Json(new { success = false });
        }

        var pluginManager = new PlugInManager();
        var mapService = _mapServiceMananger.GetMapService(service);
        var map = (await _mapServiceMananger.Instance.GetServiceMapAsync(mapService)).ThrowIfNull();

        var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        var metadataProviders = new List<IMetadataProvider>();

        foreach (var metadataProviderType in pluginManager.GetPlugins(Framework.Common.Plugins.Type.IMetadataProvider))
        {
            var metadataProvider = (map.MetadataProvider(PlugInManager.PluginIDFromType(metadataProviderType))
                ?? Activator.CreateInstance(metadataProviderType)) as IMetadataProvider;

            if (metadataProvider is IPropertyModel &&
                metadata.ContainsKey(metadataProvider.Name) &&
                await metadataProvider.ApplyTo(map) == true)
            {
                try
                {
                    var propertyObject = deserializer.Deserialize(metadata[metadataProvider.Name],
                                                                  ((IPropertyModel)metadataProvider).PropertyModelType);

                    ((IPropertyModel)metadataProvider).SetPropertyModel(propertyObject);
                    metadataProviders.Add(metadataProvider);

                }
                catch (YamlException see)
                {
                    throw new MapServerException($"Syntax Error ({metadataProvider.Name}): {see.InnerException?.Message ?? see.Message} in Line: {see.End.Line} Column: {see.End.Column}");
                }
            }
        }

        var meta = new gView.Framework.Data.Metadata.Metadata();
        await meta.SetMetadataProviders(metadataProviders, map);
        XmlStream xmlStream = new XmlStream("");
        await meta.WriteMetadata(xmlStream);

        FileInfo fi = new FileInfo($"{_mapServiceMananger.Options.ServicesPath}/{service}.meta");
        xmlStream.WriteStream(fi.FullName);

        return Json(new { success = true });
    });

    [HttpGet]
    async public Task<IActionResult> FolderSecurity(string folder)
    {
        return await SecureApiCall(async () =>
        {
            folder = folder?.ToLower() ?? String.Empty;

            var mapService = _mapServiceMananger.MapServices.Where(s => s.Type == MapServiceType.Folder && s.Fullname?.ToLower() == folder).FirstOrDefault();
            if (mapService == null)
            {
                throw new MapServerException("Unknown folder: " + folder);
            }

            var settings = await mapService.GetSettingsAsync();

            List<string> allTypes = new List<string>(Enum.GetNames(typeof(FolderAccessTypes)).Select(n => n.ToLower()).Where(n => n != "none"));
            allTypes.Add("_all");

            var accessRules = settings.AccessRules;

            foreach (var interpreterType in _mapServiceMananger.Interpreters)
            {
                allTypes.Add("_" + ((IServiceRequestInterpreter)Activator.CreateInstance(interpreterType)).IdentityName.ToLower());
            }

            return Json(new
            {
                allTypes = allTypes.ToArray(),
                accessRules = accessRules,
                allUsers = _loginManager.GetManageAndTokenUsernames(),
                anonymousUsername = Identity.AnonyomousUsername,

                onlineResource = settings.OnlineResource,
                outputUrl = settings.OutputUrl
            });
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    async public Task<IActionResult> FolderSecurity()
    {
        return await SecureApiCall(async () =>
        {
            var folder = Request.Query["folder"].ToString().ToLower();

            var mapService = _mapServiceMananger.MapServices.Where(s => s.Type == MapServiceType.Folder && s.Fullname?.ToLower() == folder).FirstOrDefault();
            if (mapService == null)
            {
                throw new MapServerException("Unknown folder: " + folder);
            }

            var settings = await mapService.GetSettingsAsync();
            settings.AccessRules = null;  // Remove all

            if (Request.Form != null)
            {
                var form = Request.Form;
                foreach (var key in form.Keys)
                {
                    if (key.Contains("~"))   // username~accesstype or username~_interpreter
                    {
                        var username = key.Substring(0, key.IndexOf("~"));

                        var accessRule = settings.AccessRules?.Where(a => a.Username.ToLower() == username.ToLower()).FirstOrDefault();
                        if (accessRule == null)
                        {
                            accessRule = new MapServiceSettings.MapServiceAccess();
                            var rules = new List<IMapServiceAccess>();
                            if (settings.AccessRules != null)
                            {
                                rules.AddRange(settings.AccessRules);
                            }

                            rules.Add(accessRule);
                            settings.AccessRules = rules.ToArray();
                        }
                        accessRule.Username = username;


                        var rule = key.Substring(key.IndexOf("~") + 1, key.Length - key.IndexOf("~") - 1);

                        string serviceType = String.Empty;

                        if (rule == "_all")
                        {
                            serviceType = rule;
                        }
                        else if (rule.StartsWith("_") && _mapServiceMananger.Interpreters
                                        .Select(t => new Framework.Common.PlugInManager().CreateInstance<IServiceRequestInterpreter>(t))
                                        .Where(i => "_" + i.IdentityName.ToLower() == rule.ToLower())
                                        .Count() == 1)  // Interpreter
                        {
                            serviceType = rule.ToLower();
                        }
                        else if (Enum.TryParse<FolderAccessTypes>(rule, true, out FolderAccessTypes accessType))
                        {
                            serviceType = accessType.ToString();
                        }

                        if (!String.IsNullOrWhiteSpace(serviceType))
                        {
                            if (Convert.ToBoolean(form[key]) == true)
                            {
                                accessRule.AddServiceType(serviceType);
                            }
                            else
                            {
                                accessRule.RemoveServiceType(serviceType);
                            }
                        }
                    }
                    else
                    {
                        switch (key)
                        {
                            case "advancedsettings_onlineresource":
                                settings.OnlineResource = String.IsNullOrWhiteSpace(form[key]) ? null : form[key].ToString();
                                break;
                            case "advancedsettings_outputurl":
                                settings.OutputUrl = String.IsNullOrWhiteSpace(form[key]) ? null : form[key].ToString();
                                break;
                        }
                    }
                }
            }

            await mapService.SaveSettingsAsync();

            return Json(new
            {
                success = true,
                folder = await MapService2Json(mapService, settings)
            });
        });
    }

    #endregion

    #region Security

    #region TokenUsers (=Clients)

    public IActionResult TokenUsers()
    {
        return SecureApiCall(() =>
        {
            return Json(new { users = _loginManager.GetTokenUsernames() });
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TokenUserCreate(CreateTokenUserModel model)
    {
        return SecureApiCall(() =>
        {
            model.NewUsername = model.NewUsername?.Trim() ?? String.Empty;
            model.NewPassword = model.NewPassword?.Trim() ?? String.Empty;

            if (String.IsNullOrWhiteSpace(model.NewUsername))
            {
                throw new MapServerException("Username is empty");
            }

            if (model.NewUsername.StartsWith(Globals.UrlTokenNamePrefix))
            {
                throw new MapServerException($"{Globals.UrlTokenNamePrefix} is not allowed as prefix for a username");
            }

            _loginManager.CreateTokenLogin(model.NewUsername.ToLower(), model.NewPassword);

            return Json(new { success = true });
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TokenUserChangePassword(ChangeTokenUserPasswordModel model)
    {
        return SecureApiCall(() =>
        {
            model.Username = model.Username?.Trim() ?? String.Empty;
            model.NewPassword = model.NewPassword?.Trim() ?? String.Empty;

            _loginManager.ChangeTokenUserPassword(model.Username, model.NewPassword);

            return Json(new { success = true });
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TokenUserDelete(DeleteTokenUserModel model)
    {
        return SecureApiCall(() =>
        {
            model.Username = model.Username?.Trim() ?? String.Empty;

            _loginManager.DeleteTokenLogin(model.Username);

            return Json(new { success = true });
        });
    }

    #endregion

    #region Url Tokens

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UrlTokenCreate(CreateUrlTokenModel model) => SecureApiCall(() =>
    {
        return SecureApiCall(() =>
        {
            model.NewTokenName = model.NewTokenName?.Trim().ToLower() ?? String.Empty;

            if (String.IsNullOrWhiteSpace(model.NewTokenName))
            {
                throw new MapServerException("token is empty");
            }

            model.NewTokenName.ValidateRawUrlTokenName();

            string tokenName = $"{Globals.UrlTokenNamePrefix}{model.NewTokenName}";
            string token = $"{tokenName}~{SecureCrypto.GenerateToken(64)}";

            _loginManager.CreateTokenLogin(tokenName, token);

            // only stored hashed: this is the only time the token can be shown
            return Json(new { success = true, token });
        });
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UrlTokenRecycle(UrlTokenModel model)
    {
        return SecureApiCall(() =>
        {
            string tokenName = model.UrlToken.NameOfUrlToken();

            string token = $"{tokenName}~{SecureCrypto.GenerateToken(64)}";

            _loginManager.ChangeTokenUserPassword(tokenName, token);

            // only stored hashed: this is the only time the token can be shown
            return Json(new { success = true, token });
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UrlTokenDelete(UrlTokenModel model)
    {
        return SecureApiCall(() =>
        {
            string tokenName = model.UrlToken.NameOfUrlToken();

            _loginManager.DeleteTokenLogin(tokenName);

            return Json(new { success = true });
        });
    }

    #endregion

    #endregion

    #region Helper

    private IActionResult SecureApiCall(Func<IActionResult> func)
    {
        try
        {
            var authToken = _loginManager.GetAuthToken(this.Request);
            if (!authToken.IsManageUser)
            {
                throw new Exception("Not allowed");
            }

            return func.Invoke();
        }
        catch (NotAuthorizedException)
        {
            return Json(new { success = false, error = "not authorized" });
        }
        catch (MapServerException mse)
        {
            return Json(new { success = false, error = mse.Message });
        }
        catch (Exception)
        {
            return Json(new { success = false, error = "unknown error" });
        }
    }

    async private Task<IActionResult> SecureApiCall(Func<Task<IActionResult>> func)
    {
        try
        {
            var authToken = _loginManager.GetAuthToken(this.Request);
            if (!authToken.IsManageUser)
            {
                throw new Exception("Not allowed");
            }

            return await func.Invoke();
        }
        catch (NotAuthorizedException)
        {
            base.RemoveAuthCookie();
            return Json(new { success = false, code = 401, error = "not authorized" });
        }
        catch (InvalidTokenException)
        {
            base.RemoveAuthCookie();
            return Json(new { success = false, code = 498, error = "Session expired" });
        }
        catch (MapServerException mse)
        {
            return Json(new { success = false, code = 500, error = mse.Message });
        }
        catch (Exception/* ex*/)
        {
            return Json(new { success = false, code = 500, error = $"unknown error" });
        }
    }

    async private Task<object> MapService2Json(IMapService mapService, IMapServiceSettings settings)
    {
        var status = settings?.Status ?? MapServiceStatus.Running;

        if (status == MapServiceStatus.Running)
        {
            if (!_mapServiceMananger.Instance.IsLoaded(mapService.Name, mapService.Folder))
            {
                status = MapServiceStatus.Idle;
            }
        }

        bool hasErrors = await _logger.LogFileExists(mapService.Fullname, loggingMethod.error);

        return new
        {
            name = mapService.Name,
            folder = mapService.Folder,
            status = status.ToString().ToLower(),
            hasSecurity = settings?.AccessRules != null && settings.AccessRules.Length > 0,
            runningSince = settings?.Status == MapServiceStatus.Running && mapService.RunningSinceUtc.HasValue ?
                mapService.RunningSinceUtc.Value.ToShortDateString() + " " + mapService.RunningSinceUtc.Value.ToLongTimeString() + " (UTC)" :
                String.Empty,
            hasErrors = hasErrors,
            hasNetwork = _network.Config(mapService) is not null
        };
    }

    #endregion
}