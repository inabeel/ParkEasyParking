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
    /// Controller for handling all Tariff management and events/actions
    /// </summary>
    [Authorize(Roles ="Admin, Manager")]
    public class TariffsController : Controller
    {
        /// <summary>
        /// Global instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// HttpGet ActionResult to return the Tariff Index view
        /// </summary>
        /// <returns>Index view</returns>
        // GET: Tariffs
        public ActionResult Index()
        {
            return View(db.Tariffs.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return the Manage Tariffs View
        /// </summary>
        /// <returns>Manage view</returns>
        // GET: Tariffs/Manage
        public ActionResult Manage()
        {
            //declare instance of usermanager
            UserManager<User> userManager = new UserManager<User>(new UserStore<User>(db));

            //get the current user's ID and store in viewbag for front-end display
            ViewBag.UserID = userManager.FindByEmail(User.Identity.GetUserName()).Id;

            return View(db.Tariffs.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult to return the Details view
        /// </summary>
        /// <param name="id">Tariff id</param>
        /// <returns>Details view</returns>
        // GET: Tariffs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tariff tariff = db.Tariffs.Find(id);
            if (tariff == null)
            {
                return HttpNotFound();
            }
            return View(tariff);
        }

        /// <summary>
        /// HttpGet ActionResult to return the Create view
        /// </summary>
        /// <returns>Create view</returns>
        // GET: Tariffs/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult to create a new tariff
        /// </summary>
        /// <param name="tariff">Created Tariff</param>
        /// <returns>Manage view</returns>
        // POST: Tariffs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Type,Amount")] Tariff tariff)
        {
            if (ModelState.IsValid)
            {
                db.Tariffs.Add(tariff);
                db.SaveChanges();
                return RedirectToAction("Manage");
            }

            return View(tariff);
        }

        /// <summary>
        /// HttpGet ActionResult to return the edit tariff view
        /// </summary>
        /// <param name="id">Tariff id</param>
        /// <returns>Edit view</returns>
        // GET: Tariffs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tariff tariff = db.Tariffs.Find(id);
            if (tariff == null)
            {
                return HttpNotFound();
            }
            return View(tariff);
        }

        /// <summary>
        /// HttpPost ActionResult to update a tariff
        /// </summary>
        /// <param name="tariff">Updated tariff</param>
        /// <returns>Manage view</returns>
        // POST: Tariffs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Type,Amount")] Tariff tariff)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(tariff).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Manage");
                }
                return View(tariff);
            }
            catch (Exception ex)
            {
                //if exception occurs, return manage view with error message
                TempData["Error"] = "Error: An error occured while updating tariff.";
                return RedirectToAction("Manage");
            }            
        }

        /// <summary>
        /// HttpGet ActionResult to return the delete tariff view
        /// </summary>
        /// <param name="id">Tariff id</param>
        /// <returns>Delete view</returns>
        // GET: Tariffs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tariff tariff = db.Tariffs.Find(id);
            if (tariff == null)
            {
                return HttpNotFound();
            }
            return View(tariff);
        }

        /// <summary>
        /// HttpPost ActionResult to delete a tariff
        /// </summary>
        /// <param name="id">Tariff Id</param>
        /// <returns>Manage view</returns>
        // POST: Tariffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Tariff tariff = db.Tariffs.Find(id);
                db.Tariffs.Remove(tariff);
                db.SaveChanges();
                return RedirectToAction("Manage");
            }
            catch (Exception ex)
            {
                //if exception occurs, return manage view with error message
                TempData["Error"] = "An error occured while deleting tariff";
                return RedirectToAction("Manage");
            }
            
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
