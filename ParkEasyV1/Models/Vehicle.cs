using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class to hold Vehicle information
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Vehicle ID - primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// vehicle registration number
        /// </summary>
        public string RegistrationNumber { get; set; }

        /// <summary>
        /// vehicle make
        /// </summary>
        public string Make { get; set; }

        /// <summary>
        /// vehicle model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// vehicle colour
        /// </summary>
        public string Colour { get; set; }

        /// <summary>
        /// number of passengers travelling
        /// </summary>
        public int NoOfPassengers { get; set; }

        //navigational properties

        /// <summary>
        /// Virtual collection of BookingLines to model One to Many relationship with BookingLine
        /// </summary>
        public virtual List<BookingLine> BookingLines { get; set; }
    }
}