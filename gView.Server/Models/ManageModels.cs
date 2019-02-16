using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace gView.Server.Models
{
    public class ManageLoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }
    }

    public class CreateTokenUserModel
    {
        public string NewUsername { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangeTokenUserPasswordModel
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
    }
}
