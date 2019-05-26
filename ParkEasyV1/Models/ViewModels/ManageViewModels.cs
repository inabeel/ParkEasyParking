using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Manage index view model
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>
        /// boolean for has password
        /// </summary>
        public bool HasPassword { get; set; }

        /// <summary>
        /// IList of logins
        /// </summary>
        public IList<UserLoginInfo> Logins { get; set; }

        /// <summary>
        /// user phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// boolean for enable/disable two factor auth
        /// </summary>
        public bool TwoFactor { get; set; }

        /// <summary>
        /// boolean for remember browser
        /// </summary>
        public bool BrowserRemembered { get; set; }
    }

    /// <summary>
    /// view model for managing logins
    /// </summary>
    public class ManageLoginsViewModel
    {
        /// <summary>
        /// IList of current logins
        /// </summary>
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        /// <summary>
        /// IList of other logins
        /// </summary>
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    /// <summary>
    /// set password view model
    /// </summary>
    public class SetPasswordViewModel
    {
        /// <summary>
        /// new password
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// new password confirmed
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// view model for changing password
    /// </summary>
    public class ChangePasswordViewModel
    {
        /// <summary>
        /// old password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        /// <summary>
        /// new password
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// new password confirmed
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// view model for adding phone number
    /// </summary>
    public class AddPhoneNumberViewModel
    {
        /// <summary>
        /// new phone number to add
        /// </summary>
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    /// <summary>
    /// view model to verify phone number
    /// </summary>
    public class VerifyPhoneNumberViewModel
    {
        /// <summary>
        /// sms verification code
        /// </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// new phone number
        /// </summary>
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// view model to configure two factor auth
    /// </summary>
    public class ConfigureTwoFactorViewModel
    {
        /// <summary>
        /// selected provider
        /// </summary>
        public string SelectedProvider { get; set; }

        /// <summary>
        /// collection of providers
        /// </summary>
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}