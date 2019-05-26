using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ParkEasyV1.Models;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller to handle all Parking Slot actions and events
    /// </summary>
    public class ParkingSlotsController : Controller
    {
        /// <summary>
        /// Global instance of the ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult to return the ParkingSlot Index view
        /// </summary>
        /// <returns>Index view</returns>
        // GET: ParkingSlots
        public ActionResult Index()
        {
            //create instance of the usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the current logged in user 
            User loggedInUser = userManager.FindByEmail(User.Identity.GetUserName());

            //store user id in viewbag for front-end display
            ViewBag.UserID = loggedInUser.Id;

            //create a list to hold active bookings in parking slots
            List<Booking> activeBookings = new List<Booking>();

            //loop through all parking slots
            foreach (var slot in db.ParkingSlots.ToList())
            {
                //loop through all bookings associated with the parking slot
                foreach (var booking in slot.Bookings)
                {
                    //if the booking has been checked in and NOT checked out
                    //the booking is currently an active booking
                    if (booking.CheckedIn == true && booking.CheckedOut==false)
                    {
                        //add the booking to the list
                        activeBookings.Add(booking);
                    }
                }
            }

            //store the ActiveBookings in a TempData to be used on the front-end
            TempData["ActiveBookings"] = activeBookings;

            //return the full list of parking slots
            return View(db.ParkingSlots.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return the Details view
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>Details view</returns>
        // GET: ParkingSlots/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            if (parkingSlot == null)
            {
                return HttpNotFound();
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpGet ActionResult to return the create parking slot view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: ParkingSlots/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to create a parking slot
        /// </summary>
        /// <param name="parkingSlot">Created parking slot</param>
        /// <returns>Parking slot index view</returns>
        // POST: ParkingSlots/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Status")] ParkingSlot parkingSlot)
        {
            if (ModelState.IsValid)
            {
                db.ParkingSlots.Add(parkingSlot);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(parkingSlot);
        }

        /// <summary>
        /// HttpGet ActionResult to return the edit parking slot view
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>Edit view</returns>
        // GET: ParkingSlots/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            if (parkingSlot == null)
            {
                return HttpNotFound();
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpPost ActionResult to update a parking slot
        /// </summary>
        /// <param name="parkingSlot">Updated parking slot</param>
        /// <returns>Index view</returns>
        // POST: ParkingSlots/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Status")] ParkingSlot parkingSlot)
        {
            if (ModelState.IsValid)
            {
                db.Entry(parkingSlot).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpGet ActionResult to return the delete parking slot view
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>Delete view</returns>
        // GET: ParkingSlots/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            if (parkingSlot == null)
            {
                return HttpNotFound();
            }
            return View(parkingSlot);
        }

        /// <summary>
        /// HttpPost ActionResult to delete a parking slot
        /// </summary>
        /// <param name="id">Parking slot id</param>
        /// <returns>ParkingSlot index</returns>
        // POST: ParkingSlots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ParkingSlot parkingSlot = db.ParkingSlots.Find(id);
            db.ParkingSlots.Remove(parkingSlot);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Method to release unused resources
        /// </summary>
        /// <param name="disposing"></param>
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
