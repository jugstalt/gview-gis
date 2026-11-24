using gView.Server.Services.Security.TwoFactor;

namespace gView.Server.Models.Manage
{
    /// <summary>
    /// Second step of the admin login: the code of the authenticator app or a recovery code.
    /// </summary>
    public class ManageLoginCodeModel
    {
        public string Code { get; set; }

        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// The page Manage/TwoFactor: set up, replace, recovery codes, disable.
    /// </summary>
    public class ManageTwoFactorModel
    {
        public string Username { get; set; }
        public TwoFactorMode Mode { get; set; }

        /// <summary>
        /// Required mode, right after the password: the admin is not logged in yet.
        /// </summary>
        public bool IsPendingSetup { get; set; }

        public bool Enabled { get; set; }
        public int RemainingRecoveryCodes { get; set; }

        /// <summary>
        /// A setup (or replacement) in progress: QR code and secret.
        /// </summary>
        public TwoFactorSetup Setup { get; set; }

        /// <summary>
        /// New recovery codes, shown once.
        /// </summary>
        public string[] RecoveryCodes { get; set; }

        // form fields
        public string SetupToken { get; set; }
        public string Code { get; set; }
        public string Password { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}
