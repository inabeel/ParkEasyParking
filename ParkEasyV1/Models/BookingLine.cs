using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class to hold all Booking Line information
    /// This class provides the functionality to book multiple vehicles in the system at once and resolves the Many to Many relationship with Booking and Vehicle
    /// </summary>
    public class BookingLine
    {
        /// <summary>
        /// BookingLineID - Primary Key
        /// </summary>
        [Key]
        public int ID { get; set; }

        //navigational properties

        /// <summary>
        /// Booking and BookingID associated with BookingLine - Foreign Key for Booking
        /// Models Many to One relationship with Booking
        /// </summary>
        [ForeignKey("Booking")]
        [Display(Name = "Booking")]
        public int BookingID { get; set; }
        public virtual Booking Booking { get; set; }

        /// <summary>
        /// Vehicle and VehicleID associated with BookingLine - Foreign Key for Vehicle
        /// Models Many to One relationship with Vehicle
        /// </summary>
        [ForeignKey("Vehicle")]
        [Display(Name = "Vehicle")]
        public int VehicleID { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }
}