using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ParkEasyV1.Models
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {        
        public ApplicationDbContext()
           : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DatabaseInitialiser());
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<BookingLine> BookingLines { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }        
    }
}