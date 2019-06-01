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
        /// Date the booking was created
        /// </summary>
        public DateTime DateBooked { get; set; }

        /// <summary>
        /// Length of booking
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Total cost of booking
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// Enum status of booking
        /// </summary>
        public BookingStatus BookingStatus { get; set; }

        /// <summary>
        /// Valet service selected boolean
        /// </summary>
        public bool ValetService { get; set; }

        /// <summary>
        /// Attribute to determine if the booking has checked in
        /// </summary>
        public bool CheckedIn { get; set; }

        /// <summary>
        /// Attribute to determine if the booking has checked out
        /// </summary>
        public bool CheckedOut { get; set; }

        
        //Navigational Properties

        /// <summary>
        /// Virtual collection of BookingLines to model the One to Many relatonship with BookingLine
        /// </summary>
        public virtual List<BookingLine> BookingLines { get; set; }

        /// <summary>
        /// Virtual instance of Invoice to model the One to One relationship with Invoice
        /// </summary>
        public virtual Invoice Invoice { get; set; }

        /// <summary>
        /// User and UserID associated with Booking - Foreign Key for User
        /// Models Many to One relationship with User
        /// </summary>
        [ForeignKey("User")]
        [Display(Name = "User")]
        public string UserID { get; set; }
        public virtual User User { get; set; }

        /// <summary>
        /// Flight and FlightID associated with Booking - Foreign Key for Flight
        /// Models Many to One relationship with Flight
        /// </summary>
        [ForeignKey("Flight")]
        [Display(Name = "Flight")]
        public int FlightID { get; set; }
        public virtual Flight Flight { get; set; }

        /// <summary>
        /// ParkingSlot and ParkingSlotID associated with Booking - Foreign Key for ParkingSlot
        /// Models Many to One relationship with ParkingSlot
        /// </summary>
        [ForeignKey("ParkingSlot")]
        [Display(Name = "ParkingSlot")]
        public int ParkingSlotID { get; set; }
        public virtual ParkingSlot ParkingSlot { get; set; }

        /// <summary>
        /// Tariff and TariffID associated with Booking - Foreign Key for Tariff
        /// Models Many to One relationship with Tariff
        /// </summary>
        [ForeignKey("Tariff")]
        [Display(Name = "Tariff")]
        public int TariffID { get; set; }
        public virtual Tariff Tariff { get; set; }

        /// <summary>
        /// Method to create a Booking confirmation message and use the SendGrid API to email it to User
        /// </summary>
        public void EmailConfirmation()
        {
            //create the hyperlink with the booking ID that will be used in the email body
            string link = "http://localhost:44350/Bookings/Confirmation/" + ID;

            //configure SendGrid API
            var apiKey = ConfigurationManager.AppSettings["SendGridKey"];
            var client = new SendGridClient(apiKey);
            //create message
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("confirmation@parkeasy.co.uk", "ParkEasy Airport Parking"),
                Subject = "Booking Reference #" + this.ID + " Confirmation",
                PlainTextContent =
                "Hello, " + this.User.FirstName +
                "Your booking has been confirmed.",
                HtmlContent = "Hello, " + User.FirstName + "<br>" +
                "Your booking with ParkEasy Airport Parking has been confirmed." + "<br>" +
                "You can view a copy of your booking confirmation by clicking the link <a href=\"" + link + "\">here</a>"
            };
            //send the message
            msg.AddTo(new EmailAddress(User.Email));
            var response = client.SendEmailAsync(msg);
        }

        /// <summary>
        /// Method to send an SMS verification - CURRENTLY DISABLED SEE ERROR LOG   
        /// </summary>
        public void SMSConfirmation()
        {
            //check if user phone number is null
            if (User.PhoneNumber!=null)
            {
                 // Twilio Begin
                var accountSid = ConfigurationManager.AppSettings["SMSAccountIdentification"];
                var authToken = ConfigurationManager.AppSettings["SMSAccountPassword"];
                var fromNumber = ConfigurationManager.AppSettings["SMSAccountFrom"];

                //initialize twilio
                TwilioClient.Init(accountSid, authToken);

                //create and send sms message
                MessageResource result = MessageResource.Create(
                new PhoneNumber(User.PhoneNumber),
                from: new PhoneNumber(fromNumber),
                body: "Your booking with ParkEasy Airport Parking has been confirmed. Your booking number is " + ID + "."
                );
            }            
        }
    }

    /// <summary>
    /// Enumeration of the different types of Booking Status
    /// </summary>
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