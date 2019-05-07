using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class ParkingSlot
    {
        [Key]
        public int ID { get; set; }
        public Status Status { get; set; }

        //navigational properties

        //one to many
        public virtual List<Booking> Bookings { get; set; }
    }

    public enum Status
    {
        Reserved, Occupied, Available
    }
}