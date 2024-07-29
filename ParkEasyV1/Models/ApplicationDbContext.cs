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
    public class ApplicationDbContext : DbContext
    {        
        /// <summary>
        /// Default constructor that specifies the DB connection and sets the initialiser
        /// </summary>
        public ApplicationDbContext()
           : base("DefaultConnection")
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
        /// DbSet to hold all Parking Slots
        /// </summary>
        public DbSet<ParkingSlot> ParkingSlots { get; set; }

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