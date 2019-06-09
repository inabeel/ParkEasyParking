using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class used to initialize the database and seed data using the DropCreateDatabaseAlways method for testing
    /// Updated: Using DropCreatDatabaseIfModelChanges for live version
    /// </summary>
    public class DatabaseInitialiser : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        /// <summary>
        /// Override Seed method to seed the database with values
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(ApplicationDbContext context)
        {
            //call to base with ApplicationDbContext
            base.Seed(context);

            if (!context.Users.Any())
            {
                //create instance of role manager
                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                //if Admin role does not exist
                if (!roleManager.RoleExists("Admin"))
                {
                    //create admin role
                    roleManager.Create(new IdentityRole("Admin"));
                }
                //if manager role does not exist
                if (!roleManager.RoleExists("Manager"))
                {
                    //create manager role
                    roleManager.Create(new IdentityRole("Manager"));
                }
                //if invoice clerk role does not exist
                if (!roleManager.RoleExists("Invoice Clerk"))
                {
                    //create invoice clerk role
                    roleManager.Create(new IdentityRole("Invoice Clerk"));
                }
                //if booking clerk role does not exist
                if (!roleManager.RoleExists("Booking Clerk"))
                {
                    //create booking clerk role
                    roleManager.Create(new IdentityRole("Booking Clerk"));
                }
                //if customer role does not exist
                if (!roleManager.RoleExists("Customer"))
                {
                    //create customer role
                    roleManager.Create(new IdentityRole("Customer"));
                }

                //save changes
                context.SaveChanges();

                //Create users

                //create instance of user manager
                UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));


                //Create an Admin
                if (userManager.FindByName("admin@parkeasy.co.uk") == null)
                {
                    // Super liberal password validation for password for seeds
                    userManager.PasswordValidator = new PasswordValidator
                    {
                        RequireDigit = false,
                        RequiredLength = 1,
                        RequireLowercase = false,
                        RequireNonLetterOrDigit = false,
                        RequireUppercase = false,
                    };

                    var administrator = new Staff
                    {
                        UserName = "admin@parkeasy.co.uk",
                        Email = "admin@parkeasy.co.uk",
                        EmailConfirmed = true,
                        FirstName = "Allistair",
                        LastName = "McCoist",
                        AddressLine1 = "1972 Ibrox Lane",
                        AddressLine2 = "Govan",
                        City = "Glasgow",
                        Postcode = "G70 9RO",
                        JobTitle = "System Admin",
                        CurrentQualification = "BSc (Hons) Computing Science",
                        EmergencyContactName = "Jane Doe",
                        EmergencyContactPhoneNo = "03117770202"
                    };
                    userManager.Create(administrator, "admin123");
                    userManager.AddToRole(administrator.Id, "Admin");
                }


                //Create a manager
                if (userManager.FindByName("manager@parkeasy.co.uk") == null)
                {
                    var manager = new Staff
                    {
                        UserName = "manager@parkeasy.co.uk",
                        Email = "manager@parkeasy.co.uk",
                        EmailConfirmed = true,
                        FirstName = "Edward",
                        LastName = "Snowden",
                        AddressLine1 = "55 Turnbull Avenue",
                        AddressLine2 = "Rutherglen",
                        City = "Glasgow",
                        Postcode = "G73 5TD",
                        JobTitle = "Duty Manager",
                        CurrentQualification = "BSc (Hons) Business Management",
                        EmergencyContactName = "John Doe",
                        EmergencyContactPhoneNo = "05527778395"

                    };
                    userManager.Create(manager, "manager");
                    userManager.AddToRole(manager.Id, "Manager");
                }

                // Create invoice clerk.
                if (userManager.FindByName("invoiceclerk@parkeasy.co.uk") == null)
                {

                    var invoiceclerk = new Staff
                    {
                        UserName = "invoiceclerk@parkeasy.co.uk",
                        Email = "invoiceclerk@parkeasy.co.uk",
                        EmailConfirmed = true,
                        FirstName = "Helen",
                        LastName = "Williamson",
                        AddressLine1 = "2 Hilton Street",
                        AddressLine2 = "East Kilbride",
                        City = "Glasgow",
                        Postcode = "G74 3RQ",
                        JobTitle = "Invoice Clerk",
                        CurrentQualification = "HND Accounting",
                        EmergencyContactName = "Michael McIntyre",
                        EmergencyContactPhoneNo = "010154540088"
                    };

                    userManager.Create(invoiceclerk, "invoiceclerk");
                    userManager.AddToRoles(invoiceclerk.Id, "Invoice Clerk");
                }

                // Create Booking Clerk
                if (userManager.FindByName("bookingclerk@parkeasy.co.uk") == null)
                {
                    var bookingclerk = new Staff
                    {
                        UserName = "bookingclerk@parkeasy.co.uk",
                        Email = "bookingclerk@parkeasy.co.uk",
                        EmailConfirmed = true,
                        FirstName = "Lucy",
                        LastName = "McDonald",
                        AddressLine1 = "45 Richmond Road",
                        AddressLine2 = "Burnside",
                        City = "Glasgow",
                        Postcode = "G73 8JD",
                        JobTitle = "Booking Clerk",
                        CurrentQualification = "BA (Hons) Business & Finance",
                        EmergencyContactName = "Harry Hill",
                        EmergencyContactPhoneNo = "01610449124"
                    };
                    userManager.Create(bookingclerk, "bookingclerk");
                    userManager.AddToRoles(bookingclerk.Id, "Booking Clerk");
                }

                //Create Customer
                if (userManager.FindByName("john@gmail.com") == null)
                {
                    var customer = new Customer
                    {
                        UserName = "john@gmail.com",
                        Email = "john@gmail.com",
                        RegistrationDate = DateTime.Now,
                        EmailConfirmed = true,
                        FirstName = "John",
                        LastName = "Smith",
                        AddressLine1 = "19 Brown Road",
                        AddressLine2 = "Calton",
                        City = "Glasgow",
                        Postcode = "G66 9PT",
                        Corporate = false
                    };
                    userManager.Create(customer, "john");
                    userManager.AddToRoles(customer.Id, "Customer");
                }

                //Create Corporate Customer
                if (userManager.FindByName("stevengerrard@rangersrfc.com") == null)
                {
                    var customer = new Customer
                    {
                        UserName = "stevengerrard@rangersrfc.com",
                        Email = "stevengerrard@rangersrfc.com",
                        RegistrationDate = DateTime.Now,
                        EmailConfirmed = true,
                        FirstName = "Steven",
                        LastName = "Gerrard",
                        AddressLine1 = "150 Edminston Drive",
                        AddressLine2 = "Govan",
                        City = "Glasgow",
                        Postcode = "G51 2XD",
                        Corporate = true
                    };
                    userManager.Create(customer, "rangers");
                    userManager.AddToRoles(customer.Id, "Customer");
                }


                //save changes
                context.SaveChanges();

                //Call to method to create the 150 initial parking slots
                CreateParkingSlots(context);

                //Call to method to create the initial 3 types of Tariff
                CreateTariffs(context);

                //Call to method to seed initial bookings in the system
                CreateBookings(context);
                

            }
        }//end method

        /// <summary>
        /// Method to create initial 150 parking slots in the system
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateParkingSlots(ApplicationDbContext context)
        {
            //loop 150 times
            for (int i = 0; i < 150; i++)
            {
                //create new available parking slot
                context.ParkingSlots.Add(new ParkingSlot()
                {
                    ID = i,
                    Status = Status.Available
                });
            }

            //save changes
            context.SaveChanges();
        }

        /// <summary>
        /// Method to create initial 3 tariffs in the system
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateTariffs(ApplicationDbContext context)
        {
            //Create parking slot tariff and set price
            context.Tariffs.Add(new Tariff()
            {
                ID = 1,
                Type = "Parking Slot",
                Amount = 4.96
            });

            //Create full valet tariff and set price
            context.Tariffs.Add(new Tariff()
            {
                ID = 2,
                Type = "Full Valet",
                Amount = 20.00
            });

            //Create mini valet tariff and set price
            context.Tariffs.Add(new Tariff()
            {
                ID = 3,
                Type = "Mini Valet",
                Amount = 10.00
            });

            //save changes
            context.SaveChanges();
        }

        /// <summary>
        /// Method to create and seed initial bookings in the system
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateBookings(ApplicationDbContext context)
        {
            //create instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 1,
                RegistrationNumber = "CH66 SCD",
                Make = "Renault",
                Model = "Clio",
                Colour = "White",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 1,
                DepartureFlightNo = "TAX3663",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "TAX3664",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = new DateTime(2019, 5, 1),
                ReturnDate = new DateTime(2019, 5, 9),
                DestinationAirport = "Barcelona"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(1);
            //get tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate the duration of the booking
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the price of the booking
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays);

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                Flight = context.Flights.Find(1),
                ParkingSlot = context.ParkingSlots.Find(99),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = false,
                CheckedIn = true,
                CheckedOut = true,

                //add booking line
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(1), Vehicle = context.Vehicles.Find(1)},
                },
            });

            //create customer payment
            context.Payments.Add(new Card()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                PaymentDate = DateTime.Now,
                Amount = price,
                Type = CardType.Visa,
                CardNumber = "8377190066956388",
                NameOnCard = "Mr John A Smith",
                ExpiryDate = new DateTime(2020,07,1).AddDays(-1),
                CVV = 377
            });

            //save changes
            context.SaveChanges();
            //Call method to create and seed a future booking in the system
            CreateFutureBooking(context);
        }

        /// <summary>
        /// Method to create and seed a booking that will occur in the future in the system
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateFutureBooking(ApplicationDbContext context)
        {
            //create instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE FUTURE BOOKING

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 2,
                RegistrationNumber = "SL57 XSD",
                Make = "Nissan",
                Model = "Note",
                Colour = "Grey",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 2,
                DepartureFlightNo = "RED55",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "RED77",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = new DateTime(2019, 5, 25),
                ReturnDate = new DateTime(2019, 5, 29),
                DestinationAirport = "Tenerife"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(2);
            //get the tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate duration of booking
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the booking cost
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 10;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                Flight = context.Flights.Find(2),
                ParkingSlot = context.ParkingSlots.Find(100),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = true,
                CheckedIn = false,
                CheckedOut = false,

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(2), Vehicle = context.Vehicles.Find(2)},
                },
            });

            //create customer payment
            context.Payments.Add(new Cash()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                PaymentDate = DateTime.Now,
                Amount = price,
            });

            //save changes
            context.SaveChanges();
            //call method to create a booking that occurs today
            CreateBookingToday(context);
        }

        /// <summary>
        /// Method to create and seed a booking that occurs at the current datetime
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateBookingToday(ApplicationDbContext context)
        {
            //create instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE BOOKING TODAY

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 3,
                RegistrationNumber = "RFC 1972",
                Make = "Bently",
                Model = "Continental",
                Colour = "White",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 3,
                DepartureFlightNo = "FID99",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "FID98",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = DateTime.Today,
                ReturnDate = DateTime.Today.AddDays(7),
                DestinationAirport = "Portugal"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(3);
            //get the tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate booking duration
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the booking cost
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 20;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                Flight = context.Flights.Find(3),
                ParkingSlot = context.ParkingSlots.Find(1),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = true,
                CheckedIn = true,
                CheckedOut = false,

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(3), Vehicle = context.Vehicles.Find(3)},
                },
            });

            //find the parking slot allocated to the booking
            ParkingSlot slot = context.ParkingSlots.Find(1);
            //set parking slot status to occupied - as this seeded booking is occuring today and has been checked in
            slot.Status = Status.Occupied;

            //create customer payment
            context.Payments.Add(new Cash()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                PaymentDate = DateTime.Now,
                Amount = price,
            });

            //save changes
            context.SaveChanges();
            //call method to create and seed a corporate booking
            CreateCorporateBooking(context);
        }

        /// <summary>
        /// Method to create and seed a corporate booking
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateCorporateBooking(ApplicationDbContext context)
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE CORPORATE BOOKING

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 4,
                RegistrationNumber = "SF19 RFC",
                Make = "Jaguar",
                Model = "XF",
                Colour = "Silver",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 4,
                DepartureFlightNo = "BA771",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "BA772",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = new DateTime(2019, 6, 24),
                ReturnDate = new DateTime(2019, 7, 01),
                DestinationAirport = "Dubai"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(4);
            //get the tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate the booking duration
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the booking cost
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 20;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("stevengerrard@rangersrfc.com"),
                Flight = context.Flights.Find(4),
                ParkingSlot = context.ParkingSlots.Find(102),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = true,
                CheckedIn = false,
                CheckedOut = false,

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(4), Vehicle = context.Vehicles.Find(4)},
                },
            });

            
            //save changes
            context.SaveChanges();
            BookingCheckInToday(context);
        }

        /// <summary>
        /// Method to create and seed a booking that occurs at the current datetime and requires checked in
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void BookingCheckInToday(ApplicationDbContext context)
        {
            //create instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE BOOKING TODAY

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 5,
                RegistrationNumber = "GTR19A",
                Make = "Audi",
                Model = "TT",
                Colour = "White",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 5,
                DepartureFlightNo = "LAX72",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "LAX71",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = DateTime.Today,
                ReturnDate = DateTime.Today.AddDays(7),
                DestinationAirport = "LA X"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(5);
            //get the tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate booking duration
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the booking cost
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 20;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                Flight = context.Flights.Find(5),
                ParkingSlot = context.ParkingSlots.Find(2),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = true,
                CheckedIn = false,
                CheckedOut = false,

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(5), Vehicle = context.Vehicles.Find(5)},
                },
            });

            //find the parking slot allocated to the booking
            ParkingSlot slot = context.ParkingSlots.Find(2);

            //create customer payment
            context.Payments.Add(new Cash()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                PaymentDate = DateTime.Now,
                Amount = price,
            });

            //save changes
            context.SaveChanges();
            BookingCheckOutToday(context);
        }

        /// <summary>
        /// Method to create and seed a booking that is ending today and requires checked out
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void BookingCheckOutToday(ApplicationDbContext context)
        {
            //create instance of user manager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE BOOKING TODAY

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 6,
                RegistrationNumber = "J0HN1",
                Make = "Mercedes",
                Model = "A-Class",
                Colour = "Silver",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 6,
                DepartureFlightNo = "LON70",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "LON71",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = DateTime.Today.AddDays(-7),
                ReturnDate = DateTime.Today,
                DestinationAirport = "London Heathrow"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(6);
            //get the tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate booking duration
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the booking cost
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 20;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                Flight = context.Flights.Find(6),
                ParkingSlot = context.ParkingSlots.Find(13),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = true,
                CheckedIn = true,
                CheckedOut = false,

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(6), Vehicle = context.Vehicles.Find(6)},
                },
            });

            //find the parking slot allocated to the booking
            ParkingSlot slot = context.ParkingSlots.Find(13);
            slot.Status = Status.Occupied;

            //create customer payment
            context.Payments.Add(new Cash()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                PaymentDate = DateTime.Now,
                Amount = price,
            });

            //save changes
            context.SaveChanges();
            CorporateBookingTomorrow(context);
        }

        /// <summary>
        /// Method to create and seed a corporate booking scheduled for tomorrow
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CorporateBookingTomorrow(ApplicationDbContext context)
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE CORPORATE BOOKING

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 7,
                RegistrationNumber = "DT19 LFT",
                Make = "Porsche",
                Model = "911",
                Colour = "White",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 7,
                DepartureFlightNo = "FR882",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "FR883",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = DateTime.Today.AddDays(1),
                ReturnDate = DateTime.Today.AddDays(8),
                DestinationAirport = "Madrid"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(7);
            //get the tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate the booking duration
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the booking cost
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 20;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("stevengerrard@rangersrfc.com"),
                Flight = context.Flights.Find(7),
                ParkingSlot = context.ParkingSlots.Find(4),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = true,
                CheckedIn = false,
                CheckedOut = false,

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(7), Vehicle = context.Vehicles.Find(7)},
                },
            });


            //save changes
            context.SaveChanges();
            CorporateBooking48HrsAhead(context);
        }

        /// <summary>
        /// Method to create and seed a corporate booking scheduled 48 hours ahead of the current date
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CorporateBooking48HrsAhead(ApplicationDbContext context)
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE CORPORATE BOOKING

            //create customer vehicle
            context.Vehicles.Add(new Vehicle()
            {
                ID = 8,
                RegistrationNumber = "LCP19 PXD",
                Make = "Ferarri",
                Model = "Aventador",
                Colour = "Blue",
                NoOfPassengers = 2
            });

            //create customer flight
            context.Flights.Add(new Flight()
            {
                ID = 8,
                DepartureFlightNo = "BK222",
                DepartureTime = new TimeSpan(09, 00, 00),
                ReturnFlightNo = "BK223",
                ReturnFlightTime = new TimeSpan(10, 00, 00),
                DepartureDate = DateTime.Today.AddDays(2),
                ReturnDate = DateTime.Today.AddDays(9),
                DestinationAirport = "Baku"
            });

            //get the previously inserted flight from database
            Flight flight = context.Flights.Find(8);
            //get the tariff from database
            Tariff tariff = context.Tariffs.Find(1);
            //calculate the booking duration
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            //calculate the booking cost
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 20;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("stevengerrard@rangersrfc.com"),
                Flight = context.Flights.Find(8),
                ParkingSlot = context.ParkingSlots.Find(5),
                Tariff = context.Tariffs.Find(1),

                DateBooked = DateTime.Now,
                Duration = Convert.ToInt32(duration.TotalDays),
                Total = price,
                BookingStatus = BookingStatus.Confirmed,
                ValetService = true,
                CheckedIn = false,
                CheckedOut = false,

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(8), Vehicle = context.Vehicles.Find(8)},
                },
            });


            //save changes
            context.SaveChanges();
        }


    }
}