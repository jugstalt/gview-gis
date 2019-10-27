//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using gView.MapServer;

//namespace gView.Interoperability.Server.Admin
//{
//    [gView.Framework.system.RegisterPlugIn("FDDF09E4-DE73-41af-B09C-DCB7CC94B29D")]
//    class MapServerInterpreter : IServiceRequestInterpreter
//    {
//        private IMapServer _mapServer = null;

//        #region IServiceRequestInterpreter Member

//        public void OnCreate(IMapServer mapServer)
//        {
//            _mapServer = mapServer;
//        }

//        async public Task Request(IServiceRequestContext context)
//        {
//            if (context == null || context.ServiceRequest == null)
//                return;

//            if (_mapServer == null)
//            {
//                context.ServiceRequest.Response = "<FATALERROR>MapServer Object is not available!</FATALERROR>";
//                return;
//            }

//            switch (context.ServiceRequest.Request)
//            {
//                case "options":
//                    StringBuilder sb = new StringBuilder();
//                    sb.Append("<MapServer><Options>");
//                    sb.Append("<OutputPath>" + _mapServer.OutputPath + "</OutputPath>");
//                    sb.Append("<OutputUrl>" + _mapServer.OutputUrl + "</OutputUrl>");
//                    sb.Append("<TileCachePath>" + _mapServer.TileCachePath + "</TileCachePath>");
//                    sb.Append("</Options></MapServer>");
//                    context.ServiceRequest.Response = sb.ToString();
//                    break;
//            }
//        }

//        public AccessTypes RequiredAccessTypes(IServiceRequestContext context)
//        {
//            return AccessTypes.Edit | AccessTypes.Map | AccessTypes.Query;
//        }

//        public string IntentityName
//        {
//            get { return String.Empty; }
//        }

//        public InterpreterCapabilities Capabilities
//        {
//            get { return null; }
//        }

//        public string IdentityName => "MapServerInterpreter";

//        public string IdentityLongName => "MapServerInterpreter";

//        #endregion
//    }
//}
