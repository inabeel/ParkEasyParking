using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Flight
    {
        [Key]
        public int ID { get; set; }
        public string DepartureFlightNo { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public string ReturnFlightNo { get; set; }
        public TimeSpan ReturnFlightTime { get; set; }
        public DateTime DepartureDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public string DestinationAirport { get; set; }

        //navigational properties

        //one to many
        public virtual List<Booking> Bookings { get; set; }
    }
}