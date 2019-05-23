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
using DHTMLX.Common;

namespace ParkEasyV1.Controllers
{

    public class SchedulerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var sched = new DHXScheduler(this);
            sched.Skin = DHXScheduler.Skins.Terrace;
            sched.LoadData = true;
            sched.EnableDataprocessor = true;
            sched.InitialDate = new DateTime(2019, 05, 22);
            sched.DataAction = "Data";
            return View(sched);
        }

        public ContentResult Data()
        {
            return (new SchedulerAjaxData(
                 new ApplicationDbContext().Bookings.
                 Select(b => new { b.ID, b.User.FirstName, b.Flight.DepartureDate, b.Flight.ReturnDate })
                 ));
        }

        public ContentResult Save(int? id, FormCollection actionValues)
        {
            var action = new DataAction(actionValues);
            var changedEvent = DHXEventsHelper.Bind<Booking>(actionValues);
            var entities = new ApplicationDbContext();
            try
            {
                switch (action.Type)
                {
                    case DataActionTypes.Insert:
                        entities.Bookings.Add(changedEvent);
                        break;
                    case DataActionTypes.Delete:
                        changedEvent = entities.Bookings.FirstOrDefault(ev => ev.ID == action.SourceId);
                        entities.Bookings.Remove(changedEvent);
                        break;
                    default:// "update"
                        var target = entities.Bookings.Single(e => e.ID == changedEvent.ID);
                        DHXEventsHelper.Update(target, changedEvent, new List<string> { "ID" });
                        break;
                }
                entities.SaveChanges();
                action.TargetId = changedEvent.ID;
            }
            catch (Exception a)
            {
                action.Type = DataActionTypes.Error;
            }

       return (new AjaxSaveResponse(action));
        }
    }
}