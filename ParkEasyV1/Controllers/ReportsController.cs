using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ParkEasyV1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ParkEasyV1.Controllers
{
    public class ReportsController : Controller
    {
        /// <summary>
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Reports
        public ActionResult Index()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            ViewBag.UserID = userManager.FindByEmail(User.Identity.GetUserName()).Id;

            return View();
        }

        public ActionResult DailyBookings()
        {
            List<Booking> dailyBookings = db.Bookings.Where(b => b.Flight.DepartureDate.Equals(DateTime.Today)).ToList();

            return View(dailyBookings);
        }

        public ActionResult DailyCarsRelease()
        {
            List<Booking> dailyCarsRelease = db.Bookings.Where(b => b.Flight.ReturnDate.Equals(DateTime.Today)).ToList();

            return View(dailyCarsRelease);
        }

        public ActionResult DailyValetingCars()
        {
            List<Booking> dailyValetingCars = new List<Booking>();

            foreach (var booking in db.Bookings.ToList())
            {
                if (booking.CheckedIn==true && booking.CheckedOut==false && booking.ValetService==true)
                {
                    dailyValetingCars.Add(booking);
                }
            }

            return View(dailyValetingCars);
        }

        public ActionResult Bookings()
        {
            List<Booking> monthlyBookings = new List<Booking>();

            double totalIncome = 0;

            foreach (var booking in db.Bookings.ToList())
            {
                if (booking.DateBooked.Month.Equals(DateTime.Today.Month))
                {
                    monthlyBookings.Add(booking);
                    totalIncome += booking.Total;
                }
            }

            ViewBag.Total = totalIncome;

            return View(monthlyBookings);
        }

        public ActionResult Turnover()
        {
            List<Booking> turnoverBookings = new List<Booking>();

            double totalIncome = 0;

            foreach (var booking in db.Bookings.ToList())
            {
                if (booking.DateBooked.Month.Equals(DateTime.Today.Month))
                {
                    turnoverBookings.Add(booking);
                    totalIncome += booking.Total;
                }
            }

            ViewBag.Total = totalIncome;

            return View(turnoverBookings);
        }

    }
}