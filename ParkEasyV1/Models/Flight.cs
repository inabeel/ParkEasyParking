using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class to hold the Flight details of a Booking
    /// </summary>
    public class Flight
    {
        /// <summary>
        /// Flight ID - primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Departure Flight Number
        /// </summary>
        public string DepartureFlightNo { get; set; }

        /// <summary>
        /// Departure Time
        /// </summary>
        public TimeSpan DepartureTime { get; set; }

        /// <summary>
        /// Return Flight Number
        /// </summary>
        public string ReturnFlightNo { get; set; }

        /// <summary>
        /// Return Flight Time
        /// </summary>
        public TimeSpan ReturnFlightTime { get; set; }

        /// <summary>
        /// Departure Date
        /// </summary>
        public DateTime DepartureDate { get; set; }

        /// <summary>
        /// Return Date
        /// </summary>
        public DateTime ReturnDate { get; set; }

        /// <summary>
        /// Trip Destination Airport
        /// </summary>
        public string DestinationAirport { get; set; }


        //navigational properties

        /// <summary>
        /// Virtual list of Bookings to model One to Many relationship with Booking
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }
    }
}