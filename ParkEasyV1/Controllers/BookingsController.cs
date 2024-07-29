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
using Rotativa;

namespace ParkEasyV1.Controllers
{
    public enum BookingRangeType
    {
        Permanent = 1,
        Temporary = 2
    }

    public class CreateBookingRequest
    {
        public int ParkingSlotFloor {  get; set; }
        public int ParkingSlotNumber { get; set; }
        public DateTime DepartureDate1 { get; set; }
        public DateTime ReturnDate {  get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeID { get; set; }
        public string VehicleMake {  get; set; }
        public string VehicleModel {  get; set; }
        public string VehicleColour { get; set; }
        public string VehicleRegistration {  get; set; }
        public BookingRangeType BookingRangeType { get; set; }
    }

    public class EditBookingRequest : CreateBookingRequest
    {
        public int BookingID { get; set; }
    }

    /// <summary>
    /// Controller for handling all booking events
    /// </summary>
    public class BookingsController : Controller
    {
        /// <summary>
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost]
        public JsonResult ClearBooking([ModelBinder(typeof(JsonNetModelBinder))] ClearBookingRequest _request)
        {
            var booking = db.Bookings.FirstOrDefault(b => b.ID == _request.BookingID);

            if (booking != null)
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

        [HttpPost]
        public JsonResult CreateBooking([ModelBinder(typeof(JsonNetModelBinder))] CreateBookingRequest _request)
        {
            try
            {
                if(string.IsNullOrEmpty(_request.FirstName))
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "First Name is required."
                    });
                }

                if (string.IsNullOrEmpty(_request.LastName))
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Last Name is required."
                    });
                }

                if (string.IsNullOrEmpty(_request.EmployeeID))
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Employee ID is required."
                    });
                }

                if(_request.ParkingSlotFloor < 1)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Invalid parking floor."
                    });
                }

                if(_request.ParkingSlotNumber < 1)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Invalid parking slot number."
                    });
                }

                _request.DepartureDate1 = DateTime.Now.Date + new TimeSpan(0, 0, 0);
                _request.ReturnDate = _request.ReturnDate.Date + new TimeSpan(23, 59, 59);

                if (_request.BookingRangeType == BookingRangeType.Temporary && _request.DepartureDate1 > _request.ReturnDate)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Invalid date range selected."
                    });
                }

                //get the TimeRange for the selected dates
                TimeRange selectedTimeRange = new TimeRange(_request.DepartureDate1, _request.ReturnDate);

                //create customer vehicle
                Vehicle vehicle = db.Vehicles.Add(new Vehicle()
                {
                    RegistrationNumber = _request.VehicleRegistration,
                    Make = _request.VehicleMake,
                    Model = _request.VehicleModel,
                    Colour = _request.VehicleColour,
                    NoOfPassengers = 0
                });

                //save database changes
                db.SaveChanges();

                var parkingSlot = db.ParkingSlots.Include(p => p.Bookings)
                .FirstOrDefault((ps) => ps.ParkingSlotNumber == _request.ParkingSlotNumber && ps.FloorNu == _request.ParkingSlotFloor);

                if (parkingSlot == null)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Selected Parking Slot is not found."
                    });
                }


                var isAvailable = IsParkingSlotAvailableByDateRange(parkingSlot, selectedTimeRange);

                if (!isAvailable)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Selected Parking Slot is not available."
                    });
                }

                //create customer booking
                Booking booking = db.Bookings.Add(new Booking()
                {
                    ParkingSlot = parkingSlot,
                    DateBooked = selectedTimeRange.Start,
                    DateBookingEnd = selectedTimeRange.End,
                    FirstName = _request.FirstName,
                    LastName = _request.LastName,
                    EmployeeID = _request.EmployeeID,
                    ReservationType = _request.BookingRangeType == BookingRangeType.Temporary ? ReservationType.DateRange : ReservationType.Permanent,
                    Vehicle = vehicle
                });

                //save database changes
                db.SaveChanges();

                return Json(new VoidResponse()
                {
                    IsSuccessful = true,
                    Message = ""
                });
            }
            catch (Exception ex)
            {
                return Json(new VoidResponse()
                {
                    IsSuccessful = false,
                    Message = "Something went wrong. Try again later."
                });
            }            
        }

        private bool IsParkingSlotAvailableByDateRange(ParkingSlot slot, TimeRange selectedTimeRange)
        {
            var booking = slot.Bookings.Where(b => 
            {   
                return (b.ReservationType == ReservationType.Permanent || new TimeRange(b.DateBooked, b.DateBookingEnd).OverlapsWith(selectedTimeRange)); 
            }).OrderByDescending(b => b.ID)
            .FirstOrDefault();
            return booking == null;
        }

        [HttpPost]
        public JsonResult EditBooking([ModelBinder(typeof(JsonNetModelBinder))] EditBookingRequest _request)
        {
            try
            {
                var booking = db.Bookings.Find(_request.BookingID);
                if (booking == null)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Booking is not found to edit."
                    });
                }

                if (string.IsNullOrEmpty(_request.FirstName))
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "First Name is required."
                    });
                }

                if (string.IsNullOrEmpty(_request.LastName))
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Last Name is required."
                    });
                }

                if (string.IsNullOrEmpty(_request.EmployeeID))
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Employee ID is required."
                    });
                }

                if (_request.ParkingSlotFloor < 1)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Invalid parking floor."
                    });
                }

                if (_request.ParkingSlotNumber < 1)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Invalid parking slot number."
                    });
                }

                _request.DepartureDate1 = DateTime.Now.Date + new TimeSpan(0, 0, 0);
                _request.ReturnDate = _request.ReturnDate.Date + new TimeSpan(23, 59, 59);

                if (_request.BookingRangeType == BookingRangeType.Temporary && _request.DepartureDate1 > _request.ReturnDate)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Invalid date range selected."
                    });
                }

                //get the TimeRange for the selected dates
                TimeRange selectedTimeRange = new TimeRange(_request.DepartureDate1, _request.ReturnDate);

                //get the vehicle linked to booking via bookingline
                Vehicle vehicle = booking.Vehicle;

                if (vehicle == null)
                {
                    vehicle = db.Vehicles.Add(new Vehicle()
                    {
                        RegistrationNumber = _request.VehicleRegistration,
                        Make = _request.VehicleMake,
                        Model = _request.VehicleModel,
                        Colour = _request.VehicleColour,
                        NoOfPassengers = 0
                    });
                }
                else
                {
                    vehicle.RegistrationNumber = _request.VehicleRegistration;
                    vehicle.Make = _request.VehicleMake;
                    vehicle.Model = _request.VehicleModel;
                    vehicle.Colour = _request.VehicleColour;
                }

                //save database changes
                db.SaveChanges();

                var parkingSlot = db.ParkingSlots.Include(p => p.Bookings)
                .FirstOrDefault((ps) => ps.ParkingSlotNumber == _request.ParkingSlotNumber && ps.FloorNu == _request.ParkingSlotFloor);

                if (parkingSlot == null)
                {
                    return Json(new VoidResponse()
                    {
                        IsSuccessful = false,
                        Message = "Selected Parking Slot is not found."
                    });
                }


                var isAvailable = IsParkingSlotAvailableByDateRange(parkingSlot, selectedTimeRange);

                if (!isAvailable)
                {
                    if (!(booking.ParkingSlot != null && booking.ParkingSlot.ParkingSlotNumber == _request.ParkingSlotNumber && booking.ParkingSlot.FloorNu == _request.ParkingSlotFloor))
                    {
                        return Json(new VoidResponse()
                        {
                            IsSuccessful = false,
                            Message = "Selected Parking Slot is not available."
                        });
                    }
                }

                booking.DateBooked = _request.DepartureDate1;
                booking.DateBookingEnd = _request.ReturnDate;
                booking.FirstName = _request.FirstName;
                booking.LastName = _request.LastName;
                booking.EmployeeID = _request.EmployeeID;
                booking.ReservationType = _request.BookingRangeType == BookingRangeType.Temporary ? ReservationType.DateRange : ReservationType.Permanent;

                //save database changes
                db.SaveChanges();

                return Json(new VoidResponse()
                {
                    IsSuccessful = true,
                    Message = ""
                });
            }
            catch (Exception ex)
            {
                return Json(new VoidResponse()
                {
                    IsSuccessful = false,
                    Message = "Something went wrong. Try again later."
                });
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
