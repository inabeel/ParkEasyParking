using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class to hold ParkingSlot details
    /// </summary>
    public class ParkingSlot
    {
        /// <summary>
        /// ParkingSlot ID - primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Enum for ParkingSlot status
        /// </summary>
        public Status Status { get; set; }

        //navigational properties

        /// <summary>
        /// Virtual collection of Bookings to model One to Many relationship with Booking
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }
    }

    /// <summary>
    /// Enumeration of the different types of Parking Slot
    /// </summary>
    public enum Status
    {
        Reserved, Occupied, Available
    }
}