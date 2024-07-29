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

        public BookingRangeType BookingRangeType { get; set; }
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
        [Display(Name = "Start Date")]
        public DateTime DepartureDate1 { get; set; }

        [Display(Name = "End Date")]
        public DateTime ReturnDate { get; set; }
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

        [HttpGet]
        public JsonResult GetParkingSlotsData(int floorNumber)
        {
            var data = db.ParkingSlots
            .Where(ps => ps.FloorNu == floorNumber)
            .ToList()
            .Select((slot) =>
            {
                // get last not ended booking
                var lastActiveBooking = slot.Bookings.Where(b =>
                {
                    return b.DateBookingEnd > DateTime.Now;
                }).OrderByDescending(b => b.ID)
                .FirstOrDefault();
                VehicleDataModel vehicleDataModel = null;
                if (lastActiveBooking != null)
                {
                        var vehicle = lastActiveBooking.Vehicle;
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
                            LastName = lastActiveBooking.LastName,
                            BookingRangeType = lastActiveBooking.ReservationType == ReservationType.Permanent ? BookingRangeType.Permanent : BookingRangeType.Temporary
                        },
                        VehicleData = vehicleDataModel
                    };
                });

            return Json(data, JsonRequestBehavior.AllowGet);
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
