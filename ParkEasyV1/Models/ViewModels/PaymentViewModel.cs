using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    /// <summary>
    /// view model to hold card payment details
    /// </summary>
    public class PaymentViewModel
    {
        /// <summary>
        /// enum for type of card
        /// </summary>
        [Required]
        [Display(Name = "Card Type")]
        public CardType Type { get; set; }

        /// <summary>
        /// card number
        /// </summary>
        [Required]
        [DataType(DataType.CreditCard)]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        /// <summary>
        /// name on card
        /// </summary>
        [Required]
        [Display(Name = "Name on Card")]
        [StringLength(32, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        public string NameOnCard { get; set; }

        /// <summary>
        /// card expiry date
        /// </summary>
        [Required]
        [Display(Name ="Expiry Month")]
        public int ExpiryMonth { get; set; }

        /// <summary>
        /// card expiry year
        /// </summary>
        [Required]
        [Display(Name ="Expiry Year")]
        public int ExpiryYear { get; set; }

        /// <summary>
        /// card CVV/CSV code
        /// </summary>
        [Required]
        public int CVV { get; set; }
    }
}