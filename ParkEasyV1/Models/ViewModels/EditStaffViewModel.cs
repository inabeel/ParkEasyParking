using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    /// <summary>
    /// view model for holding all details required to display and edit staff members
    /// </summary>
    public class EditStaffViewModel
    {
        /// <summary>
        /// User's username
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        /// <summary>
        /// Confirm email
        /// </summary>
        [Display(Name = "Confirm Email")]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// user email address
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// user firstname
        /// </summary>
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        /// <summary>
        /// user lastname
        /// </summary>
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        /// <summary>
        /// user address line 1
        /// </summary>
        [Required]
        [Display(Name ="Address")]
        public string AddressLine1 { get; set; }

        /// <summary>
        /// user address line 2
        /// </summary>
        [Required]
        [Display(Name ="")]
        public string AddressLine2 { get; set; }

        /// <summary>
        /// user's city
        /// </summary>
        [Required]
        public string City { get; set; }

        /// <summary>
        /// postcode of user
        /// </summary>
        [Required]
        [DataType(DataType.PostalCode)]
        [Display(Name = "Post Code")]
        public string Postcode { get; set; }

        /// <summary>
        /// job title of staff
        /// </summary>
        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        /// <summary>
        /// current qualification held
        /// </summary>
        [Display(Name = "Current Qualification")]
        public string CurrentQualification { get; set; }

        /// <summary>
        /// name of emergency contact
        /// </summary>
        [Required]
        [Display(Name = "Emergency Contact Name")]
        public string EmergencyContactName { get; set; }

        /// <summary>
        /// phone number of emergency contact
        /// </summary>
        [Required]
        [Display(Name = "Emergency Contact Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string EmergencyContactPhoneNo { get; set; }
    }
}