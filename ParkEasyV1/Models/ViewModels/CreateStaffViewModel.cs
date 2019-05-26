using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    /// <summary>
    /// view model to hold all information required for creating staff member
    /// </summary>
    public class CreateStaffViewModel
    {
        /// <summary>
        /// Staff email address
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// Staff firstname
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Staff lastname
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// Staff address line 1
        /// </summary>
        [Required]
        [Display(Name ="Address Line 1")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// Staff address line 2
        /// </summary>
        [Required]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// Staff city
        /// </summary>
        [Required]
        public string City { get; set; }

        /// <summary>
        /// Postcode of Staff
        /// </summary>
        [Required]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Post Code")]
        public string Postcode { get; set; }

        /// <summary>
        /// job title of staff member
        /// </summary>
        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        
        /// <summary>
        /// current qualification held by staff member
        /// </summary>
        [Display(Name = "Current Qualification")]
        public string CurrentQualification { get; set; }

        /// <summary>
        /// name of staff emergency contact
        /// </summary>
        [Required]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; }

        /// <summary>
        /// phone number of staff emergency contact
        /// </summary>
        [Required]
        [Display(Name = "Emergency Contact Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string EmergencyContactPhoneNo { get; set; }

        /// <summary>
        /// Staff password
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Compare("PasswordConfirm")]
        public string Password { get; set; }

        /// <summary>
        /// Staff password confirmed
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string PasswordConfirm { get; set; }
    }
}