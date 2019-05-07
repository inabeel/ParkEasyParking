using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Payment
    {
        [Key]
        public int ID { get; set; }
        public DateTime PaymentDate { get; set; }
        public double Amount { get; set; }

        //navigational properties

        //many to one
        [ForeignKey("User")]
        [Display(Name = "User")]
        public string UserID { get; set; }
        public virtual User User { get; set; }
    }
}