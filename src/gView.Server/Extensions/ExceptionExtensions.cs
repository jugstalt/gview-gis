//using gView.Interoperability.GeoServices.Rest.Json;
//using Microsoft.AspNetCore.Mvc;
//using System;

//namespace gView.Server.Extensions;

//static internal class ExceptionExtensions
//{
//    static public IActionResult ToErrorResult(
//        this Exception nae, 
//        int errorCode, 
//        string errorMessage, 
//        string resultFormat)
//    {
//        var error = new JsonError()
//        {
//            Error = new JsonError.ErrorDef() { Code = 403, Message = nae.Message }
//        };

//        return resultFormat?.ToLower() switch
//        {
//            "image" => Json(error),  // image request must return token errors as json explicit!
//            _ => Result(error)
//        };
//    }
//}
