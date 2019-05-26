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
    /// Controller to handle all Vehicle events and actions
    /// </summary>
    public class VehiclesController : Controller
    {
        /// <summary>
        /// Global instance of applicationdbcontext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult to return Vehicles index view
        /// </summary>
        /// <returns>Index view</returns>
        // GET: Vehicles
        public ActionResult Index()
        {
            return View(db.Vehicles.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return Vehicles Details view
        /// </summary>
        /// <param name="id">Vehicle id</param>
        /// <returns>Details view</returns>
        // GET: Vehicles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        /// <summary>
        /// HttpGet ActionResult to return Vehicles Create view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: Vehicles/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to create a new vehicle
        /// </summary>
        /// <param name="vehicle">Created vehicle</param>
        /// <returns>Vehicles index</returns>
        // POST: Vehicles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RegistrationNumber,Make,Model,Colour,NoOfPassengers")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                db.Vehicles.Add(vehicle);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vehicle);
        }

        /// <summary>
        /// HttpGet ActionResult to return Vehicles Edit view
        /// </summary>
        /// <param name="id">Vehicle id</param>
        /// <returns>Edit view</returns>
        // GET: Vehicles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        /// <summary>
        /// HttpPost ActionResult to update a Vehicle
        /// </summary>
        /// <param name="vehicle">Updated vehicle</param>
        /// <returns>Index view</returns>
        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RegistrationNumber,Make,Model,Colour,NoOfPassengers")] Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(vehicle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vehicle);
        }

        /// <summary>
        /// HttpGet ActionResult to return the Vehicles Delete view
        /// </summary>
        /// <param name="id">Vehicle id</param>
        /// <returns>Delete view</returns>
        // GET: Vehicles/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vehicle vehicle = db.Vehicles.Find(id);
            if (vehicle == null)
            {
                return HttpNotFound();
            }
            return View(vehicle);
        }

        /// <summary>
        /// HttpPost ActionResult to delete a vehicle
        /// </summary>
        /// <param name="id">Vehicle id</param>
        /// <returns>Vehicle Index view</returns>
        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vehicle vehicle = db.Vehicles.Find(id);
            db.Vehicles.Remove(vehicle);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Method to release all unused resources
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
