using System;
using System.Collections.Generic;

namespace Proj4Net.Parser
{

public class Proj4Keyword 
{

// ReSharper disable InconsistentNaming
  public const String a = "a";
  public const String b = "b";
  public const String f = "f";
  public const String alpha = "alpha";
  public const String datum = "datum";
  public const String ellps = "ellps";
  public const String es = "es";

  public const String azi = "azi";
  public const String k = "k";
  public const String k_0 = "k_0";
  public const String lat_ts = "lat_ts";
  public const String lat_0 = "lat_0";
  public const String lat_1 = "lat_1";
  public const String lat_2 = "lat_2";
  public const String lon_0 = "lon_0";
  public const String lonc = "lonc";
  public const String pm = "pm";
  
  public const String proj = "proj";
  
  public const String R = "R";
  public const String R_A = "R_A";
  public const String R_a = "R_a";
  public const String R_V = "R_V";
  public const String R_g = "R_g";
  public const String R_h = "R_h";
  public const String R_lat_a = "R_lat_a";
  public const String R_lat_g = "R_lat_g";
  public const String rf = "rf";
  
  public const String south = "south";
  public const String to_meter = "to_meter";
  public const String towgs84 = "towgs84";
  public const String units = "units";
  public const String x_0 = "x_0";
  public const String y_0 = "y_0";
  public const String zone = "zone";
  
  public const String title = "title";
  public const String nadgrids = "nadgrids";
  public const String no_defs = "no_defs";
  public const String wktext = "wktext";
    // ReSharper restore InconsistentNaming


  private static List<String> _supportedParams;
    public static List<String> SupportedParameters
  {
    get {
        if (_supportedParams == null)
        {
            _supportedParams = new List<string>
                                   {
                                       a,
                                       rf,
                                       f,
                                       alpha,
                                       es,
                                       b,
                                       datum,
                                       ellps,
                                       R_A,
                                       k,
                                       k_0,
                                       lat_ts,
                                       lat_0,
                                       lat_1,
                                       lat_2,
                                       lon_0,
                                       lonc,
                                       x_0,
                                       y_0,
                                       proj,
                                       south,
                                       towgs84,
                                       to_meter,
                                       units,
                                       zone,
                                       title,
                                       no_defs,
                                       wktext,
                                       nadgrids,
                                       pm
                                   };

            _supportedParams.Sort();
        }
        return _supportedParams;
    }
  }
  
  public static Boolean IsSupported(String paramKey)
  {
    //return _supportedParams.Contains(paramKey);
      return SupportedParameters.BinarySearch(paramKey) >= 0;
  }
  
  public static void CheckUnsupported(String paramKey)
  {
    if (! IsSupported(paramKey)) {
      throw new UnsupportedParameterException(paramKey + " parameter is not supported");
    }
  }
  
  public static void CheckUnsupported(IEnumerable<String> parameters)
  {
    foreach(string parameter in parameters) {
      CheckUnsupported(parameter);
    }
  }
}
}