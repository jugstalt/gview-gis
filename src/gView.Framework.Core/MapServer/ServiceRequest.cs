using gView.Framework.Core.Common;
using System.Text.Json;
using System;

namespace gView.Framework.Core.MapServer
{
    public class ServiceRequest
    {
        public string Service { get; private set; }
        public string Folder { get; private set; }
        public string Request { get; private set; }

        #region Response

        private object _response = null;

        public object Response
        {
            get
            {
                return _response;
            }
            set
            {
                _response = value;
            }
        }

        public string ResponseAsString
        {
            get
            {
                if (_response == null)
                {
                    return string.Empty;
                }

                if (_response is string)
                {
                    return (string)_response;
                }
                else if (_response is byte[])
                {
                    return $"base64:{Convert.ToBase64String((byte[])_response)}";
                }
                else
                {
                    return JsonSerializer.Serialize(_response);
                }
            }
        }

        #endregion

        public string ResponseContentType = "";
        public DateTime? ResponseExpries = null;

        public string OnlineResource = "";
        public string OutputUrl = "";
        public IIdentity Identity = null;
        public bool Succeeded = true;
        public string Method = "";

        private string _errorId = null;
        /// <summary>
        /// Links a generic error answer to the log entry (see ExceptionExtensions)
        /// </summary>
        public string ErrorId
        {
            get => _errorId ??= gView.Framework.Core.Extensions.ExceptionExtensions.NewErrorId();
            set => _errorId = value;
        }

        public ServiceRequest(string service, string folder, string request)
        {
            Service = service;
            Folder = folder;
            Request = request;
        }

        static public ServiceRequest CreateGerneral(string method, IIdentity identity)
            => new ServiceRequest("", "", "")
            {
                Method = method,
                Identity = identity
            };

    }
}
