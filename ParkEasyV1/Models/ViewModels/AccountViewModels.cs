using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// View Model to hold information gathered during external login confirmation stage
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        /// <summary>
        /// User first name
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// User last name
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string Surname { get; set; }

        /// <summary>
        /// User address line 1
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [Display(Name = "Address")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// User address line 2 - NOT REQUIRED
        /// </summary>
        [Display(Name = "")]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// User City
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string City { get; set; }

        /// <summary>
        /// User Postcode
        /// </summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
        [DataType(DataType.PostalCode)]
        public string Postcode { get; set; }

        /// <summary>
        /// User Email
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// Terms and Conditions Acceptance - Must be True validation
        /// </summary>
        [Required]
        [MustBeTrue(ErrorMessage = "You must agree to the ParkEasy Terms & Conditions")]
        public bool TermsConditions { get; set; }

    }

    /// <summary>
    /// ViewModel for external login list
    /// </summary>
    public class ExternalLoginListViewModel
    {
        /// <summary>
        /// Return url
        /// </summary>
        public string ReturnUrl { get; set; }
    }

    /// <summary>
    /// View model for SMS send code
    /// </summary>
    public class SendCodeViewModel
    {
        /// <summary>
        /// selected sms provider
        /// </summary>
        public string SelectedProvider { get; set; }

        /// <summary>
        /// collection of sms providers
        /// </summary>
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }

        /// <summary>
        /// return url
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// boolean to remember session
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// View model for SMS verification code
    /// </summary>
    public class VerifyCodeViewModel
    {
        /// <summary>
        /// sms provider
        /// </summary>
        [Required]
        public string Provider { get; set; }

        /// <summary>
        /// sms verification code
        /// </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// return url
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// boolean to remember browser session
        /// </summary>
        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        /// <summary>
        /// boolean to remember session
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// View model for forgotton password
    /// </summary>
    public class ForgotViewModel
    {
        /// <summary>
        /// user email
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    /// <summary>
    /// View model for logging in
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// user email
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// user password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// boolean to remember session
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// View model for account registration
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// user first name
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// user last name
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string Surname { get; set; }

        /// <summary>
        /// user address line 1
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [Display(Name = "Address")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// user address line 2 - NOT REQUIRED
        /// </summary>
        [Display(Name = "")]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// user city
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string City { get; set; }

        /// <summary>
        /// user postcode
        /// </summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
        [DataType(DataType.PostalCode)]
        public string Postcode { get; set; }

        /// <summary>
        /// user email
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// user password
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// user password confirmation
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// user terms and conditions acceptance
        /// </summary>
        [Required]
        [MustBeTrue(ErrorMessage = "You must agree to the ParkEasy Terms & Conditions")]
        public bool TermsConditions { get; set; }

        /// <summary>
        /// user corporate customer declaration
        /// </summary>
        [Display(Name ="I am a corporate client wishing to open an account")]
        public bool Corporate { get; set; }
    }

    /// <summary>
    /// View model for resetting password
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// user email
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// user password
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// user password confirmation
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// password reset code
        /// </summary>
        public string Code { get; set; }
    }

    /// <summary>
    /// View model for forgot password
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// user email
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }


    /// <summary>
    /// Validation attribute that requires a boolean attribute to be set to TRUE
    /// </summary>
    public class MustBeTrueAttribute : ValidationAttribute
    {
        /// <summary>
        /// override isValid method to validate if attribute is true
        /// </summary>
        /// <param name="value">attribute value</param>
        /// <returns>true/false if valid</returns>
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
    }

    /// <summary>
    /// Validation attribute used for ensuring the booking end date is not before the booking start
    /// </summary>
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        /// <summary>
        /// Class to check if date is greater than another
        /// </summary>
        /// <param name="dateToCompareToFieldName">field to compare date with</param>
        public DateGreaterThanAttribute(string dateToCompareToFieldName)
        {
            DateToCompareToFieldName = dateToCompareToFieldName;
        }

        /// <summary>
        /// attribute to hold comparision field name
        /// </summary>
        private string DateToCompareToFieldName { get; set; }

        /// <summary>
        /// override isValid function to check if the attribute is greater than the specified field name
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //check if attribute value is null
            if (value == null)
            {
                //return validation result error
                return new ValidationResult(FormatErrorMessage("Error"));
            }

            DateTime earlierDate = (DateTime)value;

            DateTime laterDate = (DateTime)validationContext.ObjectType.GetProperty(DateToCompareToFieldName).GetValue(validationContext.ObjectInstance, null);

            if (laterDate > earlierDate)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Booking Return Date Can't Be Earlier Than Departure Date");
            }
        }
    }


    /// <summary>
    /// Validation attribute to validate that the booking departure date is not in the past or equal to today's date
    /// </summary>
    public class DateMoreThanOrEqualToToday : ValidationAttribute
    {
        /// <summary>
        /// override error message
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override string FormatErrorMessage(string name)
        {
            return "Departure date must be a future date";
        }

        /// <summary>
        /// override isvalid to perform validation
        /// </summary>
        /// <param name="objValue">attribute value</param>
        /// <param name="validationContext">validation context</param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object objValue,
                                                       ValidationContext validationContext)
        {
            //check if attribute value is null
            if (objValue==null)
            {
                //return validation error
                return new ValidationResult(FormatErrorMessage("Error"));
            }

            var dateValue = objValue as DateTime? ?? new DateTime();

            //alter this as needed. I am doing the date comparison if the value is not null

            if (dateValue.Date < DateTime.Now.Date)
            {
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }
            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// View model for manage details display
    /// </summary>
    public class ManageDetailsViewModel
    {
        /// <summary>
        /// user id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// user first name
        /// </summary>
        [Required]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// user last name
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// user email
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// user phone number
        /// </summary>
        [Display(Name = "Phone Number")]
        public string PhoneNo { get; set; }

        /// <summary>
        /// user address line 1
        /// </summary>
        [Required]
        [Display(Name = "Address")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// user address line 2 - NOT REQUIRED
        /// </summary>
        [Display(Name = "")]
        public string AddressLine2 { get; set; }
        
        /// <summary>
        /// user city
        /// </summary>
        [Required]
        public string City { get; set; }

        /// <summary>
        /// user postcode
        /// </summary>
        [Required]
        [DataType(DataType.PostalCode)]
        public string Postcode { get; set; }

        /// <summary>
        /// user boolean for two factor enabled/disabled
        /// </summary>
        public bool TwoFactor { get; set; }
    }
}
