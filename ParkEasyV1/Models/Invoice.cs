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
    public class Invoice
    {
        [ForeignKey("Booking")]
        public int ID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public double AmountDue { get; set; }
        public InvoiceStatus? Status { get; set; }

        //navigational properties

        //one to one
        [Required]
        public virtual Booking Booking { get; set; }

        public void EmailInvoice()
        {
            string link = "http://localhost:44350/Invoice/View/" + ID;

            var apiKey = ConfigurationManager.AppSettings["SendGridKey"];
            var client = new SendGridClient(apiKey);
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
            msg.AddTo(new EmailAddress(Booking.User.Email));
            var response = client.SendEmailAsync(msg);
        }
    }

    public enum InvoiceStatus
    {
        Sent, Paid, Overdue, Void, WriteOff
    }
}