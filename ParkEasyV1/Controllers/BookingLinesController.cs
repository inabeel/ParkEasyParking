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
    public class BookingLinesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: BookingLines
        public ActionResult Index()
        {
            var bookingLines = db.BookingLines.Include(b => b.Booking).Include(b => b.Vehicle);
            return View(bookingLines.ToList());
        }

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

        // GET: BookingLines/Create
        public ActionResult Create()
        {
            ViewBag.BookingID = new SelectList(db.Bookings, "ID", "UserID");
            ViewBag.VehicleID = new SelectList(db.Vehicles, "ID", "RegistrationNumber");
            return View();
        }

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
