using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Mail;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Twilio.Clients;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class to hold Booking information
    /// </summary>
    public class Booking
    {
        /// <summary>
        /// Booking ID - Table Primary Key
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Employee First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Employee Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// CF 10 Staff Number: A unique identifier for employee
        /// </summary>
        public string EmployeeID { get; set; }

        public ReservationType ReservationType { get; set; }

        /// <summary>
        /// Date the booking was created
        /// </summary>
        public DateTime DateBooked { get; set; }

        /// <summary>
        /// Date the booking end
        /// </summary>
        public DateTime DateBookingEnd { get; set; }

        /// <summary>
        /// ParkingSlot and ParkingSlotID associated with Booking - Foreign Key for ParkingSlot
        /// Models Many to One relationship with ParkingSlot
        /// </summary>
        [ForeignKey("ParkingSlot")]
        [Display(Name = "ParkingSlot")]
        public int ParkingSlotID { get; set; }
        public virtual ParkingSlot ParkingSlot { get; set; }

        /// <summary>
        /// Vehicle and VehicleID associated with BookingLine - Foreign Key for Vehicle
        /// Models Many to One relationship with Vehicle
        /// </summary>
        [ForeignKey("Vehicle")]
        [Display(Name = "Vehicle")]
        public int VehicleID { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }

    /// <summary>
    /// Enumeration of the different types of Booking Status
    /// </summary>
    public enum BookingStatus
    {
        Confirmed, Unpaid, Cancelled, NoShow, Void, Delayed
    }

    public enum ReservationType
    {
        Permanent,
        DateRange
    }

    /// <summary>
    /// Class for validating the Google reCaptcha API Response in Create Booking
    /// </summary>
    public class CaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success
        {
            get;
            set;
        }
        [JsonProperty("error-codes")]
        public List<string> ErrorMessage
        {
            get;
            set;
        }
    }
}