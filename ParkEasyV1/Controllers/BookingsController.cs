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
using Newtonsoft.Json;
using ParkEasyV1.Models;
using ParkEasyV1.Models.ViewModels;
using Rotativa;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller for handling all booking events
    /// </summary>
    public class BookingsController : Controller
    {
        /// <summary>
        /// Global variable to store instance of the ApplicationDbManager
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Bookings
        public ActionResult Index()
        {
            var bookings = db.Bookings.Include(b => b.Flight).Include(b => b.Invoice).Include(b => b.ParkingSlot).Include(b => b.Tariff).Include(b => b.User);
            return View(bookings.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult return the Manage view with a collection of bookings
        /// </summary>
        /// <returns>Manage view with collection of bookings</returns>
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

        /// <summary>
        /// HttpGet ActionResult to return create booking view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: Bookings/Create
        public ActionResult Create()
        {
            return View();
        }

        
        //// POST: Bookings/Create        
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "ID,DateBooked,Duration,Total,BookingStatus,ValetService,CheckedIn,CheckedOut,UserID,FlightID,ParkingSlotID,TariffID")] Booking booking)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Bookings.Add(booking);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
           
        //    return View(booking);
        //}

        /// <summary>
        /// HttpPost ActionResult for creating a booking with details provided by a user
        /// </summary>
        /// <param name="model">CreateBookingViewModel with user booking data</param>
        /// <returns>Valet option view</returns>
        // POST: /Bookings/CreateBooking
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBooking(CreateBookingViewModel model)
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            CaptchaResponse response = ValidateCaptcha(Request["g-recaptcha-response"]);

            if (response.Success && ModelState.IsValid)
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

                User bookingUser = userManager.FindByEmail(model.Email);

                if (bookingUser==null)
                {
                    bookingUser = userManager.FindByName(User.Identity.Name);
                }

                //create customer booking
                db.Bookings.Add(new Booking()
                {
                    User = bookingUser,
                    Flight = db.Flights.Find(uniqueFlightId),
                    ParkingSlot = db.ParkingSlots.Find(FindAvailableParkingSlot()),
                    Tariff = db.Tariffs.Find(1),

                    DateBooked = DateTime.Now,
                    Duration = CalculateBookingDuration(model.DepartureDate, model.ReturnDate),
                    Total = db.Tariffs.Find(1).Amount * Convert.ToInt32(CalculateBookingDuration(model.DepartureDate, model.ReturnDate)),
                    BookingStatus = BookingStatus.Unpaid,
                    ValetService = false,
                    CheckedIn = false,
                    CheckedOut = false,
                });

                db.SaveChanges();

                int uniqueBookingId = GetLastBookingId();

                Booking createdBooking = db.Bookings.Find(uniqueBookingId);

                createdBooking.BookingLines = new List<BookingLine>() { new BookingLine() { Booking = db.Bookings.Find(uniqueBookingId), Vehicle = db.Vehicles.Find(uniqueVehicleId) } };

                bookingUser.PhoneNumber = model.PhoneNo;

                db.SaveChanges();

                TempData["bookingID"] = createdBooking.ID;

                return RedirectToAction("Valet");

            }
            else if(response.Success==false)
            {
                return Content("Error From Google ReCaptcha : " + response.ErrorMessage[0].ToString());
            }

            // If we got this far, something failed, redisplay form
            return View("Create");
        }

        /// <summary>  
        /// Class for validating Google reCaptcha API response 
        /// </summary>  
        /// <param name="response">reCaptcha reponse</param>  
        /// <returns>Deserialized captcha response</returns>  
        public static CaptchaResponse ValidateCaptcha(string response)
        {
            string secret = System.Web.Configuration.WebConfigurationManager.AppSettings["recaptchaPrivateKey"];
            var client = new WebClient();
            var jsonResult = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));
            return JsonConvert.DeserializeObject<CaptchaResponse>(jsonResult.ToString());
        }

        /// <summary>
        /// HttpGet ActionResult for returning the valet view
        /// </summary>
        /// <returns>Valet view</returns>
        public ActionResult Valet()
        {
            return View();
        }

        /// <summary>
        /// HttpGet ActionResult for amending a booking
        /// </summary>
        /// <param name="id">id of booking being edited</param>
        /// <returns>Edit view with ViewBookingViewModel</returns>
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

            int dateCompareResult = DateTime.Compare(booking.Flight.DepartureDate.AddHours(-24), DateTime.Now);

            if (dateCompareResult > 0)
            {
                ViewBag.Message = "You will not be charged for any amendments to this booking.";
            }
            else if (dateCompareResult <= 0)
            {
                ViewBag.Message = "Any amendmends made to this booking will result in an admin charge to be paid on arrival.";
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
                VehicleModel = vehicle.Model,
                VehicleColour = vehicle.Colour,
                VehicleRegistration = vehicle.RegistrationNumber,
                NoOfPassengers = vehicle.NoOfPassengers,
                Status = booking.BookingStatus
            };

            return View(model);
        }

        /// <summary>
        /// HttpPost ActionResult for updating the booking with new edits
        /// </summary>
        /// <param name="model">ViewBookingViewModel with inputted data</param>
        /// <returns>Booking Home View</returns>
        // POST: Bookings/Edit/5        
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

                if (booking.Flight.DepartureDate > DateTime.Now.AddHours(-24) && booking.Flight.DepartureDate <= DateTime.Now)
                {
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

                
            }
            return View(model);
        }

        /// <summary>
        /// HttpGet ActionResult for returning booking confirmation
        /// </summary>
        /// <param name="id">id of booking</param>
        /// <returns>Booking confirmation view</returns>
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
        /// ActionResult to convert Booking Confirmation to PDF
        /// </summary>
        /// <param name="bookingId">id of booking</param>
        /// <returns>booking confirmation view as PDF file</returns>
        public ActionResult PrintConfirmationPdf(int? bookingId)
        {
            var report = new ActionAsPdf("Confirmation", new { id = bookingId });
            return report;
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

        /// <summary>
        /// HttpGet ActionResult to handle if a customer chooses to purchase the valet service and update booking
        /// </summary>
        /// <param name="valetID">the id of the valet service selected</param>
        /// <returns>Payment Charge view</returns>
        public ActionResult PurchaseValet(int valetID)
        {
            Booking booking = db.Bookings.Find(TempData["bookingID"]);

            booking.ValetService = true;
            booking.Total = booking.Total + db.Tariffs.Find(valetID).Amount;

            db.SaveChanges();


            return RedirectToAction("Charge", "Payments");
        }

        /// <summary>
        /// HttpGet ActionResult to return the cancel booking view
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Cancel view with booking parameter</returns>
        // GET: Bookings/Cancel/5
        public ActionResult Cancel(int? id)
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

            int dateCompareResult = DateTime.Compare(booking.Flight.DepartureDate.AddHours(-48), DateTime.Now);

            if (dateCompareResult > 0)
            {
                ViewBag.Message = "If you cancel this booking now, you will not be charged.";
            }
            else if (dateCompareResult <= 0)
            {
                ViewBag.Message = "If you cancel this booking now, you will only recieve a partial refund of 70%.";
            }            

            return View(booking);
        }

        /// <summary>
        /// HttpPost ActionResult for cancelling a booking
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>User home</returns>
        // POST: Bookings/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public ActionResult CancelConfirmed(int id)
        {
            string message=null;

            Booking booking = db.Bookings.Find(id);

            int dateCompareResult = DateTime.Compare(booking.Flight.DepartureDate.AddHours(-48), DateTime.Now);

            if (booking.Flight.DepartureDate<=DateTime.Now)
            {
                if (dateCompareResult > 0 && User.IsInRole("Customer"))
                {
                    message = "Your full refund will be processed to your card or PayPal account.";
                }
                else if (dateCompareResult <= 0 && User.IsInRole("Customer"))
                {
                    message = "Your partial refund will be processed to your card or PayPal account.";
                }
            }            

            booking.BookingStatus = BookingStatus.Cancelled;

            booking.ParkingSlot.Status = Status.Available;

            db.SaveChanges();
            TempData["Success"] = "Booking No: " + id + " has been successfully cancelled." + message;
            return RedirectToAction("Index", "Users");
        }

        /// <summary>
        /// ActionResult for checking in a booking
        /// </summary>
        /// <param name="id">id of booking</param>
        /// <returns>Booking Check In View</returns>
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

        /// <summary>
        /// ActionResult for checking out a booking
        /// </summary>
        /// <param name="id">id of booking</param>
        /// <returns>Booking check out view</returns>
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

        /// <summary>
        /// ActionResult for handling the event a customer does not show up for a booking
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Manage bookings view</returns>
        public ActionResult NoShow(int id)
        {
            try
            {
                Booking booking = db.Bookings.Find(id);

                booking.BookingStatus = BookingStatus.NoShow;

                db.SaveChanges();

                TempData["Success"] = "Booking Successfully Marked As No Show";
                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Booking Could Not Be Marked As No Show";
                return RedirectToAction("Manage");
            }
        }

        /// <summary>
        /// ActionResult for handling the event a Customer is delayed returning from their trip by increasing the booking stay by 1 day
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Manage booking view</returns>
        public ActionResult Delay(int id)
        {
            try
            {
                Booking booking = db.Bookings.Find(id);

                booking.Flight.ReturnDate.AddDays(1);
                booking.Duration++;
                booking.BookingStatus = BookingStatus.Delayed;
                booking.Total = booking.Total + booking.Tariff.Amount;

                db.SaveChanges();

                TempData["Success"] = "Booking Delay Successfully Updated";
                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Booking Delay Could Not Be Updated";
                return RedirectToAction("Manage");
            }
        }

        /// <summary>
        /// Function for checking in a booking using booking id
        /// </summary>
        /// <param name="id">id of booking</param>
        /// <returns>true/false</returns>
        private bool CheckInBooking(int id)
        {
            foreach (var booking in db.Bookings.ToList())
            {
                if (booking.ID == id)
                {
                    booking.CheckedOut = false;
                    booking.CheckedIn = true;
                    booking.ParkingSlot.Status = Status.Occupied;
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Function for checking out a booking using booking id
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>true or false</returns>
        private bool CheckOutBooking(int id)
        {
            foreach (var booking in db.Bookings.ToList())
            {
                if (booking.ID == id)
                {
                    booking.CheckedIn = false;
                    booking.CheckedOut = true;
                    booking.ParkingSlot.Status = Status.Available;
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Function for getting the last booking id inserted into the database
        /// </summary>
        /// <returns>booking id</returns>
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

        /// <summary>
        /// Function for getting the id of the last flight inserted into the database
        /// </summary>
        /// <returns>flight id</returns>
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

        /// <summary>
        /// Function for getting the id of the last vehicle inserted into the database
        /// </summary>
        /// <returns>vehicle id</returns>
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

        /// <summary>
        /// Function to find the next available parking slot
        /// </summary>
        /// <returns>id of available parking slot</returns>
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

        /// <summary>
        /// Function to calculate the duration of a booking using the start and end date
        /// </summary>
        /// <param name="departureDate">date of flight departure</param>
        /// <param name="returnDate">date of flight return</param>
        /// <returns>duration of booking in days</returns>
        private int CalculateBookingDuration(DateTime departureDate, DateTime returnDate)
        {
            TimeSpan duration = returnDate.Subtract(departureDate);

            return Convert.ToInt32(duration.TotalDays);
        }

        /// <summary>
        /// method to unload unused resources
        /// </summary>
        /// <param name="disposing">true/false to dispose</param>
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
