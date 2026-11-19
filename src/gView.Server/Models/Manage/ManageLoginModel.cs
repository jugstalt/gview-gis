using System.ComponentModel.DataAnnotations;

namespace gView.Server.Models.Manage
{
    public class ManageLoginModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// No admin exists yet: the form creates the first admin with the setup code from the server log.
        /// </summary>
        public bool SetupRequired { get; set; }

        public string SetupCode { get; set; }

        [DataType(DataType.Password)]
        public string PasswordRepeat { get; set; }

        public string ErrorMessage { get; set; }
    }
}
