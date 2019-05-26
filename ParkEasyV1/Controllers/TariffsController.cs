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
    [Authorize(Roles ="Admin, Manager")]
    public class TariffsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tariffs
        public ActionResult Index()
        {
            return View(db.Tariffs.ToList());
        }

        // GET: Tariffs/Manage
        public ActionResult Manage()
        {
            foreach (var user in db.Users.ToList())
            {
                if (user.Email.Equals(User.Identity.Name))
                {
                    ViewBag.UserID = user.Id;
                }
            }

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

        // GET: Tariffs/Create
        public ActionResult Create()
        {
            return View();
        }

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
                return RedirectToAction("Index");
            }

            return View(tariff);
        }

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

        // POST: Tariffs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Type,Amount")] Tariff tariff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tariff).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tariff);
        }

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

        // POST: Tariffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tariff tariff = db.Tariffs.Find(id);
            db.Tariffs.Remove(tariff);
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
