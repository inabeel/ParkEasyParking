using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using Itenso.TimePeriod;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using ParkEasyV1.Models;

namespace ParkEasyV1.Controllers
{

    public class VehicleDataModel
    {
        public int ID { get; set; }

        /// <summary>
        /// vehicle registration number
        /// </summary>
        public string RegistrationNumber { get; set; }

        /// <summary>
        /// vehicle make
        /// </summary>
        public string Make { get; set; }

        /// <summary>
        /// vehicle model
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// vehicle colour
        /// </summary>
        public string Colour { get; set; }

        /// <summary>
        /// number of passengers travelling
        /// </summary>
        public int NoOfPassengers { get; set; }
    }

    public class BookingDataModel
    {
        public int ID { get; set; }

        public string EmployeeID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }
    }

    public class ParkingSlotDataModel
    {
        public int ID { get; set; }
        public int FloorNu {  get; set; }
        public int ParkingSlotNumber { get; set; }
        public Status Status { get; set; }
        public BookingDataModel BookingData { get; set; }
        public VehicleDataModel VehicleData {  get; set; }
    }

    public class VoidResponse
    {
        public bool IsSuccessful {  get; set; }
        public string Message { get; set; }
    }

    public class ChangeStatusRequest
    {
        public int SlotNumber { get; set; }
        public int FloorNumber { get; set; }
        public Status Status { get; set; }
    }

    public class ClearBookingRequest
    {
        public int BookingID { get; set; }
    }

    public class SearchParkingSlotsViewModel
    {
        /// <summary>
        /// booking start date
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        public DateTime DepartureDate1 { get; set; }


        [Required]
        [StringLength(10, ErrorMessage = "The {0} must be {2} characters long.", MinimumLength = 10)]
        [Display(Name = "Employee ID")]
        public string EmployeeID { get; set; }

        /// <summary>
        /// booking start time
        /// </summary>
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public TimeSpan DepartureTime { get; set; }

        /// <summary>
        /// booking end date
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "End Date")]
        public DateTime ReturnDate { get; set; }

        /// <summary>
        /// booking end time
        /// </summary>
        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public TimeSpan ReturnTime { get; set; }

        [Required]
        [RegularExpression("(.*[1-9].*)|(.*[.].*[1-9].*)", ErrorMessage = "Invalid Slot Number.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Floor Number.")]
        [Display(Name = "Selected Parking Slot Number")]
        public int ParkingSlotNumber { get; set; }

        [Required]
        [RegularExpression("(.*[1-9].*)|(.*[.].*[1-9].*)", ErrorMessage = "Invalid Floor Number.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Floor Number.")]
        [Display(Name = "Selected Parking Slot Floor")]
        public int ParkingSlotFloor { get; set; }
    }

    internal class JsonNetModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            controllerContext.HttpContext.Request.InputStream.Position = 0;
            var stream = controllerContext.RequestContext.HttpContext.Request.InputStream;
            var readStream = new StreamReader(stream, Encoding.UTF8);
            var json = readStream.ReadToEnd();
            return JsonConvert.DeserializeObject(json, bindingContext.ModelType);
        }
    }

    /// <summary>
    /// Controller to handle all Parking Slot actions and events
    /// </summary>
    public class ParkingSlotsController : Controller
    {
        /// <summary>
        /// Global instance of the ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        public JsonResult ChangeStatus([ModelBinder(typeof(JsonNetModelBinder))] ChangeStatusRequest _request)
        {
            var slot = db.ParkingSlots.FirstOrDefault(ps => ps.ParkingSlotNumber == _request.SlotNumber && ps.FloorNu == _request.FloorNumber);

            if (slot != null)
            {
                slot.Status = _request.Status;
                db.SaveChanges();
            }

            return Json(new VoidResponse() 
            {
                IsSuccessful = true,
                Message = ""
            });
        }

        [HttpPost]
        public JsonResult ClearBooking([ModelBinder(typeof(JsonNetModelBinder))] ClearBookingRequest _request)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.ID == _request.BookingID);

            if(booking != null)
            {
                db.Bookings.Remove(booking);
                db.SaveChanges();
            }

            return Json(new VoidResponse()
            {
                IsSuccessful = true,
                Message = ""
            });
        }

        public JsonResult GetParkingSlotsData(int floorNumber, DateTime dateStart, DateTime dateEnd, TimeSpan timeStart, TimeSpan timeEnd)
        {

            TimeRange selectedTimeRange = new TimeRange(
            new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, timeStart.Hours, timeStart.Minutes, 0),
            new DateTime(dateEnd.Year, dateEnd.Month, dateEnd.Day, timeEnd.Hours, timeEnd.Minutes, 0));

            var data = db.ParkingSlots
            .Where(ps => ps.FloorNu == floorNumber)
            .ToList()
            .Select((slot) =>
            {
                var lastActiveBooking = slot.Bookings.Where(b =>
                {
                    return (new TimeRange(b.DateBooked, b.DateBookingEnd).OverlapsWith(selectedTimeRange));
                }).OrderByDescending(b => b.ID)
                .FirstOrDefault();
                VehicleDataModel vehicleDataModel = null;
                if (lastActiveBooking != null)
                {
                        var vehicle = lastActiveBooking.BookingLines.First().Vehicle;
                        vehicleDataModel = new VehicleDataModel()
                        {
                            ID = vehicle.ID,
                            Colour = vehicle.Colour,
                            Make = vehicle.Make,
                            Model = vehicle.Model,
                            NoOfPassengers = vehicle.NoOfPassengers,
                            RegistrationNumber = vehicle.RegistrationNumber
                        };
                    }

                    return new ParkingSlotDataModel()
                    {
                        ID = slot.ID,
                        FloorNu = slot.FloorNu,
                        ParkingSlotNumber = slot.ParkingSlotNumber,
                        Status = slot.Status,
                        BookingData = lastActiveBooking == null ? null : new BookingDataModel() 
                        {
                            DateEnd = lastActiveBooking.DateBookingEnd,
                            FirstName = lastActiveBooking.FirstName,
                            DateStart = lastActiveBooking.DateBooked,
                            EmployeeID = lastActiveBooking.EmployeeID,
                            ID = lastActiveBooking.ID,
                            LastName = lastActiveBooking.LastName
                        },
                        VehicleData = vehicleDataModel
                    };
                });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// HttpGet ActionResult to return the ParkingSlot Index view
        /// </summary>
        /// <returns>Index view</returns>
        // GET: ParkingSlots
        public ActionResult Index()
        {
            //create instance of the usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the current logged in user 
            User loggedInUser = userManager.FindByEmail(User.Identity.GetUserName());

            //store user id in viewbag for front-end display
            ViewBag.UserID = loggedInUser.Id;
            var model = new SearchParkingSlotsViewModel();
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
            var endDate = startDate.AddMinutes(60);
            model.DepartureDate1 = startDate;
            model.DepartureTime = startDate.TimeOfDay;
            model.ReturnDate = endDate;
            model.ReturnTime = endDate.TimeOfDay;

            //return the full list of parking slots
            return View(model);
        }

        /// <summary>
        /// HttpGet ActionResult to return the Details view
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>Details view</returns>
        // GET: ParkingSlots/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            if (parkingSlot == null)
            {
                return HttpNotFound();
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpGet ActionResult to return the create parking slot view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: ParkingSlots/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to create a parking slot
        /// </summary>
        /// <param name="parkingSlot">Created parking slot</param>
        /// <returns>Parking slot index view</returns>
        // POST: ParkingSlots/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Status")] ParkingSlot parkingSlot)
        {
            if (ModelState.IsValid)
            {
                db.ParkingSlots.Add(parkingSlot);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(parkingSlot);
        }

        /// <summary>
        /// HttpGet ActionResult to return the edit parking slot view
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>Edit view</returns>
        // GET: ParkingSlots/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            if (parkingSlot == null)
            {
                return HttpNotFound();
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpPost ActionResult to update a parking slot
        /// </summary>
        /// <param name="parkingSlot">Updated parking slot</param>
        /// <returns>Index view</returns>
        // POST: ParkingSlots/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Status")] ParkingSlot parkingSlot)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parkingSlot).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpGet ActionResult to return the delete parking slot view
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>Delete view</returns>
        // GET: ParkingSlots/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            if (parkingSlot == null)
            {
                return HttpNotFound();
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpPost ActionResult to delete a parking slot
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>ParkingSlot index</returns>
        // POST: ParkingSlots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            db.ParkingSlots.Remove(parkingSlot);
            db.SaveChanges();
            return RedirectToAction("Index");
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
