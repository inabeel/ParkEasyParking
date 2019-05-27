using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Subclass of Payment to hold details for external payments such as Stripe API or PayPal
    /// </summary>
    public class ExternalPayment : Payment
    {
        /// <summary>
        /// External payment transaction ID
        /// </summary>
        public string TransactionID { get; set; }
    }
}