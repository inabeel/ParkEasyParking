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
            var parkingSlotId = 1;
            for (int floorNumber = 1; floorNumber <= 3 ; floorNumber++)
            {
                switch (floorNumber)
                {
                    case 1:
                        for (int parkingSlotNumber = 1; parkingSlotNumber <= 155; parkingSlotNumber++)
                        {
                            //create new available parking slot
                            context.ParkingSlots.Add(new ParkingSlot()
                            {
                                ID = parkingSlotId,
                                FloorNu = floorNumber,
                                Status = Status.Available,
                                ParkingSlotNumber = parkingSlotNumber
                            });
                            parkingSlotId++;
                        }
                        break;
                    case 2:
                        for (int parkingSlotNumber = 1; parkingSlotNumber <= 160; parkingSlotNumber++)
                        {
                            //create new available parking slot
                            context.ParkingSlots.Add(new ParkingSlot()
                            {
                                ID = parkingSlotId,
                                FloorNu = floorNumber,
                                Status = Status.Available,
                                ParkingSlotNumber = parkingSlotNumber
                            });
                            parkingSlotId++;
                        }
                        break;
                    case 3:
                        for (int parkingSlotNumber = 1; parkingSlotNumber <= 178; parkingSlotNumber++)
                        {
                            //create new available parking slot
                            context.ParkingSlots.Add(new ParkingSlot()
                            {
                                ID = parkingSlotId,
                                FloorNu = floorNumber,
                                Status = Status.Available,
                                ParkingSlotNumber = parkingSlotNumber
                            });
                            parkingSlotId++;
                        }
                        break;
                    default:
                        break;
                }
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

            CreateFutureBooking(context);
        }

        /// <summary>
        /// Method to create and seed a booking that will occur in the future in the system
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateFutureBooking(ApplicationDbContext context)
        {

            CreateBookingToday(context);
        }

        /// <summary>
        /// Method to create and seed a booking that occurs at the current datetime
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateBookingToday(ApplicationDbContext context)
        {
         
            CreateCorporateBooking(context);
        }

        /// <summary>
        /// Method to create and seed a corporate booking
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateCorporateBooking(ApplicationDbContext context)
        {
     
            BookingCheckInToday(context);
        }

        /// <summary>
        /// Method to create and seed a booking that occurs at the current datetime and requires checked in
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void BookingCheckInToday(ApplicationDbContext context)
        {
            //create instance of user manager
  
            BookingCheckOutToday(context);
        }

        /// <summary>
        /// Method to create and seed a booking that is ending today and requires checked out
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void BookingCheckOutToday(ApplicationDbContext context)
        {
            
            CorporateBookingTomorrow(context);
        }

        /// <summary>
        /// Method to create and seed a corporate booking scheduled for tomorrow
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CorporateBookingTomorrow(ApplicationDbContext context)
        {
    
            CorporateBooking48HrsAhead(context);
        }

        /// <summary>
        /// Method to create and seed a corporate booking scheduled 48 hours ahead of the current date
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CorporateBooking48HrsAhead(ApplicationDbContext context)
        {
    
            //save changes
            context.SaveChanges();
        }


    }
}