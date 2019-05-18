using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    public class DatabaseInitialiser : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

            if (!context.Users.Any())
            {

                RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                if (!roleManager.RoleExists("Admin"))
                {
                    roleManager.Create(new IdentityRole("Admin"));
                }
                if (!roleManager.RoleExists("Manager"))
                {
                    roleManager.Create(new IdentityRole("Manager"));

                }
                if (!roleManager.RoleExists("Invoice Clerk"))
                {
                    roleManager.Create(new IdentityRole("Invoice Clerk"));
                }
                if (!roleManager.RoleExists("Booking Clerk"))
                {
                    roleManager.Create(new IdentityRole("Booking Clerk"));
                }
                if (!roleManager.RoleExists("Customer"))
                {
                    roleManager.Create(new IdentityRole("Customer"));
                }

                context.SaveChanges();

                //Create users

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
                if (userManager.FindByName("graham.cadger@cityofglacol.ac.uk") == null)
                {
                    var customer = new Customer
                    {
                        UserName = "graham.cadger@cityofglacol.ac.uk",
                        Email = "graham.cadger@cityofglacol.ac.uk",
                        RegistrationDate = DateTime.Now,
                        EmailConfirmed = true,
                        FirstName = "Graham",
                        LastName = "Cadger",
                        AddressLine1 = "190 Cathedral Street",
                        AddressLine2 = "Glasgow",
                        City = "Glasgow",
                        Postcode = "G4 0RF",
                        Corporate = true
                    };
                    userManager.Create(customer, "grahamcadger");
                    userManager.AddToRoles(customer.Id, "Customer");
                }



                context.SaveChanges();

                //call to seed method
                //context save changes

                CreateParkingSlots(context);
                CreateTariffs(context);
                //CreateVehicles(context);
                //CreateFlights(context);
                CreateBookings(context);
                //CreatePayments(context);
                

            }
        }//end method

        private void CreateParkingSlots(ApplicationDbContext context)
        {
            for (int i = 0; i < 150; i++)
            {
                context.ParkingSlots.Add(new ParkingSlot()
                {
                    ID = i,
                    Status = Status.Available
                });
            }

            context.SaveChanges();
        }

        private void CreateTariffs(ApplicationDbContext context)
        {
            context.Tariffs.Add(new Tariff()
            {
                ID = 1,
                Type = "Parking Slot",
                Amount = 4.96
            });

            context.Tariffs.Add(new Tariff()
            {
                ID = 2,
                Type = "Full Valet",
                Amount = 20.00
            });

            context.Tariffs.Add(new Tariff()
            {
                ID = 3,
                Type = "Mini Valet",
                Amount = 10.00
            });

            context.SaveChanges();
        }

        private void CreateBookings(ApplicationDbContext context)
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(context));

            //CREATE NORMAL BOOKING

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

            Flight flight = context.Flights.Find(1);
            Tariff tariff = context.Tariffs.Find(1);
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
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

                //add booking lines
                BookingLines = new List<BookingLine>()
                {
                    new BookingLine() {Booking = context.Bookings.Find(1), Vehicle = context.Vehicles.Find(1)},
                },
            });

            //ParkingSlot slot = context.ParkingSlots.Find(99);
            //slot.Status = Status.Available;

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

            context.SaveChanges();
            CreateFutureBooking(context);
        }

        private void CreateFutureBooking(ApplicationDbContext context)
        {
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

            Flight flight = context.Flights.Find(2);
            Tariff tariff = context.Tariffs.Find(2);
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 10;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                Flight = context.Flights.Find(2),
                ParkingSlot = context.ParkingSlots.Find(100),
                Tariff = context.Tariffs.Find(2),

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

            ParkingSlot slot = context.ParkingSlots.Find(100);
            slot.Status = Status.Reserved;

            //create customer payment
            context.Payments.Add(new Cash()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                PaymentDate = DateTime.Now,
                Amount = price,
            });

            context.SaveChanges();
            CreateBookingToday(context);
        }

        private void CreateBookingToday(ApplicationDbContext context)
        {
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
                DepartureDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day),
                ReturnDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day+3),
                DestinationAirport = "Portugal"
            });

            Flight flight = context.Flights.Find(3);
            Tariff tariff = context.Tariffs.Find(3);
            TimeSpan duration = flight.ReturnDate - flight.DepartureDate;
            double price = tariff.Amount * Convert.ToInt32(duration.TotalDays) + 20;

            //create customer booking
            context.Bookings.Add(new Booking()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                Flight = context.Flights.Find(3),
                ParkingSlot = context.ParkingSlots.Find(101),
                Tariff = context.Tariffs.Find(3),

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
                    new BookingLine() {Booking = context.Bookings.Find(3), Vehicle = context.Vehicles.Find(3)},
                },
            });

            ParkingSlot slot = context.ParkingSlots.Find(101);
            slot.Status = Status.Reserved;

            //create customer payment
            context.Payments.Add(new Cash()
            {
                User = userManager.FindByEmail("john@gmail.com"),
                PaymentDate = DateTime.Now,
                Amount = price,
            });

            context.SaveChanges();
        }

        
    }
}