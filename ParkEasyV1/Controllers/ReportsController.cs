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
    /// <summary>
    /// Controller for handling all analytic report events and actions
    /// </summary>
    [Authorize(Roles ="Admin, Manager")]
    public class ReportsController : Controller
    {
        /// <summary>
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult to return the Reports index view
        /// </summary>
        /// <returns>Reports index view</returns>
        // GET: Reports
        public ActionResult Index()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //Find the current logged in User's ID and store in viewbag to be displayed on the front-end
            ViewBag.UserID = userManager.FindByEmail(User.Identity.GetUserName()).Id;

            //return Index view
            return View();
        }

        /// <summary>
        /// HttpGet ActionResult to return the Daily Bookings Report View
        /// </summary>
        /// <returns>Daily Bookings Report View</returns>
        public ActionResult DailyBookings()
        {
            try
            {
                //get list of all bookings where the booking departure date is today
                List<Booking> dailyBookings = db.Bookings.Where(b => b.Flight.DepartureDate.Equals(DateTime.Today)).ToList();

                //return the Daily Bookings View with list of bookings
                return View(dailyBookings);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the Daily Cars Release Report
        /// </summary>
        /// <returns>Daily Cars Release Report</returns>
        public ActionResult DailyCarsRelease()
        {
            try
            {
                //get a list of all bookings that are due to return today
                List<Booking> dailyCarsRelease = db.Bookings.Where(b => b.Flight.ReturnDate.Equals(DateTime.Today)).ToList();

                //return the daily cars release report view
                return View(dailyCarsRelease);
            }
            catch (Exception ex)
            {
                //if exception occurs, return httpnotfound
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the daily valeting cars report
        /// </summary>
        /// <returns>Daily Valeting Cars Report View</returns>
        public ActionResult DailyValetingCars()
        {
            try
            {
                //initialize a list to store the bookings with cars due to be valeting
                List<Booking> dailyValetingCars = new List<Booking>();

                //loop through all bookings
                foreach (var booking in db.Bookings.ToList())
                {
                    //if the booking has checked in
                    //has NOT checked out (meaning it is still currently parked)
                    //has booked with the valet service
                    if (booking.CheckedIn==true && booking.CheckedOut==false && booking.ValetService==true)
                    {
                        //add the booking to the daily valeting cars list
                        dailyValetingCars.Add(booking);
                    }
                }
                //return the daily valeting cars report with the list of daily valeting cars
                return View(dailyValetingCars);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the monthly bookings report view
        /// </summary>
        /// <returns>Monthly Bookings Report View</returns>
        public ActionResult Bookings()
        {
            try
            {
                //initialize a list of bookings
                List<Booking> monthlyBookings = new List<Booking>();

                //initialize total income
                double totalIncome = 0;

                //loop through all bookings 
                foreach (var booking in db.Bookings.ToList())
                {
                    //if the booking was booked during the current month
                    if (booking.DateBooked.Month.Equals(DateTime.Today.Month))
                    {
                        //add the booking to the monthly booking list and increase the total income variable
                        monthlyBookings.Add(booking);
                        totalIncome += booking.Total;
                    }
                }

                //store the total income in a view bag to be displayed on the front-end
                ViewBag.Total = totalIncome;

                //return the monthly booking report with the list of bookings
                return View(monthlyBookings);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

        /// <summary>
        /// HttpGet ActionResult to return the turnover report view
        /// </summary>
        /// <returns>Turnover Report view</returns>
        public ActionResult Turnover()
        {
            try
            {
                //initialize a list of bookings
                List<Booking> turnoverBookings = new List<Booking>();

                //initialize the total income variable
                double totalIncome = 0;

                //loop through bookings 
                foreach (var booking in db.Bookings.ToList())
                {
                    //if the booking was booking this month
                    if (booking.DateBooked.Month.Equals(DateTime.Today.Month))
                    {
                        //add the booking to the list of bookings and increase the total income
                        turnoverBookings.Add(booking);
                        totalIncome += booking.Total;
                    }
                }
                //store total income in a viewbag to be displayed on the front-end
                ViewBag.Total = totalIncome;

                //return the turnover bookings view with the list of bookings
                return View(turnoverBookings);
            }
            catch (Exception ex)
            {
                //if exception occurs, throw httpnotfound error
                return HttpNotFound();
            }
            
        }

    }
}