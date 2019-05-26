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
    public class ParkingSlotsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ParkingSlots
        public ActionResult Index()
        {
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            User loggedInUser = userManager.FindByEmail(User.Identity.GetUserName());

            ViewBag.UserID = loggedInUser.Id;

            List<Booking> activeBookings = new List<Booking>();

            foreach (var slot in db.ParkingSlots.ToList())
            {
                foreach (var booking in slot.Bookings)
                {
                    if (booking.CheckedIn == true && booking.CheckedOut==false)
                    {
                        activeBookings.Add(booking);
                    }
                }
            }

            TempData["ActiveBookings"] = activeBookings;

            return View(db.ParkingSlots.ToList());
        }

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

        // GET: ParkingSlots/Create
        public ActionResult Create()
        {
            return View();
        }

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
