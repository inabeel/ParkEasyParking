using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    /// <summary>
    /// view model for contact us information
    /// </summary>
    public class ContactViewModel
    {
        /// <summary>
        /// name of contact
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// email of contact
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// message from contact
        /// </summary>
        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }
    }
}