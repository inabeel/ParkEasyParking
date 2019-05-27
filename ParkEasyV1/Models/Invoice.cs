using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class for holding Invoice details
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// Invoice ID - Foreign Key for Booking
        /// </summary>
        [ForeignKey("Booking")]
        public int ID { get; set; }

        /// <summary>
        /// Date of Invoice Issue
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        /// Date of Invoice Expiry Date (30 days from Invoice Issue)
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Amount Due
        /// </summary>
        public double AmountDue { get; set; }

        /// <summary>
        /// Enum for Status of Invoice
        /// </summary>
        public InvoiceStatus? Status { get; set; }


        //navigational properties

        /// <summary>
        /// Virtual instance of Booking to model the One to One relationship with Booking
        /// Required Data Annotation as an Invoice cannot exist without a Booking
        /// </summary>
        [Required]
        public virtual Booking Booking { get; set; }

        /// <summary>
        /// Method to email User upon Invoice generation
        /// </summary>
        public void EmailInvoice()
        {
            //create email body hyperlink
            string link = "http://localhost:44350/Invoice/View/" + ID;

            //configure SendGrid API
            var apiKey = ConfigurationManager.AppSettings["SendGridKey"];
            var client = new SendGridClient(apiKey);
            //create email message
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("invoice@parkeasy.co.uk", "ParkEasy Airport Parking"),
                Subject = "Booking Reference #" + Booking.ID + " Invoice",
                PlainTextContent =
                "Hello, " + Booking.User.FirstName +
                "Your invoice for booking reference #" + Booking.ID + " has been processed.",
                HtmlContent = "Hello, " + Booking.User.FirstName + "<br>" +
                "Your invoice for booking reference #" + Booking.ID + " has been processed.<br>" +
                "This invoice will expire within 30 days of the sending of this email." +
                "You can view a copy of your invoice by clicking the link <a href=\"" + link + "\">here</a>"
            };
            //send email
            msg.AddTo(new EmailAddress(Booking.User.Email));
            var response = client.SendEmailAsync(msg);
        }
    }

    /// <summary>
    /// Enumeration for different types of Invoice status
    /// </summary>
    public enum InvoiceStatus
    {
        Sent, Paid, Overdue, Void, WriteOff
    }
}