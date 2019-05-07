using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Card : Payment
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
        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM}", ApplyFormatInEditMode = true)]
        public DateTime ExpiryDate { get; set; }

        [Required]
        public int CVV { get; set; }
    }

    public enum CardType
    {
        Visa,
        Mastercard,
        [Display(Name = "American Express")]
        AmericanExpress,
        Discover,
        Maestro
    }
}
