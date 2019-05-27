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
    /// Controller for handling all Invoice events and actions
    /// </summary>
    public class InvoiceController : Controller
    {
        /// <summary>
        /// global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult for returning the Invoice index view
        /// </summary>
        /// <returns>Invoice Index view</returns>
        // GET: Invoice
        [Authorize(Roles = "Admin, Manager, Invoice Clerk")]
        public ActionResult Index()
        {
            //create instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //find the current logged in user via email
            User loggedInUser = userManager.FindByEmail(User.Identity.GetUserName());

            //store user id in viewbag for front-end display
            ViewBag.UserID = loggedInUser.Id;

            //create a list of bookings
            List<Booking> bookings = new List<Booking>();

            //loop through bookings in database
            foreach (var booking in db.Bookings.ToList())
            {
                //parse booking user to Customer
                Customer customer = booking.User as Customer;

                //if customer is a corporate customer
                if (customer.Corporate)
                {
                    //add the booking to booking list
                    bookings.Add(booking);
                }
            }
           //return view with corporate customer bookings
            return View(bookings);
        }

        /// <summary>
        /// HttpGet ActionResult for returning the View Invoice view - to display online invoice
        /// </summary>
        /// <param name="id">Booking id</param>
        /// <returns>View Invoice view</returns>
        // GET: Invoice/View
        public ActionResult View(int? id)
        {
            try
            {
                //find the booking via id
                Booking booking = db.Bookings.Find(id);

                //if the booking invoice is null
                if (booking.Invoice==null)
                {
                    //return httpnotfound error
                    return HttpNotFound();
                }

                //create variable to hold the booking VAT (20% of booking total) amount
                double vat = Math.Round(booking.Total / 10 * 2, 2);

                //store subtotal (booking total without VAT) and vat in viewbag for front-end display
                ViewBag.Subtotal = booking.Total - vat;
                ViewBag.Vat = vat;

                //return View with booking
                return View(booking);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
        }

        /// <summary>
        /// ActionResult to convert Invoice View to PDF
        /// </summary>
        /// <param name="bookingId">booking id</param>
        /// <returns>View Invoice in PDF format</returns>
        public ActionResult PrintViewToPdf(int? bookingId)
        {
            try
            {
                //convert View Invoice to PDF and return
                var pdf = new ActionAsPdf("View", new {id=bookingId});
                return pdf;
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }            
        }
        

        /// <summary>
        /// HttpGet ActionResult to return a booking confirmation for payment via invoice
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Confirmation view</returns>
        public ActionResult Confirmation(int id)
        {
            try
            {
                //find booking via id
                Booking booking = db.Bookings.Find(id);
                //if booking is null
                if (booking == null)
                {
                    //return httpnotfound eror
                    return HttpNotFound();
                }

                //get the vehicle associated with booking via booking line
                Vehicle vehicle = db.Vehicles.Find(booking.BookingLines.First().VehicleID);

                //create new view booking view model and populate with flight/booking/vehicle data
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

                //return confirmation view with model
                return View(model);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// ActionResult to convert Invoice Payment Confirmation to PDF
        /// </summary>
        /// <param name="bookingId">booking id</param>
        /// <returns>Confirmation view as PDF</returns>
        public ActionResult PrintConfirmationToPdf(int? bookingId)
        {
            try
            {
                //convert Confirmation view to PDF and return
                var pdf = new ActionAsPdf("Confirmation", new { id = bookingId });
                return pdf;
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the invoice details view
        /// </summary>
        /// <param name="id">invoice id</param>
        /// <returns>Details view</returns>
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

        /// <summary>
        /// HttpGet ActionResult to return the create invoice view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: Invoice/Create
        public ActionResult Create()
        {
            ViewBag.ID = new SelectList(db.Bookings, "ID", "UserID");
            return View();
        }

        /// <summary>
        /// ActionResult for generating an invoice for corporate customer bookings
        /// </summary>
        /// <param name="id">Booking id</param>
        /// <returns>Invoice Index</returns>
        [Authorize(Roles = "Admin, Manager, Invoice Clerk")]
        public ActionResult Generate(int id)
        {
            try
            {
                //if invoice is successfully generated for booking
                if (GenerateInvoice(id))
                {
                    //return index view with success message stored in TempData
                    TempData["Success"] = "Invoice Successfully Generated";
                    return RedirectToAction("Index");
                }
                //if invoice is NOT successfully generated for booking
                else
                {
                    //return index view with error message stored in TempData
                    TempData["Error"] = "Unable to Generate Invoice";
                    return RedirectToAction("Index");
                }       
            }
            catch (Exception ex)
            {
                //if exception occurs, return to invoice index with error message
                TempData["Error"] = "Error: Something went wrong while generating invoice.";
                return RedirectToAction("Index");
            }               
        }

        /// <summary>
        /// ActionResult to Write Off an unpaid expired invoice
        /// </summary>
        /// <param name="id">Invoice id</param>
        /// <returns>Index view</returns>
        [Authorize(Roles = "Admin, Manager, Invoice Clerk")]
        public ActionResult WriteOff(int id)
        {
            try
            {
                //Find the invoice via ID and update status to write off 
                db.Invoices.Find(id).Status = InvoiceStatus.WriteOff;
                //save database changes
                db.SaveChanges();
                //update success message in TempData and return index view
                TempData["Success"] = "Invoice Successfully Written Off";
                return RedirectToAction("Index");
            }
            //if exception occurs
            catch (Exception ex)
            {
                //update error message in TempData and return Index view
                TempData["Error"] = "Invoice Could Not Be Written Off";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// HttpPost ActionResult to create a new invoice
        /// </summary>
        /// <param name="invoice">Created Invoice</param>
        /// <returns>Invoice Index view</returns>
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

        /// <summary>
        /// HttpGet ActionResult to return the edit invoice view
        /// </summary>
        /// <param name="id">Invoice id</param>
        /// <returns>Edit view</returns>
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

        /// <summary>
        /// HttpPost ActionResult to update an invoice
        /// </summary>
        /// <param name="invoice">Updated invoice</param>
        /// <returns>Invoice Index</returns>
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

        /// <summary>
        /// HttpGet ActionResult to return the delete invoice view
        /// </summary>
        /// <param name="id">Invoice id</param>
        /// <returns>Delete view</returns>
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

        /// <summary>
        /// HttpPost ActionResult to delete an invoice
        /// </summary>
        /// <param name="id">Invoice id</param>
        /// <returns>Invoice index</returns>
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
        /// <param name="id">Booking id</param>
        /// <returns>true/false success boolean</returns>
        private bool GenerateInvoice(int id)
        {
            try
            {
                //find booking via id
                Booking booking = db.Bookings.Find(id);

                //create a new invoice for the associated booking
                db.Invoices.Add(new Invoice()
                {
                    Booking = booking,  //associated booking
                    InvoiceDate = DateTime.Now, //current invoice generation date
                    ExpiryDate = DateTime.Now.AddDays(30),  //expiry date is 30 days from current date/time
                    AmountDue = booking.Total,  //amount due is booking total
                    Status = InvoiceStatus.Sent //invoice status is sent
                });

                //save database changes
                db.SaveChanges();

                //send booking invoice notification
                booking.Invoice.EmailInvoice();

                //return success true
                return true;
            }
            //if exception occurs
            catch (Exception ex)
            {
                //return false
                return false;
            }
            
        }

        /// <summary>
        /// Method to release unused resources
        /// </summary>
        /// <param name="disposing"></param>
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
