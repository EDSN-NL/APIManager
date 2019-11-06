using System;
using System.DirectoryServices.AccountManagement;
using Framework.Logging;

namespace Framework.Util
{
    /// <summary>
    /// A simple singleton helper class that provides the name of the logged-in user.
    /// </summary>
    internal sealed class UserSlt
    {
        // This is the actual singleton. It is created automatically on first load.
        private static readonly UserSlt _UserSlt = new UserSlt();

        internal string DisplayName { get; }       // This is the user-friendly username, it might be an empty string!
        internal string PrincipalName { get; }     // Windows account name.
        internal string MailAddress { get; }       // User mail address, it might be an empty string!

        /// <summary>
        /// Public Context "factory" method. Simply returns the static instance.
        /// </summary>
        /// <returns>User singleton object</returns>
        internal static UserSlt GetUserSlt() { return _UserSlt; }

        /// <summary>
        /// Returns a combination of principal name and display name. If display name is not defined, the property only returns the principal name.
        /// </summary>
        internal string FormattedName
        {
            get
            {
                string fName = this.PrincipalName;
                if (!string.IsNullOrEmpty(this.DisplayName)) fName += " (" + this.DisplayName + ")";
                return fName;
            }
        }
        /// <summary>
        /// Default constructor, which loads the user info on first activation.
        /// </summary>
        private UserSlt()
        {
            this.DisplayName = UserPrincipal.Current.DisplayName;
            this.MailAddress = UserPrincipal.Current.EmailAddress;
            this.PrincipalName = Environment.UserName;
            if (this.DisplayName == null) this.DisplayName = "UNKNOWN-NAME";
            if (this.MailAddress == null) this.MailAddress = "UNKNOWN-MAIL";
            if (this.PrincipalName == null) this.PrincipalName = "UNKNOWN-USER";
            this.DisplayName = this.DisplayName.Replace("Admin Account ", "");  // Bit of a hack, but we seem to use this prefix for local admin accounts.
            Logger.WriteInfo("Framework.Util.UserSlt >> Logged-in user = '" + FormattedName + "...");
            Logger.WriteInfo("Framework.Util.UserSlt >> With mail address: " + this.MailAddress);
        }
    }
}
