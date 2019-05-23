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
    public class InvoiceController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invoice
        public ActionResult Index()
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            User loggedInUser = userManager.FindByEmail(User.Identity.GetUserName());

            ViewBag.UserID = loggedInUser.Id;

            List<Booking> bookings = new List<Booking>();

            foreach (var booking in db.Bookings.ToList())
            {
                Customer customer = booking.User as Customer;

                if (customer.Corporate)
                {
                    bookings.Add(booking);
                }
            }
           
            return View(bookings);
        }

        // GET: Invoice/View
        public ActionResult View(int? id)
        {
            Booking booking = db.Bookings.Find(id);

            if (booking.Invoice==null)
            {
                return HttpNotFound();
            }

            double vat = Math.Round(booking.Total / 10 * 2, 2);

            ViewBag.Subtotal = booking.Total - vat;
            ViewBag.Vat = vat;

            return View(booking);
        }

        /// <summary>
        /// ActionResult to convert Invoice View to PDF
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public ActionResult PrintViewToPdf(int? bookingId)
        {
            var report = new ActionAsPdf("View", new {id=bookingId});
            return report;
        }
        

        /// <summary>
        /// HttpGet User Invoice Payment Confirmation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Confirmation(int id)
        {
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            int vehicleID = 0;

            //get bookingline vehicle id
            foreach (var line in db.BookingLines.ToList())
            {
                if (line.BookingID == id)
                {
                    vehicleID = line.VehicleID;
                }
            }

            Vehicle vehicle = db.Vehicles.Find(vehicleID);

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


            return View(model);
        }

        /// <summary>
        /// ActionResult to convert Invoice Payment Confirmation to PDF
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public ActionResult PrintConfirmationToPdf(int? bookingId)
        {
            var report = new ActionAsPdf("Confirmation", new { id = bookingId });
            return report;
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
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Generate(int id)
        {
            if (GenerateInvoice(id))
            {
                TempData["Success"] = "Invoice Successfully Generated";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Unable to Generate Invoice";
                return RedirectToAction("Index");
            }          
        }

        /// <summary>
        /// ActionResult to Write Off an unpaid expired invoice
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult WriteOff(int id)
        {
            try
            {
                db.Invoices.Find(id).Status = InvoiceStatus.WriteOff;
                db.SaveChanges();
                TempData["Success"] = "Invoice Successfully Written Off";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
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
        /// <param name="id"></param>
        /// <returns></returns>
        private bool GenerateInvoice(int id)
        {
            try
            {
                Booking booking = db.Bookings.Find(id);

                db.Invoices.Add(new Invoice()
                {
                    Booking = booking,
                    InvoiceDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddDays(30),
                    AmountDue = booking.Total,
                    Status = InvoiceStatus.Sent
                });

                db.SaveChanges();

                booking.Invoice.EmailInvoice();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            
        }

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
