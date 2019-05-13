using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class StripePayment : Payment
    {
        public string TransactionID { get; set; }
    }
}