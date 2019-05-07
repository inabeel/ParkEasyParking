using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Vehicle
    {
        [Key]
        public int ID { get; set; }
        public string RegistrationNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Colour { get; set; }
        public int NoOfPassengers { get; set; }

        //navigational properties

        //one to many
        public virtual List<BookingLine> BookingLines { get; set; }
    }
}