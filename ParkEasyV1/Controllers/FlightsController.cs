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
    /// Controller to handle all flight events and actions
    /// </summary>
    public class FlightsController : Controller
    {
        //global instance of the ApplicationDbContext
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult to return the flight index view
        /// </summary>
        /// <returns>flight index view</returns>
        // GET: Flights
        public ActionResult Index()
        {
            return View(db.Flights.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return the flight details view
        /// </summary>
        /// <param name="id">flight id</param>
        /// <returns>details view</returns>
        // GET: Flights/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flight flight = db.Flights.Find(id);
            if (flight == null)
            {
                return HttpNotFound();
            }
            return View(flight);
        }

        /// <summary>
        /// HttpGet ActionResult to return the Flight Create view
        /// </summary>
        /// <returns>create view</returns>
        // GET: Flights/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to create a new flight
        /// </summary>
        /// <param name="flight">Created Flight</param>
        /// <returns>Flight Index</returns>
        // POST: Flights/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,DepartureFlightNo,DepartureTime,ReturnFlightNo,ReturnFlightTime,DepartureDate,ReturnDate,DestinationAirport")] Flight flight)
        {
            if (ModelState.IsValid)
            {
                db.Flights.Add(flight);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(flight);
        }

        /// <summary>
        /// HttpGet ActionResult to return the Flight Edit View
        /// </summary>
        /// <param name="id">Flight id</param>
        /// <returns>Flight Edit View</returns>
        // GET: Flights/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flight flight = db.Flights.Find(id);
            if (flight == null)
            {
                return HttpNotFound();
            }
            return View(flight);
        }

        /// <summary>
        /// HttpPost ActionResult to update a Flight
        /// </summary>
        /// <param name="flight">updated Flight</param>
        /// <returns>Flight index</returns>
        // POST: Flights/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,DepartureFlightNo,DepartureTime,ReturnFlightNo,ReturnFlightTime,DepartureDate,ReturnDate,DestinationAirport")] Flight flight)
        {
            if (ModelState.IsValid)
            {
                db.Entry(flight).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(flight);
        }

        /// <summary>
        /// HttpGet ActionResult to return the Flight Delete view
        /// </summary>
        /// <param name="id">flight id</param>
        /// <returns>Flight Delete View</returns>
        // GET: Flights/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Flight flight = db.Flights.Find(id);
            if (flight == null)
            {
                return HttpNotFound();
            }
            return View(flight);
        }

        /// <summary>
        /// HttpPost ActionResult to delete a Flight
        /// </summary>
        /// <param name="id">Flight ID</param>
        /// <returns>Flight index</returns>
        // POST: Flights/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Flight flight = db.Flights.Find(id);
            db.Flights.Remove(flight);
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
