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

namespace ParkEasyV1.Controllers
{   

    public class BackendController : Controller
    {       

        public ActionResult Scheduler()
        {
            return new Dps().CallBack(this);
        }

    }

    class Dps : DayPilotScheduler
    {
        //instance of DBContext
        private ApplicationDbContext db = new ApplicationDbContext();

        protected void OnInit(InitArgs e)
        {
            // load resources
            Resources.Add("Resource 1", "R1");
            Resources.Add("Resource 2", "R2");

            // load events
            //Events = new DataManager().GetData();
            //DataIdField = "Id";
            //DataTextField = "Text";
            //DataResourceField = "ResourceId";
            //DataStartField = "Start";
            //DataEndField = "End";

            foreach (var booking in db.Bookings.ToList())
            {
                DataIdField = booking.ID.ToString();
                DataTextField = booking.User.FirstName.ToString();
                DataStartField = booking.Flight.DepartureDate.ToString();
                DataEndField = booking.Flight.ReturnDate.ToString();
            }


            // request a full update (resources and events)
            Update(CallBackUpdateType.Full);
        }

        protected void OnEventMove(EventMoveArgs e)
        {
            //new DataManager().MoveEvent(e.Id, e.NewStart, e.NewEnd, e.NewResource);
        }
    }
}