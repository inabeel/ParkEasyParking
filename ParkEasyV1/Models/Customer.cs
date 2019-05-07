using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Customer : User
    {
        public DateTime RegistrationDate { get; set; }
        public bool Corporate { get; set; }
    }
}