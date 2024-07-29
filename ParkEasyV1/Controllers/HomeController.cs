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
        /// ActionResult to return Index view
        /// </summary>
        /// <returns>Index view</returns>
        public ActionResult Index()
        {
            return View();
        }
    }
}