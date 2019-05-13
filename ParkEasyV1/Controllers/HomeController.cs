using ParkEasyV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ParkEasyV1.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {

            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }

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

        public ActionResult Services()
        {

            return View();
        }

        public ActionResult ToS()
        {

            return View();
        }

        public ActionResult Accessibility()
        {

            return View();
        }

        public ActionResult Sitemap()
        {

            return View();
        }

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

        public ActionResult Test()
        {
            return View();
        }
    }
}