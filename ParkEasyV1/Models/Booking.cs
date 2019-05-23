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
using Itenso.TimePeriod;

namespace ParkEasyV1.Models
{
    public class Booking
    {
        [Key]
        public int ID { get; set; }
        public DateTime DateBooked { get; set; }
        public DateTime BookingStart { get; set; }
        public DateTime BookingEnd { get; set; }
        public int Duration { get; set; }
        public double Total { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public bool ValetService { get; set; }
        public bool CheckedIn { get; set; }
        public bool CheckedOut { get; set; }

        [NotMapped]
        public TimeRange TimeRange { get; set; }

        public Booking()
        {
            TimeRange = new TimeRange(
                new DateTime(BookingStart.Year, BookingStart.Month, BookingStart.Day, 0, 0 ,0),
                new DateTime(BookingEnd.Year, BookingEnd.Month, BookingEnd.Day, 0, 0, 0));
        }

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

        public void EmailConfirmation()
        {
            var apiKey = ConfigurationManager.AppSettings["SendGridKey"];
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("confirmation@parkeasy.co.uk", "ParkEasy Airport Parking"),
                Subject = "Booking Reference #" + this.ID + " Confirmation",
                PlainTextContent = 
                "Hello, " + this.User.FirstName +
                "Your booking has been confirmed.",
                HtmlContent = "Hello, " + User.FirstName + "<br>" +
                "Your booking with ParkEasy Airport Parking has been confirmed." + "<br>" +
                "You can view a copy of your booking confirmation by clicking the link <a href=localhost:44350/Bookings/Confirmation/" + ID + ">here</a>"
            };
            msg.AddTo(new EmailAddress(User.Email));
            var response = client.SendEmailAsync(msg);
        }
    }

    public enum BookingStatus
    {
        Confirmed, Unpaid, Cancelled, NoShow, Void, Delayed
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