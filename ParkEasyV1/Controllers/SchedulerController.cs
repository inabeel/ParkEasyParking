using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Events.Calendar;
using ParkEasyV1.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DayPilot.Web.Mvc.Enums;
using DHTMLX.Scheduler;
using DHTMLX.Scheduler.Data;

namespace ParkEasyV1.Controllers
{   

    public class SchedulerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            //Being initialized in that way, scheduler will use CalendarController.Data as a the datasource and CalendarController. Save to process the changes  
            var scheduler = new DHXScheduler(this);
            scheduler.DataAction = "Data";

            /* 
             * It's possible to use different actions of the current controller 
             *      var scheduler = new DHXScheduler(this);      
             *      scheduler.DataAction = "ActionName1"; 
             *      scheduler.SaveAction = "ActionName2"; 
             *  
             * Or to specify full paths 
             *      var scheduler = new DHXScheduler(); 
             *      scheduler.DataAction = Url.Action("Data", "Calendar"); 
             *      scheduler.SaveAction = Url.Action("Save", "Calendar"); 
             */

            /* 
             * The default codebase folder is ~/Scripts/dhtmlxScheduler. It can be overriden: 
             *      scheduler.Codebase = Url.Content("~/customCodebaseFolder"); 
             */


            scheduler.InitialDate = new DateTime(2019, 05, 21);

            scheduler.LoadData = true;
            scheduler.EnableDataprocessor = true;

            return View(scheduler);
        }

        public ContentResult Data()
        {
            try
            {
                var details = db.Bookings.ToList();



                return new SchedulerAjaxData(details);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}