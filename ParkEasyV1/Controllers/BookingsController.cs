using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ParkEasyV1.Models;
using ParkEasyV1.Models.ViewModels;

namespace ParkEasyV1.Controllers
{
    public class BookingsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.Flight).Include(b => b.Invoice).Include(b => b.ParkingSlot).Include(b => b.Tariff).Include(b => b.User);
            return View(bookings.ToList());
        }

        // GET: Bookings/Manage
        public ActionResult Manage()
        {
            foreach (var user in db.Users.ToList())
            {
                if (user.Email.Equals(User.Identity.Name))
                {
                    ViewBag.UserID = user.Id;
                }
            }

            return View(db.Bookings.OrderBy(b => b.DateBooked).ToList());
        }

        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Bookings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,DateBooked,Duration,Total,BookingStatus,ValetService,CheckedIn,CheckedOut,UserID,FlightID,ParkingSlotID,TariffID")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                db.Bookings.Add(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.FlightID = new SelectList(db.Flights, "ID", "DepartureFlightNo", booking.FlightID);
            ViewBag.ID = new SelectList(db.Invoices, "ID", "CustomerID", booking.ID);
            ViewBag.ParkingSlotID = new SelectList(db.ParkingSlots, "ID", "ID", booking.ParkingSlotID);
            ViewBag.TariffID = new SelectList(db.Tariffs, "ID", "Type", booking.TariffID);
            ViewBag.UserID = new SelectList(db.Users, "Id", "FirstName", booking.UserID);
            return View(booking);
        }

        //
        // POST: /Bookings/CreateBooking
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBooking(CreateBookingViewModel model)
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            if (ModelState.IsValid)
            {
                //create customer vehicle
                db.Vehicles.Add(new Vehicle()
                {
                    RegistrationNumber = model.VehicleRegistration,
                    Make = model.VehicleMake,
                    Model = model.VehicleModel,
                    Colour = model.VehicleModel,
                    NoOfPassengers = model.NoOfPassengers
                });

                //create customer flight
                db.Flights.Add(new Flight()
                {
                    DepartureFlightNo = model.DepartureFlightNo,
                    DepartureTime = model.DepartureTime,
                    ReturnFlightNo = model.ReturnFlightNo,
                    ReturnFlightTime = model.ReturnTime,
                    DepartureDate = model.DepartureDate,
                    ReturnDate = model.ReturnDate,
                    DestinationAirport = model.DestinationAirport
                });

                db.SaveChanges();


                //CREATE NEW BOOKING

                int uniqueVehicleId = GetLastVehicleId();
                int uniqueFlightId = GetLastFlightId();

                //create customer booking
                db.Bookings.Add(new Booking()
                {
                    User = userManager.FindByName(User.Identity.Name),
                    Flight = db.Flights.Find(uniqueFlightId),
                    ParkingSlot = db.ParkingSlots.Find(FindAvailableParkingSlot()),
                    Tariff = db.Tariffs.Find(1),

                    DateBooked = DateTime.Now,
                    Duration = CalculateBookingDuration(model.DepartureDate, model.ReturnDate),
                    Total = db.Tariffs.Find(1).Amount * Convert.ToInt32(CalculateBookingDuration(model.DepartureDate, model.ReturnDate)),
                    BookingStatus = BookingStatus.UnPaid,
                    ValetService = false,
                    CheckedIn = false,
                    CheckedOut = false,
                });

                db.SaveChanges();

                int uniqueBookingId = GetLastBookingId();

                Booking createdBooking = db.Bookings.Find(uniqueBookingId);

                createdBooking.BookingLines = new List<BookingLine>() { new BookingLine() { Booking = db.Bookings.Find(uniqueBookingId), Vehicle = db.Vehicles.Find(uniqueVehicleId) } };

                User user = userManager.FindByName(User.Identity.Name);
                user.PhoneNumber = model.PhoneNo;

                db.SaveChanges();

                TempData["bookingID"] = createdBooking.ID;

                return RedirectToAction("Valet");

            }

            // If we got this far, something failed, redisplay form
            return View("Create");
        }

        public ActionResult Valet()
        {
            return View();
        }

        // GET: Bookings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            int vehicleID=0;

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
                VehicleModel  = vehicle.Model,
                VehicleColour = vehicle.Colour,
                VehicleRegistration = vehicle.RegistrationNumber,
                NoOfPassengers = vehicle.NoOfPassengers
            };

            return View(model);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ViewBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                //get the booking
                Booking booking = db.Bookings.Find(model.BookingID);

                //initialize vehicle id
                int vehicleID = 0;

                //get bookingline vehicle id
                foreach (var line in db.BookingLines.ToList())
                {
                    if (line.BookingID == model.BookingID)
                    {
                        vehicleID = line.VehicleID;
                    }
                }

                //get the vehicle linked to booking
                Vehicle vehicle = db.Vehicles.Find(vehicleID);

                //update booking
                booking.User.FirstName = model.FirstName;
                booking.User.LastName = model.Surname;
                booking.User.AddressLine1 = model.AddressLine1;
                booking.User.AddressLine2 = model.AddressLine2;
                booking.User.City = model.City;
                booking.User.Email = model.Email;
                booking.User.PhoneNumber = model.PhoneNo;
                vehicle.Make = model.VehicleMake;
                vehicle.Model = model.VehicleModel;
                vehicle.Colour = model.VehicleColour;
                vehicle.RegistrationNumber = model.VehicleRegistration;
                vehicle.NoOfPassengers = model.NoOfPassengers;

                db.SaveChanges();

                TempData["Success"] = "Booking Successfully Updated";

                if (User.IsInRole("Customer"))
                {
                    return RedirectToAction("MyBookings", "Users");
                }
                else
                {
                    return RedirectToAction("Manage", "Bookings");
                }
            }
            return View(model);
        }

        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            db.Bookings.Remove(booking);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult PurchaseValet(int valetID)
        {
            Booking booking = db.Bookings.Find(TempData["bookingID"]);

            booking.ValetService = true;
            booking.Total = booking.Total + db.Tariffs.Find(valetID).Amount;

            db.SaveChanges();


            return RedirectToAction("Charge", "Payments");
        }

        public ActionResult CheckIn(int id)
        {
            if (CheckInBooking(id))
            {
                TempData["Success"] = "Booking Checked In Successfully";
                return RedirectToAction("Departures", "Users");
            }
            else
            {
                return RedirectToAction("Index");
            }            
        }

        public ActionResult CheckOut(int id)
        {
            if (CheckOutBooking(id))
            {
                TempData["Success"] = "Booking Checked Out Successfully";
                return RedirectToAction("Returns", "Users");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        private bool CheckInBooking(int id)
        {
            foreach (var booking in db.Bookings.ToList())
            {
                if (booking.ID == id)
                {
                    booking.CheckedOut = false;
                    booking.CheckedIn = true;
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        private bool CheckOutBooking(int id)
        {
            foreach (var booking in db.Bookings.ToList())
            {
                if (booking.ID == id)
                {
                    booking.CheckedIn = false;
                    booking.CheckedOut = true;
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        private int GetLastBookingId()
        {
            Booking lastBookingEntry = db.Bookings
                       .OrderByDescending(b => b.ID)
                       .FirstOrDefault();

            if (lastBookingEntry!=null)
            {
                return lastBookingEntry.ID;
            }

            return 1;
            
        }


        private int GetLastFlightId()
        {
            Flight lastFlightEntry = db.Flights
                       .OrderByDescending(f => f.ID)
                       .FirstOrDefault();

            if (lastFlightEntry!=null)
            {
                return lastFlightEntry.ID;
            }

            return 1;
            
        }

        private int GetLastVehicleId()
        {
            Vehicle lastVehicleEntry = db.Vehicles
                       .OrderByDescending(v => v.ID)
                       .FirstOrDefault();

            if (lastVehicleEntry!=null)
            {
                return lastVehicleEntry.ID;
            }

            return 1;
        }

        private int FindAvailableParkingSlot()
        {
            foreach (var slot in db.ParkingSlots)
            {
                if (slot.Status.Equals(Status.Available))
                {
                    slot.Status = Status.Reserved;

                    return slot.ID;
                }
            }

            return 0;
        }

        private int CalculateBookingDuration(DateTime departureDate, DateTime returnDate)
        {
            TimeSpan duration = returnDate.Subtract(departureDate);

            return Convert.ToInt32(duration.TotalDays);
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
