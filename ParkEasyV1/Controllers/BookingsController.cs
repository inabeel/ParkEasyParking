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
using Itenso.TimePeriod;
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
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult for returning Bookings Index
        /// </summary>
        /// <returns>Index view</returns>
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
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult Manage()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the current user's ID and store in viewbag for front-end display
            ViewBag.UserID = userManager.FindByEmail(User.Identity.GetUserName()).Id;

            //return view with collection of bookings ordered by date booked
            return View(db.Bookings.OrderBy(b => b.DateBooked).ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return Booking details view
        /// </summary>
        /// <param name="id">Booking id</param>
        /// <returns>Details view</returns>
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
        [Authorize]
        public ActionResult Create()
        {
            //check if logged in user is in Customer role
            if (User.IsInRole("Customer"))
            {
                //declare instance of usermanager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

                //get the current logged in user 
                User currentUser = userManager.FindByEmail(User.Identity.GetUserName());

                //create a new CreateBookingViewModel and populate with current user data
                CreateBookingViewModel model = new CreateBookingViewModel
                {
                    FirstName = currentUser.FirstName,
                    Surname = currentUser.LastName,
                    Email = currentUser.Email,
                    AddressLine1 = currentUser.AddressLine1,
                    AddressLine2 = currentUser.AddressLine2,
                    City = currentUser.City,
                    Postcode = currentUser.Postcode,
                    PhoneNo = currentUser.PhoneNumber,
                    DepartureDate = DateTime.Today,
                    ReturnDate = DateTime.Today.AddDays(1)
                };

                //check if AvailabilityViewModel stored in TempData is NOT null
                //if NOT null then user has came from Availability Checker view
                if (TempData["AvailabilityModel"] as AvailabilityViewModel != null)
                {
                    //retrieve the view model from TempData
                    AvailabilityViewModel availabilityModel = TempData["AvailabilityModel"] as AvailabilityViewModel;

                    //input the departure/return details to the CreateBookingViewModel
                    model.DepartureDate = availabilityModel.DepartureDate;
                    model.DepartureTime = availabilityModel.DepartureTime;
                    model.ReturnDate = availabilityModel.ReturnDate;
                    model.ReturnTime = availabilityModel.ReturnTime;                                
                }

                //return the View with the CreateBookingViewModel attached
                return View(model);
            }
            //if the current user is not a Customer - return the Create view with no model
            return View();
        }

        
        /// <summary>
        /// HttpPost ActionResult for creating a booking with details provided by a user
        /// </summary>
        /// <param name="model">CreateBookingViewModel with user booking data</param>
        /// <returns>Valet option view</returns>
        // POST: /Bookings/CreateBooking
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateBookingViewModel model)
        {
            try
            {
                //declare instance of usermanager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

                //get Google reCaptcha API response
                CaptchaResponse response = ValidateCaptcha(Request["g-recaptcha-response"]);

                //if Google reCaptcha API response and model are valid
                if (response.Success && ModelState.IsValid)
                {
                    //VALIDATION TO CHECK BOOKINGS TODAY ARE BOOKED AT LEAST 1 HOUR IN ADVANCE
                    //check if booking departure date is today and departure time is at least 1 hour ahead of the current time
                    if (model.DepartureDate.Equals(DateTime.Today) && model.DepartureTime < new TimeSpan(DateTime.Today.Hour, DateTime.Today.Minute, 0).Add(new TimeSpan(1,0,0)))
                    {
                        //set error message and return view
                        TempData["Error"] = "Error: The departure time for booking today must be at least 1 hour in advance minimum.";
                        return View(model);
                    }

                    //get the TimeRange for the selected dates
                    TimeRange selectedTimeRange = new TimeRange(
                    new DateTime(model.DepartureDate.Year, model.DepartureDate.Month, model.DepartureDate.Day, model.DepartureTime.Hours, model.DepartureTime.Minutes, 0),
                    new DateTime(model.ReturnDate.Year, model.ReturnDate.Month, model.ReturnDate.Day, model.ReturnTime.Hours, model.ReturnTime.Minutes, 0));

                    //if available parking space is NOT 0
                    if (FindAvailableParkingSlot(selectedTimeRange)!=0)
                    {
                        //create customer vehicle
                        Vehicle vehicle = db.Vehicles.Add(new Vehicle()
                        {
                            RegistrationNumber = model.VehicleRegistration,
                            Make = model.VehicleMake,
                            Model = model.VehicleModel,
                            Colour = model.VehicleColour,
                            NoOfPassengers = model.NoOfPassengers
                        });

                        //create customer flight
                        Flight flight = db.Flights.Add(new Flight()
                        {
                            DepartureFlightNo = model.DepartureFlightNo,
                            DepartureTime = model.DepartureTime,
                            ReturnFlightNo = model.ReturnFlightNo,
                            ReturnFlightTime = model.ReturnTime,
                            DepartureDate = model.DepartureDate,
                            ReturnDate = model.ReturnDate,
                            DestinationAirport = model.DestinationAirport
                        });

                        //save database changes
                        db.SaveChanges();

                        //try to find the booking user by email from booking form
                        User bookingUser = userManager.FindByEmail(model.Email);

                        //if user cannot be found via email from booking form
                        if (bookingUser == null)
                        {
                            //find the current logged in user
                            bookingUser = userManager.FindByName(User.Identity.Name);
                        }

                        //create customer booking
                        Booking booking = db.Bookings.Add(new Booking()
                        {
                            User = bookingUser,
                            Flight = db.Flights.Find(flight.ID),
                            ParkingSlot = db.ParkingSlots.Find(FindAvailableParkingSlot(selectedTimeRange)),
                            Tariff = db.Tariffs.Find(1),
                            DateBooked = DateTime.Now,
                            Duration = CalculateBookingDuration(model.DepartureDate, model.ReturnDate),
                            Total = db.Tariffs.Find(1).Amount * Convert.ToInt32(CalculateBookingDuration(model.DepartureDate, model.ReturnDate)),
                            BookingStatus = BookingStatus.Unpaid,
                            ValetService = false,
                            CheckedIn = false,
                            CheckedOut = false,
                        });

                        //save database changes
                        db.SaveChanges();

                        //create a new booking line for the booking with customer vehicle
                        booking.BookingLines = new List<BookingLine>() { new BookingLine() { Booking = db.Bookings.Find(booking.ID), Vehicle = db.Vehicles.Find(vehicle.ID) } };

                        //update the contact phone number on the user's account from the booking form
                        bookingUser.PhoneNumber = model.PhoneNo;

                        //save database changes
                        db.SaveChanges();

                        //store the booking id in a TempData
                        TempData["bookingID"] = booking.ID;

                        //return the Valet options view to the user
                        return RedirectToAction("Valet");
                    }
                    //if available parking space id = 0 (no spaces are available)
                    else                
                    {
                        //return view with error message
                        TempData["Error"] = "No Parking Slots Available For Your Selected Dates. Please Enter New Dates And Try Again.";
                        return View(model);
                    }

                }
                else if(response.Success==false)
                {
                    //return Google reCaptcha API error
                    return Content("Error From Google ReCaptcha : " + response.ErrorMessage[0].ToString());
                }
            }
            catch (Exception ex)
            {   
                // If exception occurs, redisplay form
                return View(model);
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Function to find the next available parking slot for the booking period selected by the user, and return the id of the available parking slot
        /// </summary>
        /// <param name="selectedTimeRange">Booking date period selected by user</param>
        /// <returns>Available parking slot id</returns>
        private int FindAvailableParkingSlot(TimeRange selectedTimeRange)
        {
            try
            {
                //loop through all parking slots
                foreach (var slot in db.ParkingSlots.ToList())
                {
                    //set the overlap counter to 0
                    int overlapCounter = 0;

                    //loop through all bookings associated with parking slot (excluding cancelled bookings)
                    foreach (var slotBooking in slot.Bookings.Where(b=>b.BookingStatus!=BookingStatus.Cancelled).ToList())
                    {
                        //create a TimeRange variable to hold the range of dates of the booking
                        TimeRange bookingRange = new TimeRange(
                        new DateTime(
                            slotBooking.Flight.DepartureDate.Year,
                            slotBooking.Flight.DepartureDate.Month,
                            slotBooking.Flight.DepartureDate.Day,
                            slotBooking.Flight.DepartureTime.Hours,
                            slotBooking.Flight.DepartureTime.Minutes,
                            0),
                        new DateTime(
                            slotBooking.Flight.ReturnDate.Year,
                            slotBooking.Flight.ReturnDate.Month,
                            slotBooking.Flight.ReturnDate.Day,
                            slotBooking.Flight.ReturnFlightTime.Hours,
                            slotBooking.Flight.ReturnFlightTime.Minutes,
                            0));

                        //check if the booking time range overlaps with the user's selected booking time range
                        if (selectedTimeRange.OverlapsWith(bookingRange))
                        {
                            //if there is an overlap, then there is already a booking in this parking slot during the user's selected date period
                            //update the overlap counter
                            overlapCounter++;
                        }
                    }

                    //if there are no overlaps in the parking slot bookings
                    if (overlapCounter == 0)
                    {
                        //return the parking slot id as this parking slot is available during the selected dates
                        return slot.ID;
                    }
                }
            }
            catch (Exception ex)
            {
                //if exception 
                return 0;
            }            

            //if no available parking slots could be found, return 0
            return 0;
        }

        /// <summary>  
        /// Class for validating Google reCaptcha API response 
        /// </summary>  
        /// <param name="response">reCaptcha reponse</param>  
        /// <returns>Deserialized captcha response</returns>  
        private static CaptchaResponse ValidateCaptcha(string response)
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
        [Authorize]
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
        [Authorize]
        public ActionResult Edit(int? id)
        {
            //check if booking id is null
            if (id == null)
            {
                //return Bad Request error
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //find the booking via id and check if booking is null
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                //return httpnotfound if booking is null
                return HttpNotFound();
            }

            //if booking start date is later than or equal to the current date
            //run datecomparison code to check if booking can be amended for free or not
            if (booking.Flight.DepartureDate > DateTime.Now || booking.Flight.DepartureDate.Day == DateTime.Now.Day)
            {
                //create a variable to hold the result of the date comparison between the booking departure date and the current date
                int dateCompareResult = DateTime.Compare(booking.Flight.DepartureDate.AddHours(-24), DateTime.Now);

                //if the date compare result is more than 0
                if (dateCompareResult > 0)
                {
                    //the booking is outside the 24 hour booking amendment period
                    //store message in ViewBag to be displayed on the front-end
                    ViewBag.Message = "You will not be charged for any amendments to this booking.";
                }
                //if the date compare result is less than or equal to 0
                else if (dateCompareResult <= 0)
                {
                    //the booking is inside the 24 hour booking amendment period
                    //store message in ViewBag to be displayed on the front-end
                    ViewBag.Message = "Any amendmends made to this booking will result in an admin charge to be paid on arrival.";
                }
            }
            

            //find the vehicle attached to the booking via bookingline
           Vehicle vehicle = db.Vehicles.Find(booking.BookingLines.First().VehicleID);

            //create a new viewbooking view model and input all booking/flight/vehicle data
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

            //return the edit view with model 
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
        [Authorize]
        public ActionResult Edit([Bind(Include = "BookingID, DepartureDate, DepartureTime, ReturnDate, ReturnTime, Duration, Total, Valet, FirstName, Surname, AddressLine1, AddressLine2, City, Postcode, Email, PhoneNo, VehicleMake, VehicleModel, VehicleColour, VehicleRegistration, NoOfPassengers, Status")]ViewBookingViewModel model)
        {
            //check if model state is valid
            if (ModelState.IsValid)
            {
                try
                {
                    //get the booking via id
                    Booking booking = db.Bookings.Find(model.BookingID);

                    //get the vehicle linked to booking via bookingline
                    Vehicle vehicle = db.Vehicles.Find(booking.BookingLines.First().VehicleID);

                    //check if the booking departure date is later than the current date - 24hours and departure date is earlier than the current date
                    if (DateTime.Now <= booking.Flight.DepartureDate.AddHours(-24))
                    {
                        ////update booking
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

                        ////save changes
                        db.SaveChanges();


                        //store success message in tempdata
                        TempData["Success"] = "Booking Successfully Updated";

                        //check if user is in customer role
                        if (User.IsInRole("Customer"))
                        {
                            //return customer to my bookings
                            return RedirectToAction("MyBookings", "Users");
                        }
                        else
                        {
                            //return staff to manage bookings
                            return RedirectToAction("Manage", "Bookings");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //if exception occurs, redisplay view
                    return View(model);
                }             
            }
            //if model state is not valid, return view with model
            return View(model);
        }

        /// <summary>
        /// HttpGet ActionResult for returning booking confirmation
        /// </summary>
        /// <param name="id">id of booking</param>
        /// <returns>Booking confirmation view</returns>
        public ActionResult Confirmation(int id)
        {
            //find booking via id and check if booking is null
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                //if booking is null, return httpnotfound error
                return HttpNotFound();
            }

            //get the vehicle associated with the booking via booking line
            Vehicle vehicle = booking.BookingLines.First().Vehicle;

            //create a new viewbookingviewmodel and populate with booking/flight/vehicle data
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

            //return the view with model
            return View(model);
        }

        /// <summary>
        /// ActionResult to convert Booking Confirmation to PDF
        /// </summary>
        /// <param name="bookingId">id of booking</param>
        /// <returns>booking confirmation view as PDF file</returns>
        public ActionResult PrintConfirmationPdf(int? bookingId)
        {
            try
            {
                //create new ActionAsPdf of the Confirmation view using Rotativa
                var pdf = new ActionAsPdf("Confirmation", new { id = bookingId });
                //return PDF
                return pdf;
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }            
        }

        /// <summary>
        /// HttpGet ActionResult to return the delete booking view
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>delete booking view</returns>
        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            //check if booking id is null
            if (id == null)
            {
                //return bad request
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //get the booking via id
            Booking booking = db.Bookings.Find(id);
            //check if booking is null
            if (booking == null)
            {
                //return httpnotfound
                return HttpNotFound();
            }
            //return the view with booking
            return View(booking);
        }

        /// <summary>
        /// HttpPost ActionResult for deleting a booking
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Index view</returns>
        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                //find the booking and remove it from the database, save changes and return view
                Booking booking = db.Bookings.Find(id);
                db.Bookings.Remove(booking);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //if exception occurs, update error message and return to index
                TempData["Error"] = "Error: Unable to perform delete action.";
                return RedirectToAction("Index");
            }            
        }

        /// <summary>
        /// HttpGet ActionResult to handle if a customer chooses to purchase the valet service and update booking
        /// </summary>
        /// <param name="valetID">the id of the valet service selected</param>
        /// <returns>Payment Charge view</returns>
        [Authorize]
        public ActionResult PurchaseValet(int valetID)
        {
            try
            {
                //find booking via id stored in TempData
                Booking booking = db.Bookings.Find(TempData["bookingID"]);

                //set valet service attribute to true
                booking.ValetService = true;
                //update booking total for selected valet ID tariff charge
                booking.Total = booking.Total + db.Tariffs.Find(valetID).Amount;
                //save database changes
                db.SaveChanges();

                //return charge payment view
                return RedirectToAction("Charge", "Payments");
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the cancel booking view
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Cancel view with booking parameter</returns>
        // GET: Bookings/Cancel/5
        [Authorize]
        public ActionResult Cancel(int? id)
        {
            //if the booking id is null
            if (id == null)
            {
                //return badrequest error
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //find the booking using booking id parameter
            Booking booking = db.Bookings.Find(id);
            //if the booking returns null
            if (booking == null)
            {
                //return httpnotfound error
                return HttpNotFound();
            }

            //declare a variable to hold the result of the comparision between the current date and the booking departure date - 48 hours
            int dateCompareResult = DateTime.Compare(booking.Flight.DepartureDate.AddHours(-48), DateTime.Now);

            //if the result is more than 0
            //the cancellation is outwith 48 hours of departure date
            if (dateCompareResult > 0)
            {
                //display no cancellation charge message
                ViewBag.Message = "If you cancel this booking now, you will not be charged.";
            }
            //if the result is less than or equal to 0
            //the cancellation is within 48 hours of departure date
            else if (dateCompareResult <= 0)
            {
                //display cancellation charge message
                ViewBag.Message = "If you cancel this booking now, you will only recieve a partial refund of 70%.";
            }            
            //return the cancellation view
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
        [Authorize]
        public ActionResult CancelConfirmed(int id)
        {
            try
            {
                //declare and initialize a variable to hold a message to the user
                string message=null;

                //find the booking using the id parameter
                Booking booking = db.Bookings.Find(id);

                //declare a variable to hold the result of a date comparison between the current date and the booking departure date - 48 hours
                int dateCompareResult = DateTime.Compare(booking.Flight.DepartureDate.AddHours(-48), DateTime.Now);

                //if the departure date is before or equal to the current date
                if (booking.Flight.DepartureDate<=DateTime.Now)
                {
                    //check if date compare result is more than 0 and booking is being cancelled by Customer
                    //if date compare result is more than 0 - cancellation is outwith the 48 hours surplus charge period
                    if (dateCompareResult > 0 && User.IsInRole("Customer"))
                    {
                        //set message to the user stating they will not be charged for cancel
                        message = "Your full refund will be processed to your card or PayPal account.";
                    }
                    //check if date compare result is less than or equal to 0 and booking is being cancelled by Customer
                    //if date compare result less than or equal to 0 - cancellation is witin the 48 hours surplus charge period
                    else if (dateCompareResult <= 0 && User.IsInRole("Customer"))
                    {
                        //set message to the user stating they will only receieve partial refund for cancellation
                        message = "Your partial refund will be processed to your card or PayPal account.";
                    }
                }            

                //update booking status
                booking.BookingStatus = BookingStatus.Cancelled;

                //set parking slot status to available
                booking.ParkingSlot.Status = Status.Available;

                //save db changes and set success message
                db.SaveChanges();
                TempData["Success"] = "Booking No: " + id + " has been successfully cancelled." + message;
                return RedirectToAction("Index", "Users");
            }
            catch (Exception ex)
            {
                //if exception occurs, redisplay form with error message
                TempData["Error"] = "Booking could not be cancelled.";
                return RedirectToAction("Cancel", new { id});
            }            
        }

        /// <summary>
        /// HttpGet ActionResult to return a view to allow users to check booking availability
        /// </summary>
        /// <returns>Availability View</returns>
        public ActionResult Availability()
        {
            //return availability view
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to check the availability of a booking and return success
        /// </summary>
        /// <param name="model">AvailabilityViewModel with selected booking dates inputted</param>
        /// <returns>Availability view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Availability(AvailabilityViewModel model)
        {
            //check view model state is valid
            if (ModelState.IsValid)
            {
                try
                {
                    //VALIDATION TO CHECK BOOKINGS TODAY ARE BOOKED AT LEAST 1 HOUR IN ADVANCE
                    //check if booking departure date is today and departure time is at least 1 hour ahead of the current time
                    //if (model.DepartureDate.Equals(DateTime.Today) && model.DepartureTime < new TimeSpan(DateTime.Today.Hour, DateTime.Today.Minute, 0).Add(new TimeSpan(1, 0, 0)))
                    //{
                    //    //set error message and return view
                    //    TempData["UnAvailable"] = "Error: The departure time for booking today must be at least 1 hour in advance minimum.";
                    //    return View(model);
                    //}

                    //create a TimeRange from the selected departure/return date and time in model
                    TimeRange selectedTimeRange = new TimeRange(
                    new DateTime(model.DepartureDate.Year, model.DepartureDate.Month, model.DepartureDate.Day, model.DepartureTime.Hours, model.DepartureTime.Minutes, 0),
                    new DateTime(model.ReturnDate.Year, model.ReturnDate.Month, model.ReturnDate.Day, model.ReturnTime.Hours, model.ReturnTime.Minutes, 0));

                    //call function to calculate the number of unavailable parking slots during this time period
                    int unavailableSlots = GetUnavailableSlots(selectedTimeRange);

                    //if the number of unavailable parking slots DOES NOT equal 150 (150 is the total number of parking spaces)
                    //then a parking slot is available during this time period
                    //!=1 for testing
                    if (unavailableSlots != 150)
                    {
                        //update availability message, store the model and return the availability view
                        TempData["Available"] = "Booking Available!";
                        TempData["AvailabilityModel"] = model;
                        ViewBag.Total = Math.Round(CalculateBookingTotal(model.DepartureDate, model.ReturnDate), 2);
                        return View(model);
                    }
                    //if number of unavailble slots IS equal to 150 - then parking slot is not available
                    else
                    {
                        //update message in TempData and return the view
                        TempData["UnAvailable"] = "Booking Not Available, Please select new booking dates.";
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    //if exception occurs, redisplay form with error message
                    TempData["UnAvailable"] = "Sorry, an error occured with checking the availability of this booking. Please contact us for further information.";
                    return View(model);
                }
                
            }
            //if model state is not valid return the availability view with model
            return View(model);
        }

        /// <summary>
        /// Function to determine how many unavailable parking slots there are for a certain TimeRange
        /// </summary>
        /// <param name="selectedTimeRange">TimeRange slots should be checked against</param>
        /// <returns>Number of unavailable parking slots</returns>
        public int GetUnavailableSlots(TimeRange selectedTimeRange)
        {
            try
            {
                //initialize unavailable slots to 0
                int unavailableSlots = 0;

                //loop through all parking slots
                //remove where clause for live version
                //foreach (var slot in db.ParkingSlots.Where(s => s.ID == 102).ToList())
                foreach(var slot in db.ParkingSlots.ToList())
                {
                    //loop through all bookings associated with parking slot (excluding cancelled bookings)
                    foreach (var booking in slot.Bookings.Where(b=>b.BookingStatus!=BookingStatus.Cancelled).ToList())
                    {
                        //create a TimeRange variable to hold the range of dates of the booking
                        TimeRange bookingRange = new TimeRange(
                        new DateTime(
                            booking.Flight.DepartureDate.Year,
                            booking.Flight.DepartureDate.Month,
                            booking.Flight.DepartureDate.Day,
                            booking.Flight.DepartureTime.Hours,
                            booking.Flight.DepartureTime.Minutes,
                            0),
                        new DateTime(
                            booking.Flight.ReturnDate.Year,
                            booking.Flight.ReturnDate.Month,
                            booking.Flight.ReturnDate.Day,
                            booking.Flight.ReturnFlightTime.Hours,
                            booking.Flight.ReturnFlightTime.Minutes,
                            0));

                        //check if the booking time range overlaps with the user's selected booking time range
                        if (selectedTimeRange.OverlapsWith(bookingRange))
                        {
                            //if overlap - this slot is unavailable during the time range
                            //increase counter
                            unavailableSlots++;
                        }
                    }
                }
                //return the number of unavailable parking slots
                return unavailableSlots;
            }
            catch (Exception ex)
            {
                //if exception occurs, return 0
                return 0;
            }
            
        }

        /// <summary>
        /// ActionResult for checking in a booking
        /// </summary>
        /// <param name="id">id of booking</param>
        /// <returns>Booking Check In View</returns>
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult CheckIn(int id)
        {
            try
            {
                //if check in booking function returns true (success)
                if (CheckInBooking(id))
                {
                    //update success message in tempdata and return departures view
                    TempData["Success"] = "Booking Checked In Successfully";
                    return RedirectToAction("Departures", "Users");
                }
                else
                {
                    //return view with error message
                    TempData["Error"] = "Error: Could not check in booking.";
                    return RedirectToAction("Departures", "Users");
                }
            }
            catch (Exception ex)
            {
                //if exception occurs, return departures view with error message
                TempData["Error"] = "Error: An error occured while checking in this booking.";
                return RedirectToAction("Departures", "Users");
            }                        
        }

        /// <summary>
        /// ActionResult for checking out a booking
        /// </summary>
        /// <param name="id">id of booking</param>
        /// <returns>Booking check out view</returns>
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult CheckOut(int id)
        {
            try
            {
                //if check out function returns true (success)
                if (CheckOutBooking(id))
                {
                    //update success message in tempdata and return user returns
                    TempData["Success"] = "Booking Checked Out Successfully";
                    return RedirectToAction("Returns", "Users");
                }
                else
                {
                    //return returns view with error message
                    TempData["Error"] = "Error: Unable to check out booking.";
                    return RedirectToAction("Returns", "Users");
                }
            }
            catch (Exception ex)
            {
                //if exception occurs, return the returns view with error message
                TempData["Error"] = "Error: An error occured whilst checking out booking.";
                return RedirectToAction("Returns", "Users");
            }            
        }

        /// <summary>
        /// ActionResult for handling the event a customer does not show up for a booking
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Manage bookings view</returns>
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult NoShow(int id)
        {
            try
            {
                //find the booking using the id parameter
                Booking booking = db.Bookings.Find(id);

                //update the booking status to no show
                booking.BookingStatus = BookingStatus.NoShow;

                //save database changes
                db.SaveChanges();

                //update success message in tempdata and return the manage view
                TempData["Success"] = "Booking Successfully Marked As No Show";
                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                //if exception occurs
                //update error message in tempdata and return the manage view
                TempData["Error"] = "Booking Could Not Be Marked As No Show";
                return RedirectToAction("Manage");
            }
        }

        /// <summary>
        /// ActionResult for handling the event a Customer is delayed returning from their trip by increasing the booking stay by 1 day
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>Manage booking view</returns>
        [Authorize(Roles = "Admin, Manager, Invoice Clerk, Booking Clerk")]
        public ActionResult Delay(int id)
        {
            try
            {
                //find the booking via id parameter
                Booking booking = db.Bookings.Find(id);

                //add 1 additonal day to the booking return date
                booking.Flight.ReturnDate.AddDays(1);
                booking.Duration++; //update booking duration
                booking.BookingStatus = BookingStatus.Delayed;  //update booking status to delayed
                booking.Total = booking.Total + booking.Tariff.Amount;  //update booking total for additional day

                //save database changes
                db.SaveChanges();

                //get the new TimeRange for the booking
                TimeRange selectedTimeRange = new TimeRange(
                new DateTime(booking.Flight.DepartureDate.Year, booking.Flight.DepartureDate.Month, booking.Flight.DepartureDate.Day, booking.Flight.DepartureTime.Hours, booking.Flight.DepartureTime.Minutes, 0),
                new DateTime(booking.Flight.ReturnDate.Year, booking.Flight.ReturnDate.Month, booking.Flight.ReturnDate.Day, booking.Flight.ReturnFlightTime.Hours, booking.Flight.ReturnFlightTime.Minutes, 0));

                //if the next available slot does not equal 0 (if next available slot = 0 then no slot is available)
                if (FindAvailableParkingSlot(selectedTimeRange)!=0)
                {
                    //assign the booking a new parking slot
                    booking.ParkingSlot = db.ParkingSlots.Find(FindAvailableParkingSlot(selectedTimeRange));
                    db.SaveChanges();

                    //update success message and return to manage bookings view
                    TempData["Success"] = "Booking Delay Successfully Updated";
                    return RedirectToAction("Manage");
                }
                else
                {
                    //display error message
                    TempData["Error"] = "Booking could not be delayed as no parking slots are available";
                    return RedirectToAction("Manage");
                }

                
            }
            catch (Exception ex)
            {
                //if exception occurs
                //update error message in tempdata and return manage bookings view
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
            try
            {
                //loop through all bookings
                foreach (var booking in db.Bookings.ToList())
                {
                    //if the booking matches the booking id parameter
                    if (booking.ID == id)
                    {
                        //check booking in and update parking slot ststaus
                        booking.CheckedOut = false;
                        booking.CheckedIn = true;
                        booking.ParkingSlot.Status = Status.Occupied;
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                //if exception occurs, return false
                return false;
            }
            //if we get to this point, something went throw - return false
            return false;
        }

        /// <summary>
        /// Function for checking out a booking using booking id
        /// </summary>
        /// <param name="id">booking id</param>
        /// <returns>true or false</returns>
        private bool CheckOutBooking(int id)
        {
            try
            {
                //loop through all bookings
                foreach (var booking in db.Bookings.ToList())
                {
                    //if booking matches id parameter
                    if (booking.ID == id)
                    {
                        //check booking out and update parking slot status
                        booking.CheckedIn = false;
                        booking.CheckedOut = true;
                        booking.ParkingSlot.Status = Status.Available;
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                //if exception occurs, return false
                return false;
            }
            
            return false;
        }

        /// <summary>
        /// Function to calculate the duration of a booking using the start and end date
        /// </summary>
        /// <param name="departureDate">date of flight departure</param>
        /// <param name="returnDate">date of flight return</param>
        /// <returns>duration of booking in days</returns>
        private int CalculateBookingDuration(DateTime departureDate, DateTime returnDate)
        {
            try
            {
                //calculate the total duration of booking dates
                TimeSpan duration = returnDate.Subtract(departureDate);

                //return the total duration in integer number of days
                return Convert.ToInt32(duration.TotalDays);
            }
            catch (Exception ex)
            {
                //if exception occurs, return -1 to indicate error occured
                return -1;
            }            
        }

        /// <summary>
        /// Function to calculate the total cost of a booking using the start and end date
        /// </summary>
        /// <param name="departureDate">booking flight departure date</param>
        /// <param name="returnDate">booking flight return date</param>
        /// <returns>booking total</returns>
        private double CalculateBookingTotal(DateTime departureDate, DateTime returnDate)
        {
            try
            {
                //return the total cost of booking via the tariff price from database and booking duration
                return db.Tariffs.Find(1).Amount * Convert.ToInt32(CalculateBookingDuration(departureDate, returnDate));
            }
            catch (Exception ex)
            {
                //if exception occurs, return -1 to indicate error occured
                return -1;
            }            
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
