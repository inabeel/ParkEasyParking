using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using ParkEasyV1.Models;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller for handling all Tariff events or actions
    /// </summary>
    public class TariffsController : Controller
    {
        /// <summary>
        /// Global instance of the ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tariffs
        public ActionResult Index()
        {
            return View(db.Tariffs.ToList());
        }

        /// <summary>
        /// HttpGet ActionResult for returning the manage tariffs view
        /// </summary>
        /// <returns></returns>
        // GET: Tariffs/Manage
        public ActionResult Manage()
        {
            //loop through users in database
            foreach (var user in db.Users.ToList())
            {
                //if user email matches the current logged in user
                if (user.Email.Equals(User.Identity.Name))
                {
                    //store the user id in viewbag to be displayed on front-end
                    ViewBag.UserID = user.Id;
                }
            }
            //return the manage tariffs view with a list of tariffs from database
            return View(db.Tariffs.ToList());
        }

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
        /// HttpGet ActionResult for returning a view to create a new tariff
        /// </summary>
        /// <returns></returns>
        // GET: Tariffs/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// HttpPost ActionResult for creating a new tariff
        /// </summary>
        /// <param name="tariff"></param>
        /// <returns></returns>
        // POST: Tariffs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Type,Amount")] Tariff tariff)
        {
            //check model state is valid
            if (ModelState.IsValid)
            {
                //add the tariff to the database and save database changes
                db.Tariffs.Add(tariff);
                db.SaveChanges();
                //return Manage tariffs view
                return RedirectToAction("Manage");
            }

            //if model state is not valid return the create tariff view with model attached
            return View(tariff);
        }

        /// <summary>
        /// HttpGet ActionResult for returning the edit tariff view
        /// </summary>
        /// <param name="id">tariff id</param>
        /// <returns>edit tariff view</returns>
        // GET: Tariffs/Edit/5
        public ActionResult Edit(int? id)
        {
            //if the tariff id is null
            if (id == null)
            {
                //return BadRequest StatusCode
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //find the tariff in the database using id parameter
            Tariff tariff = db.Tariffs.Find(id);
            //if the tariff is null
            if (tariff == null)
            {
                //return http not found error
                return HttpNotFound();
            }
            //return the edit view with the tariff
            return View(tariff);
        }

        /// <summary>
        /// HttpPost ActionResult for updating a tariff
        /// </summary>
        /// <param name="tariff">Updated tariff</param>
        /// <returns>Manage View</returns>
        // POST: Tariffs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Type,Amount")] Tariff tariff)
        {
            //if the model state is valid
            if (ModelState.IsValid)
            {
                //update the tariff in the database and save changes
                db.Entry(tariff).State = EntityState.Modified;
                db.SaveChanges();
                //return the manage tariff view
                return RedirectToAction("Manage");
            }
            //if model state is not valid - return the edit tariff view with model
            return View(tariff);
        }

        /// <summary>
        /// HttpGet ActionResult for returning the delete tariff view
        /// </summary>
        /// <param name="id">nullable tariff id</param>
        /// <returns>delete view</returns>
        // GET: Tariffs/Delete/5
        public ActionResult Delete(int? id)
        {
            //if the tariff id is null
            if (id == null)
            {
                //return bad request error
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //find the tariff in the database
            Tariff tariff = db.Tariffs.Find(id);
            //if the tariff is null
            if (tariff == null)
            {
                //return not found
                return HttpNotFound();
            }
            //return the view with tariff
            return View(tariff);
        }

        /// <summary>
        /// HttpPost ActionResult for deleting a tariff
        /// </summary>
        /// <param name="id">tariff id</param>
        /// <returns>Manage view</returns>
        // POST: Tariffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //find the tariff in the database using the id parameter
            Tariff tariff = db.Tariffs.Find(id);
            db.Tariffs.Remove(tariff);  //remove the tariff from the database
            db.SaveChanges();   //save database changes
            return RedirectToAction("Manage");   //return manage tariff view
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
