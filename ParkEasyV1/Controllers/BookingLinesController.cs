using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ParkEasyV1.Models;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller to handle any booking line events or actions
    /// </summary>
    public class BookingLinesController : Controller
    {
        /// <summary>
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult to return the index view
        /// </summary>
        /// <returns>Index view</returns>
        // GET: BookingLines
        public ActionResult Index()
        {
            var bookingLines = db.BookingLines.Include(b => b.Booking).Include(b => b.Vehicle);
            return View(bookingLines.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return the details of a booking line
        /// </summary>
        /// <param name="id">booking line id</param>
        /// <returns>booking line details view</returns>
        // GET: BookingLines/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookingLine bookingLine = db.BookingLines.Find(id);
            if (bookingLine == null)
            {
                return HttpNotFound();
            }
            return View(bookingLine);
        }

        /// <summary>
        /// HttpGet ActionResult for returning the create booking line view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: BookingLines/Create
        public ActionResult Create()
        {
            ViewBag.BookingID = new SelectList(db.Bookings, "ID", "UserID");
            ViewBag.VehicleID = new SelectList(db.Vehicles, "ID", "RegistrationNumber");
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to create a booking line
        /// </summary>
        /// <param name="bookingLine">created bookingline</param>
        /// <returns>booking line index</returns>
        // POST: BookingLines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,BookingID,VehicleID")] BookingLine bookingLine)
        {
            if (ModelState.IsValid)
            {
                db.BookingLines.Add(bookingLine);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookingID = new SelectList(db.Bookings, "ID", "UserID", bookingLine.BookingID);
            ViewBag.VehicleID = new SelectList(db.Vehicles, "ID", "RegistrationNumber", bookingLine.VehicleID);
            return View(bookingLine);
        }

        /// <summary>
        /// HttpGet ActionResult to return the edit booking line view
        /// </summary>
        /// <param name="id">booking line id</param>
        /// <returns>Edit booking line view</returns>
        // GET: BookingLines/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookingLine bookingLine = db.BookingLines.Find(id);
            if (bookingLine == null)
            {
                return HttpNotFound();
            }
            ViewBag.BookingID = new SelectList(db.Bookings, "ID", "UserID", bookingLine.BookingID);
            ViewBag.VehicleID = new SelectList(db.Vehicles, "ID", "RegistrationNumber", bookingLine.VehicleID);
            return View(bookingLine);
        }

        /// <summary>
        /// HttpPost ActionResult for updating the booking line for any edits made
        /// </summary>
        /// <param name="bookingLine">edited booking line</param>
        /// <returns>Booking line index</returns>
        // POST: BookingLines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,BookingID,VehicleID")] BookingLine bookingLine)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bookingLine).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BookingID = new SelectList(db.Bookings, "ID", "UserID", bookingLine.BookingID);
            ViewBag.VehicleID = new SelectList(db.Vehicles, "ID", "RegistrationNumber", bookingLine.VehicleID);
            return View(bookingLine);
        }

        /// <summary>
        /// HttpGet ActionResult for returning the Delete booking line view
        /// </summary>
        /// <param name="id">Booking line id</param>
        /// <returns>Delete view</returns>
        // GET: BookingLines/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BookingLine bookingLine = db.BookingLines.Find(id);
            if (bookingLine == null)
            {
                return HttpNotFound();
            }
            return View(bookingLine);
        }

        /// <summary>
        /// HttpPost ActionResult for deleting a booking line
        /// </summary>
        /// <param name="id">Booking line id</param>
        /// <returns>Booking line index</returns>
        // POST: BookingLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BookingLine bookingLine = db.BookingLines.Find(id);
            db.BookingLines.Remove(bookingLine);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Method for releasing unused resources
        /// </summary>
        /// <param name="disposing">boolean to dispose</param>
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
