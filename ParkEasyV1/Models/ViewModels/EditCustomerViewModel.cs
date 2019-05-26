using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    /// <summary>
    /// view model for all details displayed/edited during edit customer process
    /// </summary>
    public class EditCustomerViewModel
    {
        /// <summary>
        /// username of user
        /// </summary>
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        /// <summary>
        /// email address of user
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        /// <summary>
        /// email confirmed boolean
        /// </summary>
        [Display(Name = "Confirm Email")]
        public bool EmailConfirmed { get; set; }

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
        /// boolean for if customer is corporate or not
        /// </summary>
        [Required]
        [Display(Name ="Corporate Customer?")]
        public bool Corporate { get; set; }

    }
}