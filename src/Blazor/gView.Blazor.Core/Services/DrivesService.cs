using gView.Blazor.Core.Extensions;
using gView.Blazor.Core.Services.Abstraction;
using gView.Framework.Blazor.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;

namespace gView.Blazor.Core.Services;

public class DrivesService
{
    private readonly DrivesServiceOptions _options;
    private readonly IAppIdentityProvider _identityProvider;

    public DrivesService(IAppIdentityProvider identityProvider,
                         IOptions<DrivesServiceOptions> options)
    {
        _identityProvider = identityProvider;
        _options = options.Value;
    }

    public IEnumerable<VirtualDrive> VirtualDrives()
    {
        List<VirtualDrive> virtualDrives = new();

        if(_options.Drives != null)
        {
            foreach(var key in _options.Drives.Keys)
            {
                var path = _options.Drives[key];
                if(path.Contains("{{username}}"))
                {
                    path = path.Replace("{{username}}",
                        string.IsNullOrEmpty(_identityProvider.Identity.Username)
                            ? "_"
                            : _identityProvider.Identity.Username.UserNameToFolder());
                    path.TryCreateDirectory();
                }
                virtualDrives.Add(new VirtualDrive(
                        key.ToEnvironmentVairableName(), 
                        path, 
                        VirtualDriveType.EnvironmentVariable
                    ));
            }
        }
        else
        {
            string[] logicalDrives = System.IO.Directory.GetLogicalDrives();

            foreach (string logicalDrive in logicalDrives)
            {
                System.IO.DriveInfo info = new System.IO.DriveInfo(logicalDrive);

                virtualDrives.Add(new VirtualDrive(
                        info.Name.Replace("\\", ""), 
                        info.Name, 
                        VirtualDriveType.Drive, info.DriveType
                    ));
            }
        }

        return virtualDrives;
    }

    #region Models

   
    #endregion
}
