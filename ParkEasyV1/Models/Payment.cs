using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class to hold details about Payment
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// PaymentID - Primary Key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Date of payment
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public double Amount { get; set; }

        //navigational properties

        /// <summary>
        /// User and UserID to model Many to One relationship with User
        /// Foreign Key for User
        /// </summary>
        [ForeignKey("User")]
        [Display(Name = "User")]
        public string UserID { get; set; }
        public virtual User User { get; set; }
    }
}