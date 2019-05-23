using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ParkEasyV1.Models;
using ParkEasyV1.Models.ViewModels;
using Rotativa;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller to handle all Invoice events and actions
    /// </summary>
    public class InvoiceController : Controller
    {
        /// <summary>
        /// Global variable to store instance of the ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult to return the invoice index view
        /// </summary>
        /// <returns>Index view</returns>
        public ActionResult Index()
        {
            // create new instance of the user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            // get the current logged in user using the user manager
            User loggedInUser = userManager.FindByEmail(User.Identity.GetUserName());

            // store the user id in the viewbag to be used in the view
            ViewBag.UserID = loggedInUser.Id;

            // Create a new list of bookings
            List<Booking> bookings = new List<Booking>();

            // loop through all bookings stored in the database Bookings table
            foreach (var booking in db.Bookings.ToList())
            {
                // parse the User associated with the booking to a Customer
                Customer customer = booking.User as Customer;

                // check if the Customer is a corporate customer
                if (customer.Corporate)
                {
                    // add booking to the bookings list
                    bookings.Add(booking);
                }
            }
           
            //return the index view with the list of corporate bookings
            return View(bookings);
        }

        /// <summary>
        /// HttpGet ActionResult for returning the the View Invoice page
        /// </summary>
        /// <param name="id">invoice id</param>
        /// <returns>view invoice screen</returns>
        public ActionResult View(int? id)
        {
            //find the booking using id parameter
            Booking booking = db.Bookings.Find(id);

            // check if booking has an invoice associated with it
            if (booking.Invoice==null)
            {
                // return HttpNotFound error if no invoice is associated with booking
                return HttpNotFound();
            }

            //calculate the VAT of the booking (find 20% of booking total)
            double vat = Math.Round(booking.Total / 10 * 2, 2);

            //store the subtotal of the booking (booking without vat) in a viewbag
            ViewBag.Subtotal = booking.Total - vat;
            //store the vat total in a viewbag
            ViewBag.Vat = vat;

            //return the view with booking
            return View(booking);
        }

        /// <summary>
        /// ActionResult to convert Invoice View to PDF
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns>PDF file of invoice view</returns>
        public ActionResult PrintViewToPdf(int? bookingId)
        {
            //use Rotativa extension to get the View Invoice page as a PDF and return PDF
            var pdf = new ActionAsPdf("View", new {id=bookingId});
            return pdf;
        }
        

        /// <summary>
        /// HttpGet User Invoice Payment Confirmation
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>invoice payment confirmation view</returns>
        public ActionResult Confirmation(int id)
        {
            //find booking using the id parameter
            Booking booking = db.Bookings.Find(id);
            //check if the booking is null
            if (booking == null)
            {
                //return HttpNotFound error
                return HttpNotFound();
            }

            //initialize variable to hold vehicle id
            int vehicleID = 0;

            //loop through booking lines table in database
            foreach (var line in db.BookingLines.ToList())
            {
                //if the booking id foreign key in bookingline matches the current booking id
                if (line.BookingID == id)
                {
                    //store the vehicle id associated with the booking line
                    vehicleID = line.VehicleID;
                }
            }

            //get the vehicle associated with booking from Vehicles table using vehicle id
            Vehicle vehicle = db.Vehicles.Find(vehicleID);

            //create a new ViewBookingViewModel and input data from the booking
            ViewBookingViewModel model = new ViewBookingViewModel
            {
                BookingID = booking.ID,
                DepartureDate = booking.Flight.DepartureDate,
                DepartureTime = booking.Flight.DepartureTime,
                ReturnDate = booking.Flight.ReturnDate,
                ReturnTime = booking.Flight.ReturnFlightTime,
                Duration = booking.Duration,
                Total = booking.Total,
                Valet = booking.ValetService,
                FirstName = booking.User.FirstName,
                Surname = booking.User.LastName,
                AddressLine1 = booking.User.AddressLine1,
                AddressLine2 = booking.User.AddressLine2,
                City = booking.User.City,
                Postcode = booking.User.Postcode,
                Email = booking.User.Email,
                PhoneNo = booking.User.PhoneNumber,
                VehicleMake = vehicle.Make,
                VehicleModel = vehicle.Model,
                VehicleColour = vehicle.Colour,
                VehicleRegistration = vehicle.RegistrationNumber,
                NoOfPassengers = vehicle.NoOfPassengers
            };

            //return the view with the view model
            return View(model);
        }

        /// <summary>
        /// ActionResult to convert Invoice Payment Confirmation to PDF
        /// </summary>
        /// <param name="bookingId">booking id</param>
        /// <returns>PDF of invoice payment view</returns>
        public ActionResult PrintConfirmationToPdf(int? bookingId)
        {
            //Use Rotativa extension to get the Confirmation view as a PDF file and return
            var pdf = new ActionAsPdf("Confirmation", new { id = bookingId });
            return pdf;
        }

        // GET: Invoice/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // GET: Invoice/Create
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.Bookings, "ID", "UserID");
            return View();
        }

        /// <summary>
        /// ActionResult for generating an invoice for corporate customer bookings
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Invoice Index</returns>
        public ActionResult Generate(int id)
        {
            //if invoice is successfully generated
            if (GenerateInvoice(id))
            {
                //return Index view with success message
                TempData["Success"] = "Invoice Successfully Generated";
                return RedirectToAction("Index");
            }
            else
            {
                //return Index view with error message
                TempData["Error"] = "Unable to Generate Invoice";
                return RedirectToAction("Index");
            }          
        }

        /// <summary>
        /// ActionResult to Write Off an unpaid expired invoice
        /// </summary>
        /// <param name="id">invoice id</param>
        /// <returns>Invoice Index view</returns>
        public ActionResult WriteOff(int id)
        {
            try
            {
                //find the invoice using the id parameter and set invoice status to write off
                db.Invoices.Find(id).Status = InvoiceStatus.WriteOff;
                db.SaveChanges();   //save changes to database
                TempData["Success"] = "Invoice Successfully Written Off";   //update success message
                return RedirectToAction("Index");   //return index view
            }
            catch (Exception ex)
            {
                //update error message and return index view
                TempData["Error"] = "Invoice Could Not Be Written Off";
                return RedirectToAction("Index");
            }
        }

        // POST: Invoice/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,InvoiceDate,CustomerID,BookingID,AmountDue")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Invoices.Add(invoice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID = new SelectList(db.Bookings, "ID", "UserID", invoice.ID);
            return View(invoice);
        }

        // GET: Invoice/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID = new SelectList(db.Bookings, "ID", "UserID", invoice.ID);
            return View(invoice);
        }

        // POST: Invoice/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,InvoiceDate,CustomerID,BookingID,AmountDue")] Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID = new SelectList(db.Bookings, "ID", "UserID", invoice.ID);
            return View(invoice);
        }

        // GET: Invoice/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Invoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);
            db.Invoices.Remove(invoice);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Function to generate an invoice for a customer booking
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>true/false boolean</returns>
        private bool GenerateInvoice(int id)
        {
            try
            {
                //find booking in database using the id parameter
                Booking booking = db.Bookings.Find(id);

                //create a new invoice in the Invoices database table associated with the current booking
                db.Invoices.Add(new Invoice()
                {
                    Booking = booking,  //associate invoice with current booking
                    InvoiceDate = DateTime.Now, //set invoice date to current datetime
                    ExpiryDate = DateTime.Now.AddDays(30),  //invoice will expire within 30 days
                    AmountDue = booking.Total,  //set invoice amount due to booking total
                    Status = InvoiceStatus.Sent //update invoice status to sent
                });

                //save changes to the database
                db.SaveChanges();

                //call method to email copy of invoice to customer
                booking.Invoice.EmailInvoice();

                //return true
                return true;
            }
            catch (Exception ex)
            {
                //if exception occurs return false
                return false;
            }
            
        }

        /// <summary>
        /// Method for disposing of unused resources
        /// </summary>
        /// <param name="disposing">boolean for disposing</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
