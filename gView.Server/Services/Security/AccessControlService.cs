using gView.Core.Framework.Exceptions;
using gView.Framework.system;
using gView.Server.AppCode;
using gView.Server.Services.MapServer;
using System;
using System.Threading.Tasks;

namespace gView.Server.Services.Security
{
    public class AccessControlService
    {
        private readonly MapServiceManager _mapServerService;
        private readonly LoginManager _loginManagerService;

        public AccessControlService(MapServiceManager mapServerService, LoginManager loginManagerService)
        {
            _mapServerService = mapServerService;
            _loginManagerService = loginManagerService;
        }

        public IIdentity GetIdentity(string user, string password)
        {
            if (String.IsNullOrWhiteSpace(user))
            {
                return new Identity(Identity.AnonyomousUsername);
            }

            var authToken = _loginManagerService.GetAuthToken(user, password);

            if (authToken == null)
            {
                throw new MapServerException("Unknown user or password");
            }

            return new Identity(user);
        }

        async public Task CheckPublishAccess(string folder, string usr, string pwd)
        {
            var identity = GetIdentity(usr, pwd);

            await CheckPublishAccess(folder, identity);
        }

        async public Task CheckPublishAccess(string folder, IIdentity identity)
        {
            var folderService = _mapServerService.GetFolderService(folder);
            if (folderService == null)
            {
                throw new MapServerException("Unknown folder: " + folder);
            }

            if (identity != null && identity.IsAdministrator)
            {
                return;
            }

            if (!await folderService.HasPublishAccess(identity))
            {
                throw new MapServerException("Forbidden for user " + identity.UserName);
            }
        }
    }
}
