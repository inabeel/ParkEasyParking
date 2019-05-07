using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Tariff
    {
        [Key]
        public int ID { get; set; }
        public string Type { get; set; }
        public double Amount { get; set; }

        //navigational properties

        //one to many
        public virtual List<Booking> Bookings { get; set; }
    }
}