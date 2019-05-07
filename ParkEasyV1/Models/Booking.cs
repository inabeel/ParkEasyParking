using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class Booking
    {
        [Key]
        public int ID { get; set; }
        public DateTime DateBooked { get; set; }
        public int Duration { get; set; }
        public double Total { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public bool ValetService { get; set; }
        public bool CheckedIn { get; set; }
        public bool CheckedOut { get; set; }

        //Navigational Properties

        //One to Many
        public virtual List<BookingLine> BookingLines { get; set; }

        //One to one
        public virtual Invoice Invoice { get; set; }

        //Many to one
        [ForeignKey("User")]
        [Display(Name = "User")]
        public string UserID { get; set; }
        public virtual User User { get; set; }

        [ForeignKey("Flight")]
        [Display(Name = "Flight")]
        public int FlightID { get; set; }
        public virtual Flight Flight { get; set; }

        [ForeignKey("ParkingSlot")]
        [Display(Name = "ParkingSlot")]
        public int ParkingSlotID { get; set; }
        public virtual ParkingSlot ParkingSlot { get; set; }

        [ForeignKey("Tariff")]
        [Display(Name = "Tariff")]
        public int TariffID { get; set; }
        public virtual Tariff Tariff { get; set; }
    }

    public enum BookingStatus
    {
        Confirmed, UnPaid, Cancelled, NoShow, Void
    }
}