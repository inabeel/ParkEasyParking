using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models.ViewModels
{
    public class CreateBookingViewModel
        {
            /// <summary>
            /// booking start date
            /// </summary>
            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Display(Name ="Departure Date")]
            [DateGreaterThan("ReturnDate")]
            [DateMoreThanOrEqualToToday]
            public DateTime DepartureDate { get; set; }

            /// <summary>
            /// booking start time
            /// </summary>
            [Required]
            [DataType(DataType.Time)]
            [Display(Name ="Departure Time")]
            public TimeSpan DepartureTime { get; set; }

            /// <summary>
            /// booking end date
            /// </summary>
            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Display(Name ="Return Date")]
            public DateTime ReturnDate { get; set; }

            /// <summary>
            /// booking end time
            /// </summary>
            [Required]
            [DataType(DataType.Time)]
            [Display(Name ="Return Time")]
            public TimeSpan ReturnTime { get; set; }

            /// <summary>
            /// booking departure flight number
            /// </summary>
            [Required]
            [StringLength(16, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Departure Flight Number")]
            public string DepartureFlightNo { get; set; }

            /// <summary>
            /// booking return flight number
            /// </summary>
            [Required]
            [StringLength(16, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Return Flight Number")]
            public string ReturnFlightNo { get; set; }

            /// <summary>
            /// trip destination
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Destination Airport")]
            public string DestinationAirport { get; set; }

            /// <summary>
            /// user first name
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            /// <summary>
            /// user last name
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            public string Surname { get; set; }

            /// <summary>
            /// user address line 1
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Address")]
            public string AddressLine1 { get; set; }

            /// <summary>
            /// user address line 2 - not required
            /// </summary>
            [Display(Name = "")]
            public string AddressLine2 { get; set; }

            /// <summary>
            /// user city
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            public string City { get; set; }

            /// <summary>
            /// user postcode
            /// </summary>
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
            [DataType(DataType.PostalCode)]
            public string Postcode { get; set; }

            /// <summary>
            /// user email
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            /// user phone number
            /// </summary>
            [Required]
            [Phone]
            [Display(Name ="Contact Phone Number")]
            public string PhoneNo { get; set; }

            /// <summary>
            /// vehicle make
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name ="Vehicle Make")]
            public string VehicleMake { get; set; }

            /// <summary>
            /// vehicle model
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name ="Vehicle Model")]
            public string VehicleModel { get; set; }

            /// <summary>
            /// vehicle colour
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name ="Vehicle Colour")]
            public string VehicleColour { get; set; }

            /// <summary>
            /// vehicle registration number
            /// </summary>
            [Required]
            [StringLength(16, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name ="Vehicle Registration")]
            public string VehicleRegistration { get; set; }

            /// <summary>
            /// number of passengers travelling
            /// </summary>
            [Required]
            [RegularExpression("(.*[1-9].*)|(.*[.].*[1-9].*)", ErrorMessage ="Number of passengers cannot be 0")]
            [Display(Name ="Number of Passengers")]
            public int NoOfPassengers { get; set; }

            /// <summary>
            /// terms of service acceptance boolean
            /// </summary>
            [Required]
            [MustBeTrue(ErrorMessage = "You must agree to the ParkEasy Terms & Conditions")]
            public bool TOS { get; set; }
        }

        /// <summary>
        /// View model for displaying all data associated with a booking
        /// </summary>
        public class ViewBookingViewModel
        {
            /// <summary>
            /// booking id - hidden from user on front-end
            /// </summary>
            public int BookingID { get; set; }

            /// <summary>
            /// booking start date
            /// </summary>
            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Display(Name = "Departure Date")]
            public DateTime DepartureDate { get; set; }

            /// <summary>
            /// booking start time
            /// </summary>
            [Required]
            [DataType(DataType.Time)]
            [Display(Name = "Departure Time")]
            public TimeSpan DepartureTime { get; set; }

            /// <summary>
            /// booking end date
            /// </summary>
            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Display(Name = "Return Date")]
            public DateTime ReturnDate { get; set; }

            /// <summary>
            /// booking end time
            /// </summary>
            [Required]
            [DataType(DataType.Time)]
            [Display(Name = "Return Time")]
            public TimeSpan ReturnTime { get; set; }

            /// <summary>
            /// length of booking
            /// </summary>
            [Display(Name ="Booking Length")]
            public int Duration { get; set; }

            /// <summary>
            /// total cost of booking
            /// </summary>
            public double Total { get; set; }

            /// <summary>
            /// boolean to hold if valet service is selected
            /// </summary>
            public bool Valet { get; set; }

            /// <summary>
            /// enumeration selected booking status
            /// </summary>
            public BookingStatus Status { get; set; }

            /// <summary>
            /// user first name
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            /// <summary>
            /// user last name
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            public string Surname { get; set; }

            /// <summary>
            /// user address line 1
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Address")]
            public string AddressLine1 { get; set; }

            /// <summary>
            /// user address line 2 - not required
            /// </summary>
            [Display(Name = "")]
            public string AddressLine2 { get; set; }

            /// <summary>
            /// user city
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            public string City { get; set; }

            /// <summary>
            /// user postcode
            /// </summary>
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 7)]
            [DataType(DataType.PostalCode)]
            public string Postcode { get; set; }

            /// <summary>
            /// user email
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            /// user phone number
            /// </summary>
            [Required]
            [Phone]
            [Display(Name = "Contact Phone Number")]
            public string PhoneNo { get; set; }

            /// <summary>
            /// vehicle make
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Vehicle Make")]
            public string VehicleMake { get; set; }

            /// <summary>
            /// vehicle model
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Vehicle Model")]
            public string VehicleModel { get; set; }

            /// <summary>
            /// vehicle colour
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Vehicle Colour")]
            public string VehicleColour { get; set; }

            /// <summary>
            /// vehicle registration number
            /// </summary>
            [Required]
            [StringLength(16, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
            [Display(Name = "Vehicle Registration")]
            public string VehicleRegistration { get; set; }

            /// <summary>
            /// number of passengers travelling
            /// </summary>
            [Required]
            [Display(Name = "Number of Passengers")]
            public int NoOfPassengers { get; set; }
        }

        /// <summary>
        /// view model for checking availability of a booking
        /// </summary>
        public class AvailabilityViewModel
        {
            /// <summary>
            /// booking start date
            /// </summary>
            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Display(Name = "Departure Date")]
            [DateMoreThanOrEqualToToday]
            [DateGreaterThan("ReturnDate")]
            public DateTime DepartureDate { get; set; }
        
            /// <summary>
            /// booking start time
            /// </summary>
            [Required]
            [DataType(DataType.Time)]
            [Display(Name = "Departure Time")]
            public TimeSpan DepartureTime { get; set; }

            /// <summary>
            /// booking end date
            /// </summary>
            [Required]
            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
            [Display(Name = "Return Date")]
            public DateTime ReturnDate { get; set; }

            /// <summary>
            /// booking end time
            /// </summary>
            [Required]
            [DataType(DataType.Time)]
            [Display(Name = "Return Time")]
            public TimeSpan ReturnTime { get; set; }
        }
}   