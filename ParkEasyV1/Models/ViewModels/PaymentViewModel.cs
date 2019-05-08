using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    public class PaymentViewModel
    {
        [Required]
        [Display(Name = "Card Type")]
        public CardType Type { get; set; }

        [Required]
        [DataType(DataType.CreditCard)]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Required]
        [Display(Name = "Name on Card")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string NameOnCard { get; set; }

        [Required]
        [Display(Name ="Expiry Month")]
        public int ExpiryMonth { get; set; }

        [Required]
        [Display(Name ="Expiry Year")]
        public int ExpiryYear { get; set; }

        [Required]
        public int CVV { get; set; }
    }
}