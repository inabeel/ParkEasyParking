using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Subclass of User to model Staff Members
    /// </summary>
    public class Staff : User
    {
        /// <summary>
        /// Job Title
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// Current qualification held
        /// </summary>
        public string CurrentQualification { get; set; }

        /// <summary>
        /// Name of emergency contact
        /// </summary>
        public string EmergencyContactName { get; set; }

        /// <summary>
        /// Phone number of emergency contact
        /// </summary>
        public string EmergencyContactPhoneNo { get; set; }
    }
}