using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Invoice
    {
        [ForeignKey("Booking")]
        public int ID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string CustomerID { get; set; }
        public string BookingID { get; set; }
        public double AmountDue { get; set; }

        //navigational properties

        //one to one
        [Required]
        public virtual Booking Booking { get; set; }
    }
}