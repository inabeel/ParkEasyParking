using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ParkEasyV1.Models.ViewModels
{
    /// <summary>
    /// view model to hold change role attributes
    /// </summary>
    public class ChangeRoleViewModel
    {
        /// <summary>
        /// Username of user
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// old role of the user
        /// </summary>
        public string OldRole { get; set; }

        /// <summary>
        /// New role of the user
        /// </summary>
        [Required, Display(Name = "Role")]
        public string Role { get; set; }

        /// <summary>
        /// Collection of all roles
        /// </summary>
        public ICollection<SelectListItem> Roles { get; set; }
    }
}