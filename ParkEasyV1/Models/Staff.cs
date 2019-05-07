using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Staff : User
    {
        public string JobTitle { get; set; }
        public string CurrentQualification { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhoneNo { get; set; }
    }
}