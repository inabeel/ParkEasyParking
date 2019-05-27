using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// ApplicationDbContext class to provide access to database tables
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User>
    {        
        /// <summary>
        /// Default constructor that specifies the DB connection and sets the initialiser
        /// </summary>
        public ApplicationDbContext()
           : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DatabaseInitialiser());
        }

        /// <summary>
        /// DbSet to hold all Bookings
        /// </summary>
        public DbSet<Booking> Bookings { get; set; }

        /// <summary>
        /// DbSet to hold all Vehicles
        /// </summary>
        public DbSet<Vehicle> Vehicles { get; set; }

        /// <summary>
        /// DbSet to hold all Flights
        /// </summary>
        public DbSet<Flight> Flights { get; set; }

        /// <summary>
        /// DbSet to hold all Parking Slots
        /// </summary>
        public DbSet<ParkingSlot> ParkingSlots { get; set; }

        /// <summary>
        /// DbSet to hold all Payments
        /// </summary>
        public DbSet<Payment> Payments { get; set; }

        /// <summary>
        /// DbSet to hold all Tariffs
        /// </summary>
        public DbSet<Tariff> Tariffs { get; set; }

        /// <summary>
        /// DbSet to hold all Booking Lines
        /// </summary>
        public DbSet<BookingLine> BookingLines { get; set; }

        /// <summary>
        /// DbSet to hold all Invoices
        /// </summary>
        public DbSet<Invoice> Invoices { get; set; }

        /// <summary>
        /// Function to build the ApplicationDbContext
        /// </summary>
        /// <returns>ApplicationDbContext</returns>
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }        
    }
}