using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class BookingLine
    {
        [Key]
        public int ID { get; set; }

        //navigational properties

        //many to one
        [ForeignKey("Booking")]
        [Display(Name = "Booking")]
        public int BookingID { get; set; }
        public virtual Booking Booking { get; set; }

        [ForeignKey("Vehicle")]
        [Display(Name = "Vehicle")]
        public int VehicleID { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }
}