using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gView.GeoJsonService.DTOs;

public class ErrorResponse
{
    public string Type => "ErrorResponse";

    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = "";
}
