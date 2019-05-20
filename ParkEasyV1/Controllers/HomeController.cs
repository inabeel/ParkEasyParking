using ParkEasyV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ParkEasyV1.Controllers
{
    /// <summary>
    /// Controller for handling home actions
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Instance of ApplicationDbContext
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// ActionResult to return Index view
        /// </summary>
        /// <returns>Index view</returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        /// <summary>
        /// ActionResult to return About view
        /// </summary>
        /// <returns>About view</returns>
        public ActionResult About()
        {

            return View();
        }

        /// <summary>
        /// ActionResult to return Contact view
        /// </summary>
        /// <returns>Contact view</returns>
        public ActionResult Contact()
        {

            return View();
        }

        /// <summary>
        /// ActionResult to return Prices view
        /// </summary>
        /// <returns>Prices view</returns>
        public ActionResult Prices()
        {
            //get tarrifs from db
            var tariffs = db.Tariffs.ToList();

            //loop through tariffs and get current prices
            foreach (var tariff in tariffs)
            {
                if (tariff.Type.Equals("Parking Slot"))
                {
                    ViewBag.SlotPrice = tariff.Amount*8;
                }

                if (tariff.Type.Equals("Mini Valet"))
                {
                    ViewBag.MiniValetPrice = tariff.Amount;
                }

                if (tariff.Type.Equals("Full Valet"))
                {
                    ViewBag.FullValetPrice = tariff.Amount;
                }
            }

            return View();
        }

        /// <summary>
        /// ActionResult to return Services view
        /// </summary>
        /// <returns>Services view</returns>
        public ActionResult Services()
        {

            return View();
        }

        /// <summary>
        /// ActionResult to return Terms of Service view
        /// </summary>
        /// <returns>Terms of service view</returns>
        public ActionResult ToS()
        {

            return View();
        }

        /// <summary>
        /// ActionResult to return Accessiblity view
        /// </summary>
        /// <returns>Accessiblity view</returns>
        public ActionResult Accessibility()
        {

            return View();
        }

        /// <summary>
        /// ActionResult to return Sitemap view
        /// </summary>
        /// <returns>Sitemap view</returns>
        public ActionResult Sitemap()
        {

            return View();
        }

        /// <summary>
        /// ActionResult to return FAQs view
        /// </summary>
        /// <returns>FAQs view</returns>
        public ActionResult FAQs()
        {
            //get tariffs from db
            var tariffs = db.Tariffs.ToList();

            //loop through tariffs and get valet prices
            foreach (var tariff in tariffs)
            {
                if (tariff.Type.Equals("Mini Valet"))
                {
                    ViewBag.MiniValetPrice = tariff.Amount;
                }

                if (tariff.Type.Equals("Full Valet"))
                {
                    ViewBag.FullValetPrice = tariff.Amount;
                }
            }

            return View();
        }
    }
}