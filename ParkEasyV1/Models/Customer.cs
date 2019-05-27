using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Subclass of User to hold Customer details
    /// </summary>
    public class Customer : User
    {
        /// <summary>
        /// Date of registration
        /// </summary>
        public DateTime? RegistrationDate { get; set; }

        /// <summary>
        /// Boolean to determine whether a Customer is a corporate customer or not
        /// </summary>
        public bool Corporate { get; set; }
    }
}