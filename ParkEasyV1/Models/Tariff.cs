using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class to model a Tariff charge
    /// </summary>
    public class Tariff
    {
        /// <summary>
        /// Tariff ID - primary key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Type of tariff (Tariff Name)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Tariff Amount
        /// </summary>
        public double Amount { get; set; }

        //navigational properties

        /// <summary>
        /// Virtual collection of Bookings to model One to Many relationship with Booking
        /// </summary>
        public virtual List<Booking> Bookings { get; set; }
    }
}